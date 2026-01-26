using VaderSharp2;

namespace SentimentAnalyzer.Engines;

/// <summary>
/// VADER (Valence Aware Dictionary and sEntiment Reasoner) sentiment engine.
/// A lexicon and rule-based sentiment analysis tool optimized for social media.
/// </summary>
public sealed class VaderSentimentEngine : ISentimentEngine
{
    private readonly SentimentIntensityAnalyzer _analyzer;
    private readonly SentimentAnalyzerOptions _options;
    private bool _disposed;

    /// <inheritdoc/>
    public string Name => "Vader";

    /// <summary>
    /// Creates a new VADER sentiment engine with default options.
    /// </summary>
    public VaderSentimentEngine() : this(SentimentAnalyzerOptions.Default)
    {
    }

    /// <summary>
    /// Creates a new VADER sentiment engine with the specified options.
    /// </summary>
    /// <param name="options">Configuration options for sentiment classification.</param>
    public VaderSentimentEngine(SentimentAnalyzerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _analyzer = new SentimentIntensityAnalyzer();
    }

    /// <inheritdoc/>
    public SentimentResult Analyze(string text)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        var scores = _analyzer.PolarityScores(text);

        // VADER compound score ranges from -1 to 1, normalize to 0-1
        var normalizedScore = (scores.Compound + 1) / 2;

        // Calculate confidence based on the strength of the compound score
        // Compound closer to -1 or 1 means higher confidence
        var confidence = Math.Abs(scores.Compound);

        var label = ClassifyLabel(normalizedScore);

        return new SentimentResult(text, label, normalizedScore, confidence, Name);
    }

    /// <inheritdoc/>
    public IReadOnlyList<SentimentResult> AnalyzeBatch(IEnumerable<string> texts)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
        if (texts == null)
            throw new ArgumentNullException(nameof(texts));

        return texts
            .Where(t => t != null)
            .Select(Analyze)
            .ToList();
    }

    private SentimentLabel ClassifyLabel(double score)
    {
        if (!_options.EnableNeutral)
        {
            return score >= 0.5 ? SentimentLabel.Positive : SentimentLabel.Negative;
        }

        if (score < _options.NeutralLowerThreshold)
            return SentimentLabel.Negative;
        if (score > _options.NeutralUpperThreshold)
            return SentimentLabel.Positive;

        return SentimentLabel.Neutral;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposed = true;
    }
}
