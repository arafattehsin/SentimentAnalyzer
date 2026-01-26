using System;

namespace SentimentAnalyzer.Onnx.Multilingual
{
    /// <summary>
    /// Extension methods to add DistilBERT ONNX engine support to SentimentAnalyzerBuilder.
    /// </summary>
    public static class SentimentAnalyzerBuilderExtensions
    {
        /// <summary>
        /// Configures the builder to use the DistilBERT ONNX engine with automatic model download.
        /// The ~270MB model is downloaded on first use and cached locally for offline use.
        /// </summary>
        /// <remarks>
        /// Cache location: %LOCALAPPDATA%/SentimentAnalyzer/models/distilbert-multilingual-sentiment/
        /// </remarks>
        /// <param name="builder">The builder instance.</param>
        /// <returns>The builder for chaining.</returns>
        public static SentimentAnalyzerBuilder UseDistilBert(this SentimentAnalyzerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.WithEngine(options => new DistilBertSentimentEngine(options));
        }

        /// <summary>
        /// Configures the builder to use the DistilBERT ONNX engine with a specific model path.
        /// Use this if you have downloaded the model manually or want to use a custom location.
        /// </summary>
        /// <param name="builder">The builder instance.</param>
        /// <param name="modelPath">Path to the ONNX model file.</param>
        /// <param name="vocabPath">Path to the vocabulary file.</param>
        /// <returns>The builder for chaining.</returns>
        public static SentimentAnalyzerBuilder UseDistilBert(
            this SentimentAnalyzerBuilder builder,
            string modelPath,
            string vocabPath)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (modelPath == null)
                throw new ArgumentNullException(nameof(modelPath));
            if (vocabPath == null)
                throw new ArgumentNullException(nameof(vocabPath));

            return builder.WithEngine(options => new DistilBertSentimentEngine(modelPath, vocabPath, options));
        }
    }
}
