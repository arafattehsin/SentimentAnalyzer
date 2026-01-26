namespace SentimentAnalyzer;

/// <summary>
/// Configuration options for sentiment analysis.
/// </summary>
public sealed class SentimentAnalyzerOptions
{
    /// <summary>
    /// Gets or sets the lower threshold for neutral classification.
    /// Scores below this are considered negative.
    /// </summary>
    public double NeutralLowerThreshold { get; set; } = 0.4;

    /// <summary>
    /// Gets or sets the upper threshold for neutral classification.
    /// Scores above this are considered positive.
    /// </summary>
    public double NeutralUpperThreshold { get; set; } = 0.6;

    /// <summary>
    /// Gets or sets whether to enable neutral classification.
    /// When false, only Positive/Negative labels are returned.
    /// </summary>
    public bool EnableNeutral { get; set; } = true;

    /// <summary>
    /// Default options with standard neutral zone (0.4-0.6).
    /// </summary>
    public static SentimentAnalyzerOptions Default => new();

    /// <summary>
    /// Binary classification (no neutral, only positive/negative).
    /// </summary>
    public static SentimentAnalyzerOptions Binary => new() { EnableNeutral = false };

    /// <summary>
    /// Strict options with narrow neutral zone (0.45-0.55).
    /// </summary>
    public static SentimentAnalyzerOptions Strict => new()
    {
        NeutralLowerThreshold = 0.45,
        NeutralUpperThreshold = 0.55
    };

    /// <summary>
    /// Lenient options with wide neutral zone (0.3-0.7).
    /// </summary>
    public static SentimentAnalyzerOptions Lenient => new()
    {
        NeutralLowerThreshold = 0.3,
        NeutralUpperThreshold = 0.7
    };
}
