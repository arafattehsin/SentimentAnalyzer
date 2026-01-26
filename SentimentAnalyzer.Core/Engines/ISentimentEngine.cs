namespace SentimentAnalyzer;

/// <summary>
/// Interface for sentiment analysis engines.
/// </summary>
public interface ISentimentEngine : IDisposable
{
    /// <summary>
    /// Gets the name of this engine.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Analyzes the sentiment of the given text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    /// <returns>A sentiment result with score and confidence.</returns>
    SentimentResult Analyze(string text);

    /// <summary>
    /// Analyzes the sentiment of multiple texts.
    /// </summary>
    /// <param name="texts">The texts to analyze.</param>
    /// <returns>Sentiment results for each text.</returns>
    IReadOnlyList<SentimentResult> AnalyzeBatch(IEnumerable<string> texts);
}
