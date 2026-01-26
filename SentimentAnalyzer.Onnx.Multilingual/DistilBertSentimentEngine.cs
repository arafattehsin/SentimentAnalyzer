using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SentimentAnalyzer.Onnx.Multilingual
{
    /// <summary>
    /// DistilBERT-based sentiment engine using ONNX Runtime.
    /// Supports 104 languages with 3-class classification (positive/neutral/negative).
    /// 
    /// NOTE: This engine requires a ~270MB model that is downloaded on first use.
    /// The model is cached locally and subsequent uses are fully offline.
    /// Cache location: %LOCALAPPDATA%/SentimentAnalyzer/models/
    /// </summary>
    public sealed class DistilBertSentimentEngine : ISentimentEngine, IDisposable
    {
        private readonly InferenceSession _session;
        private readonly SentimentAnalyzerOptions _options;
        private readonly WordPieceTokenizer _tokenizer;
        private bool _disposed;

        /// <inheritdoc/>
        public string Name => "DistilBert";

        /// <summary>
        /// Creates a new DistilBERT sentiment engine with automatic model download.
        /// The ~270MB model will be downloaded on first use and cached locally.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <remarks>
        /// This constructor will block until the model is downloaded (first use only).
        /// For async download with progress, use CreateAsync instead.
        /// </remarks>
        public DistilBertSentimentEngine(SentimentAnalyzerOptions? options = null)
        {
            _options = options ?? SentimentAnalyzerOptions.Default;
            var modelPath = ModelDownloader.EnsureModel();
            _session = new InferenceSession(modelPath);
            _tokenizer = new WordPieceTokenizer(ModelDownloader.GetVocabPath());
        }

        /// <summary>
        /// Creates a new DistilBERT sentiment engine from a specific model path.
        /// </summary>
        /// <param name="modelPath">Path to the ONNX model file.</param>
        /// <param name="vocabPath">Path to the vocabulary file.</param>
        /// <param name="options">Configuration options.</param>
        public DistilBertSentimentEngine(string modelPath, string vocabPath, SentimentAnalyzerOptions? options = null)
        {
            if (modelPath == null)
                throw new ArgumentNullException(nameof(modelPath));
            if (vocabPath == null)
                throw new ArgumentNullException(nameof(vocabPath));

            if (!File.Exists(modelPath))
                throw new FileNotFoundException($"ONNX model not found at: {modelPath}", modelPath);
            if (!File.Exists(vocabPath))
                throw new FileNotFoundException($"Vocabulary file not found at: {vocabPath}", vocabPath);

            _options = options ?? SentimentAnalyzerOptions.Default;
            _session = new InferenceSession(modelPath);
            _tokenizer = new WordPieceTokenizer(vocabPath);
        }

        /// <summary>
        /// Creates a new DistilBERT sentiment engine asynchronously with download progress.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <param name="progress">Optional progress callback (0.0 to 1.0).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A configured DistilBertSentimentEngine instance.</returns>
        public static async Task<DistilBertSentimentEngine> CreateAsync(
            SentimentAnalyzerOptions? options = null,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var modelPath = await ModelDownloader.EnsureModelAsync(progress, cancellationToken);
            var vocabPath = ModelDownloader.GetVocabPath();
            return new DistilBertSentimentEngine(modelPath, vocabPath, options);
        }

        /// <summary>
        /// Checks if the model is already cached locally (no download needed).
        /// </summary>
        public static bool IsModelCached => ModelDownloader.IsModelCached();

        /// <inheritdoc/>
        public SentimentResult Analyze(string text)
        {
            ThrowIfDisposed();
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var (inputIds, attentionMask) = _tokenizer.Encode(text, maxLength: 512);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", CreateTensor(inputIds)),
                NamedOnnxValue.CreateFromTensor("attention_mask", CreateTensor(attentionMask))
            };

            using var results = _session.Run(inputs);
            var logits = results.First().AsTensor<float>();

            // Debug: Print raw logits to verify model output order
            var rawLogits = new float[logits.Length];
            for (int i = 0; i < logits.Length; i++)
            {
                rawLogits[i] = logits.GetValue(i);
            }

            // Apply softmax to get probabilities
            var probabilities = Softmax(rawLogits);

            // Model outputs: [positive, neutral, negative] based on config.json id2label
            var (label, confidence, score) = InterpretProbabilities(probabilities);

            return new SentimentResult(text, label, score, confidence, Name);
        }

        /// <inheritdoc/>
        public IReadOnlyList<SentimentResult> AnalyzeBatch(IEnumerable<string> texts)
        {
            ThrowIfDisposed();
            if (texts == null)
                throw new ArgumentNullException(nameof(texts));

            var resultList = new List<SentimentResult>();
            foreach (var t in texts)
            {
                if (t != null)
                    resultList.Add(Analyze(t));
            }
            return resultList;
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
            // ONNX model outputs: [positive, neutral, negative]
            // per config.json id2label: {"0": "positive", "1": "neutral", "2": "negative"}
            var positiveProb = probs.Length > 0 ? probs[0] : 0;
            var neutralProb = probs.Length > 1 ? probs[1] : 0;
            var negativeProb = probs.Length > 2 ? probs[2] : 0;

            // Calculate normalized score (0 = negative, 0.5 = neutral, 1 = positive)
            var score = (positiveProb - negativeProb + 1) / 2;

            // Determine label and confidence
            SentimentLabel label;
            double confidence;

            if (!_options.EnableNeutral)
            {
                // Binary mode: ignore neutral probability
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
            else
            {
                // Find the highest probability class
                var maxProb = Math.Max(Math.Max(negativeProb, neutralProb), positiveProb);
                confidence = maxProb;

                if (maxProb == positiveProb)
                    label = SentimentLabel.Positive;
                else if (maxProb == negativeProb)
                    label = SentimentLabel.Negative;
                else
                    label = SentimentLabel.Neutral;
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
