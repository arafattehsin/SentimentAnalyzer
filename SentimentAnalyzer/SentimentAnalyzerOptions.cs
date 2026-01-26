namespace SentimentAnalyzer
{
    /// <summary>
    /// Configuration options for the sentiment analyzer.
    /// </summary>
    public class SentimentAnalyzerOptions
    {
        /// <summary>
        /// Lower threshold for neutral classification. 
        /// Scores below this are classified as Negative.
        /// Default: 0.4
        /// </summary>
        public float NeutralLowerThreshold { get; set; } = 0.4f;

        /// <summary>
        /// Upper threshold for neutral classification.
        /// Scores above this are classified as Positive.
        /// Default: 0.6
        /// </summary>
        public float NeutralUpperThreshold { get; set; } = 0.6f;

        /// <summary>
        /// When true, disables neutral classification and uses binary (Positive/Negative) only.
        /// Default: false
        /// </summary>
        public bool DisableNeutral { get; set; } = false;

        /// <summary>
        /// Default options with standard thresholds.
        /// </summary>
        public static SentimentAnalyzerOptions Default => new SentimentAnalyzerOptions();

        /// <summary>
        /// Binary classification options (no neutral zone).
        /// </summary>
        public static SentimentAnalyzerOptions Binary => new SentimentAnalyzerOptions
        {
            DisableNeutral = true
        };

        /// <summary>
        /// Strict options with narrow neutral zone (0.45 - 0.55).
        /// </summary>
        public static SentimentAnalyzerOptions Strict => new SentimentAnalyzerOptions
        {
            NeutralLowerThreshold = 0.45f,
            NeutralUpperThreshold = 0.55f
        };

        /// <summary>
        /// Lenient options with wide neutral zone (0.3 - 0.7).
        /// </summary>
        public static SentimentAnalyzerOptions Lenient => new SentimentAnalyzerOptions
        {
            NeutralLowerThreshold = 0.3f,
            NeutralUpperThreshold = 0.7f
        };
    }
}
