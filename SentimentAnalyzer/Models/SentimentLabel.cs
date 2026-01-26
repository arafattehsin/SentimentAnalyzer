namespace SentimentAnalyzer.Models
{
    /// <summary>
    /// Represents the sentiment classification label.
    /// </summary>
    public enum SentimentLabel
    {
        /// <summary>
        /// Negative sentiment (score below lower threshold).
        /// </summary>
        Negative = -1,

        /// <summary>
        /// Neutral sentiment (score between lower and upper thresholds).
        /// </summary>
        Neutral = 0,

        /// <summary>
        /// Positive sentiment (score above upper threshold).
        /// </summary>
        Positive = 1
    }
}
