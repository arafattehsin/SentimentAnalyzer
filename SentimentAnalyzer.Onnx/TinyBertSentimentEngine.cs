using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SentimentAnalyzer.Onnx
{
    /// <summary>
    /// TinyBERT-based sentiment engine using ONNX Runtime.
    /// Lightweight model (7.5x smaller than BERT) with binary classification.
    /// Model is embedded - no downloads required.
    /// </summary>
    public sealed class TinyBertSentimentEngine : ISentimentEngine, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly SentimentAnalyzerOptions _options;
        private readonly WordPieceTokenizer _tokenizer;
        private bool _disposed;
        private static readonly object _initLock = new object();
        private static string? _cachedModelPath;

        /// <inheritdoc/>
        public string Name => "TinyBert";

        /// <summary>
        /// Creates a new TinyBERT sentiment engine with embedded model.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        public TinyBertSentimentEngine(SentimentAnalyzerOptions? options = null)
        {
            _options = options ?? SentimentAnalyzerOptions.Default;
            var modelPath = ExtractEmbeddedModel();
            _session = new InferenceSession(modelPath);
            _tokenizer = new WordPieceTokenizer();
        }

        /// <summary>
        /// Creates a new TinyBERT sentiment engine with session options.
        /// </summary>
        /// <param name="sessionOptions">ONNX Runtime session options.</param>
        /// <param name="options">Configuration options.</param>
        public TinyBertSentimentEngine(SessionOptions sessionOptions, SentimentAnalyzerOptions? options = null)
        {
            if (sessionOptions == null)
                throw new ArgumentNullException(nameof(sessionOptions));

            _options = options ?? SentimentAnalyzerOptions.Default;
            var modelPath = ExtractEmbeddedModel();
            _session = new InferenceSession(modelPath, sessionOptions);
            _tokenizer = new WordPieceTokenizer();
        }

        private static string ExtractEmbeddedModel()
        {
            lock (_initLock)
            {
                if (_cachedModelPath != null && File.Exists(_cachedModelPath))
                    return _cachedModelPath;

                var assembly = typeof(TinyBertSentimentEngine).Assembly;
                var tempDir = Path.Combine(Path.GetTempPath(), "SentimentAnalyzer", "TinyBert");
                Directory.CreateDirectory(tempDir);

                var modelPath = Path.Combine(tempDir, "tinybert-sentiment.onnx");
                var vocabPath = Path.Combine(tempDir, "vocab.txt");

                // Extract model if not already extracted
                if (!File.Exists(modelPath))
                {
                    using var modelStream = assembly.GetManifestResourceStream("tinybert-sentiment.onnx");
                    if (modelStream == null)
                        throw new InvalidOperationException("TinyBERT model not found in embedded resources.");

                    using var fileStream = File.Create(modelPath);
                    modelStream.CopyTo(fileStream);
                }

                // Extract vocab if not already extracted
                if (!File.Exists(vocabPath))
                {
                    using var vocabStream = assembly.GetManifestResourceStream("vocab.txt");
                    if (vocabStream == null)
                        throw new InvalidOperationException("Vocabulary file not found in embedded resources.");

                    using var fileStream = File.Create(vocabPath);
                    vocabStream.CopyTo(fileStream);
                }

                _cachedModelPath = modelPath;
                return modelPath;
            }
        }

        /// <inheritdoc/>
        public SentimentResult Analyze(string text)
        {
            ThrowIfDisposed();
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var (inputIds, attentionMask) = _tokenizer.Encode(text, maxLength: 512);

            // Create token_type_ids (all zeros for single-sequence classification)
            var tokenTypeIds = new int[inputIds.Length];

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", CreateTensor(inputIds)),
                NamedOnnxValue.CreateFromTensor("attention_mask", CreateTensor(attentionMask)),
                NamedOnnxValue.CreateFromTensor("token_type_ids", CreateTensor(tokenTypeIds))
            };

            using var results = _session.Run(inputs);
            var logits = results.First().AsTensor<float>();

            // Apply softmax to get probabilities
            var probabilities = Softmax(logits.ToArray());

            // TinyBERT outputs: [negative, positive] (binary classification)
            var (label, confidence, score) = InterpretProbabilities(probabilities);

            return new SentimentResult(text, label, score, confidence, Name);
        }

        /// <inheritdoc/>
        public IReadOnlyList<SentimentResult> AnalyzeBatch(IEnumerable<string> texts)
        {
            ThrowIfDisposed();
            if (texts == null)
                throw new ArgumentNullException(nameof(texts));

            var results = new List<SentimentResult>();
            foreach (var text in texts)
            {
                if (text != null)
                    results.Add(Analyze(text));
            }
            return results;
        }

        private void ThrowIfDisposed()
        {
#if NETSTANDARD2_0
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
#else
            ObjectDisposedException.ThrowIf(_disposed, this);
#endif
        }

        private static DenseTensor<long> CreateTensor(int[] values)
        {
            var tensor = new DenseTensor<long>(new[] { 1, values.Length });
            for (int i = 0; i < values.Length; i++)
            {
                tensor[0, i] = values[i];
            }
            return tensor;
        }

        private static float[] Softmax(float[] logits)
        {
            var maxLogit = logits.Max();
            var exps = new double[logits.Length];
            double sumExps = 0;

            for (int i = 0; i < logits.Length; i++)
            {
                exps[i] = Math.Exp(logits[i] - maxLogit);
                sumExps += exps[i];
            }

            var result = new float[logits.Length];
            for (int i = 0; i < logits.Length; i++)
            {
                result[i] = (float)(exps[i] / sumExps);
            }
            return result;
        }

        private (SentimentLabel label, double confidence, double score) InterpretProbabilities(float[] probs)
        {
            // TinyBERT binary model outputs: [negative, positive]
            var negativeProb = probs.Length > 0 ? probs[0] : 0;
            var positiveProb = probs.Length > 1 ? probs[1] : 0;

            // Calculate normalized score (0 = negative, 1 = positive)
            var score = (double)positiveProb;

            SentimentLabel label;
            double confidence;

            if (_options.EnableNeutral)
            {
                // Use thresholds to determine neutral
                if (score >= _options.NeutralUpperThreshold)
                {
                    label = SentimentLabel.Positive;
                    confidence = positiveProb;
                }
                else if (score <= _options.NeutralLowerThreshold)
                {
                    label = SentimentLabel.Negative;
                    confidence = negativeProb;
                }
                else
                {
                    label = SentimentLabel.Neutral;
                    confidence = 1.0 - Math.Abs(positiveProb - negativeProb);
                }
            }
            else
            {
                // Binary mode
                if (positiveProb >= negativeProb)
                {
                    label = SentimentLabel.Positive;
                    confidence = positiveProb;
                }
                else
                {
                    label = SentimentLabel.Negative;
                    confidence = negativeProb;
                }
            }

            return (label, confidence, score);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _session?.Dispose();
                _disposed = true;
            }
        }
    }
}
