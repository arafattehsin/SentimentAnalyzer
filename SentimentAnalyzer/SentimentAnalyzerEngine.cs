using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.ML;
using SentimentAnalyzer.Models;

namespace SentimentAnalyzer
{
    /// <summary>
    /// Modern sentiment analyzer with configurable options and enhanced results.
    /// Note: This engine is NOT thread-safe. Create separate instances for concurrent use.
    /// </summary>
    public sealed class SentimentAnalyzerEngine : IDisposable
    {
        private readonly PredictionEngine<Sentiment, SentimentPrediction> _predictionEngine;
        private readonly SentimentAnalyzerOptions _options;
        private bool _disposed;

        /// <summary>
        /// Creates a new sentiment analyzer with the specified options.
        /// </summary>
        /// <param name="options">Configuration options. Uses default if null.</param>
        public SentimentAnalyzerEngine(SentimentAnalyzerOptions options = null)
        {
            _options = options ?? SentimentAnalyzerOptions.Default;

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("SentimentAnalyzer.MLModels.SentimentModel.zip"))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not load the sentiment model. Ensure SentimentModel.zip is embedded.");
                }

                var mlContext = new MLContext();
                ITransformer trainedModel = mlContext.Model.Load(stream, out var modelInputSchema);
                _predictionEngine = mlContext.Model.CreatePredictionEngine<Sentiment, SentimentPrediction>(trainedModel);
            }
        }

        /// <summary>
        /// Analyzes the sentiment of the provided text.
        /// </summary>
        /// <param name="text">The text to analyze.</param>
        /// <returns>A SentimentResult containing the classification and scores.</returns>
        /// <exception cref="ArgumentNullException">Thrown when text is null.</exception>
        /// <exception cref="ObjectDisposedException">Thrown when the analyzer has been disposed.</exception>
        public SentimentResult Analyze(string text)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SentimentAnalyzerEngine));
            }

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var input = new Sentiment { Col0 = text };
            var prediction = _predictionEngine.Predict(input);

            // Use Probability if available (0-1), otherwise apply sigmoid to raw Score
            float normalizedScore = prediction.Probability > 0 ? prediction.Probability : Sigmoid(prediction.Score);

            return CreateResult(text, normalizedScore);
        }

        /// <summary>
        /// Converts a raw score (logit) to a probability using sigmoid function.
        /// </summary>
        private static float Sigmoid(float x)
        {
            return 1f / (1f + (float)Math.Exp(-x));
        }

        /// <summary>
        /// Analyzes the sentiment of multiple texts.
        /// </summary>
        /// <param name="texts">The texts to analyze.</param>
        /// <returns>A collection of SentimentResult objects.</returns>
        /// <exception cref="ArgumentNullException">Thrown when texts is null.</exception>
        public IEnumerable<SentimentResult> AnalyzeBatch(IEnumerable<string> texts)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SentimentAnalyzerEngine));
            }

            if (texts == null)
            {
                throw new ArgumentNullException(nameof(texts));
            }

            foreach (var text in texts)
            {
                if (text != null)
                {
                    yield return Analyze(text);
                }
            }
        }

        private SentimentResult CreateResult(string text, float score)
        {
            SentimentLabel label;
            float confidence;

            if (_options.DisableNeutral)
            {
                // Binary classification
                label = score >= 0.5f ? SentimentLabel.Positive : SentimentLabel.Negative;
                confidence = Math.Abs(score - 0.5f) * 2f; // 0 to 1 scale
            }
            else
            {
                // Three-class classification with neutral zone
                if (score < _options.NeutralLowerThreshold)
                {
                    label = SentimentLabel.Negative;
                    confidence = (_options.NeutralLowerThreshold - score) / _options.NeutralLowerThreshold;
                }
                else if (score > _options.NeutralUpperThreshold)
                {
                    label = SentimentLabel.Positive;
                    confidence = (score - _options.NeutralUpperThreshold) / (1f - _options.NeutralUpperThreshold);
                }
                else
                {
                    label = SentimentLabel.Neutral;
                    // Confidence for neutral: how close to the center of the neutral zone
                    float neutralCenter = (_options.NeutralLowerThreshold + _options.NeutralUpperThreshold) / 2f;
                    float neutralHalfWidth = (_options.NeutralUpperThreshold - _options.NeutralLowerThreshold) / 2f;
                    confidence = 1f - Math.Abs(score - neutralCenter) / neutralHalfWidth;
                }
            }

            return new SentimentResult(text, score, label, Math.Min(1f, Math.Max(0f, confidence)));
        }

        /// <summary>
        /// Releases resources used by the analyzer.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _predictionEngine?.Dispose();
                _disposed = true;
            }
        }

        #region Static Factory Methods

        /// <summary>
        /// Creates a sentiment analyzer with default options.
        /// </summary>
        public static SentimentAnalyzerEngine Create() => new SentimentAnalyzerEngine();

        /// <summary>
        /// Creates a sentiment analyzer with the specified options.
        /// </summary>
        public static SentimentAnalyzerEngine Create(SentimentAnalyzerOptions options) => new SentimentAnalyzerEngine(options);

        /// <summary>
        /// Creates a sentiment analyzer using a builder pattern.
        /// </summary>
        public static SentimentAnalyzerBuilder CreateBuilder() => new SentimentAnalyzerBuilder();

        #endregion
    }

    /// <summary>
    /// Builder for creating a configured SentimentAnalyzerEngine.
    /// </summary>
    public class SentimentAnalyzerBuilder
    {
        private readonly SentimentAnalyzerOptions _options = new SentimentAnalyzerOptions();

        /// <summary>
        /// Sets the lower threshold for neutral classification.
        /// </summary>
        public SentimentAnalyzerBuilder WithNeutralLowerThreshold(float threshold)
        {
            _options.NeutralLowerThreshold = threshold;
            return this;
        }

        /// <summary>
        /// Sets the upper threshold for neutral classification.
        /// </summary>
        public SentimentAnalyzerBuilder WithNeutralUpperThreshold(float threshold)
        {
            _options.NeutralUpperThreshold = threshold;
            return this;
        }

        /// <summary>
        /// Sets both neutral thresholds at once.
        /// </summary>
        public SentimentAnalyzerBuilder WithNeutralZone(float lower, float upper)
        {
            _options.NeutralLowerThreshold = lower;
            _options.NeutralUpperThreshold = upper;
            return this;
        }

        /// <summary>
        /// Disables neutral classification (binary positive/negative only).
        /// </summary>
        public SentimentAnalyzerBuilder WithBinaryClassification()
        {
            _options.DisableNeutral = true;
            return this;
        }

        /// <summary>
        /// Enables three-class classification (positive/neutral/negative).
        /// </summary>
        public SentimentAnalyzerBuilder WithThreeClassClassification()
        {
            _options.DisableNeutral = false;
            return this;
        }

        /// <summary>
        /// Builds the configured sentiment analyzer.
        /// </summary>
        public SentimentAnalyzerEngine Build() => new SentimentAnalyzerEngine(_options);
    }
}
