namespace SentimentAnalyzer;

/// <summary>
/// Represents the result of sentiment analysis.
/// </summary>
public sealed class SentimentResult
{
    /// <summary>
    /// Gets the original text that was analyzed.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the sentiment classification label.
    /// </summary>
    public SentimentLabel Label { get; }

    /// <summary>
    /// Gets the sentiment score (normalized 0-1, where 0 is most negative, 1 is most positive).
    /// </summary>
    public double Score { get; }

    /// <summary>
    /// Gets the confidence level of the prediction (0-1).
    /// </summary>
    public double Confidence { get; }

    /// <summary>
    /// Gets the name of the engine that produced this result.
    /// </summary>
    public string Engine { get; }

    /// <summary>
    /// Gets whether the sentiment is positive.
    /// </summary>
    public bool IsPositive => Label == SentimentLabel.Positive;

    /// <summary>
    /// Gets whether the sentiment is negative.
    /// </summary>
    public bool IsNegative => Label == SentimentLabel.Negative;

    /// <summary>
    /// Gets whether the sentiment is neutral.
    /// </summary>
    public bool IsNeutral => Label == SentimentLabel.Neutral;

    /// <summary>
    /// Creates a new sentiment result.
    /// </summary>
    public SentimentResult(string text, SentimentLabel label, double score, double confidence, string engine)
    {
        Text = text ?? throw new ArgumentNullException(nameof(text));
        Label = label;
        Score = Clamp(score, 0, 1);
        Confidence = Clamp(confidence, 0, 1);
        Engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Label} (Score: {Score:F3}, Confidence: {Confidence:F3}, Engine: {Engine})";
}
