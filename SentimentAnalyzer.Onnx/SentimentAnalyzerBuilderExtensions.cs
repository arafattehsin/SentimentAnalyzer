using System;

namespace SentimentAnalyzer.Onnx
{
    /// <summary>
    /// Extension methods for configuring TinyBERT engine with SentimentAnalyzerBuilder.
    /// </summary>
    public static class SentimentAnalyzerBuilderExtensions
    {
        /// <summary>
        /// Configures the analyzer to use TinyBERT transformer model.
        /// The model is embedded in the package - no download required.
        /// </summary>
        /// <remarks>
        /// TinyBERT is 7.5x smaller and 9.4x faster than BERT while maintaining
        /// reasonable accuracy (~65%). Best for mobile and edge scenarios.
        /// English-only with binary classification (positive/negative).
        /// </remarks>
        /// <param name="builder">The builder instance.</param>
        /// <returns>The builder for method chaining.</returns>
        public static SentimentAnalyzerBuilder UseTinyBert(this SentimentAnalyzerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.WithEngine(options => new TinyBertSentimentEngine(options));
        }
    }
}
