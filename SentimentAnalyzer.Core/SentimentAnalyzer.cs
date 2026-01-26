using SentimentAnalyzer.Engines;

namespace SentimentAnalyzer;

/// <summary>
/// Main entry point for sentiment analysis. Uses a pluggable engine architecture.
/// </summary>
public sealed class SentimentAnalyzer : ISentimentEngine
{
    private readonly ISentimentEngine _engine;
    private bool _disposed;

    /// <inheritdoc/>
    public string Name => _engine.Name;

    /// <summary>
    /// Creates a new SentimentAnalyzer with the default VADER engine.
    /// </summary>
    public SentimentAnalyzer() : this(new VaderSentimentEngine())
    {
    }

    /// <summary>
    /// Creates a new SentimentAnalyzer with the specified options using VADER engine.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public SentimentAnalyzer(SentimentAnalyzerOptions options)
        : this(new VaderSentimentEngine(options))
    {
    }

    /// <summary>
    /// Creates a new SentimentAnalyzer with a custom engine.
    /// </summary>
    /// <param name="engine">The sentiment engine to use.</param>
    public SentimentAnalyzer(ISentimentEngine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    /// <summary>
    /// Creates a new builder for configuring a SentimentAnalyzer.
    /// </summary>
    /// <returns>A new builder instance.</returns>
    public static SentimentAnalyzerBuilder CreateBuilder() => new();

    /// <inheritdoc/>
    public SentimentResult Analyze(string text)
    {
        ThrowIfDisposed();
        return _engine.Analyze(text);
    }

    /// <inheritdoc/>
    public IReadOnlyList<SentimentResult> AnalyzeBatch(IEnumerable<string> texts)
    {
        ThrowIfDisposed();
        return _engine.AnalyzeBatch(texts);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _engine.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Builder for creating configured SentimentAnalyzer instances.
/// </summary>
public sealed class SentimentAnalyzerBuilder
{
    private SentimentAnalyzerOptions _options = SentimentAnalyzerOptions.Default;
    private Func<SentimentAnalyzerOptions, ISentimentEngine>? _engineFactory;

    /// <summary>
    /// Sets the options for sentiment classification.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <returns>This builder for chaining.</returns>
    public SentimentAnalyzerBuilder WithOptions(SentimentAnalyzerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Configures for binary classification (no neutral).
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public SentimentAnalyzerBuilder WithBinaryClassification()
    {
        _options = SentimentAnalyzerOptions.Binary;
        return this;
    }

    /// <summary>
    /// Sets custom neutral thresholds.
    /// </summary>
    /// <param name="lowerThreshold">Lower bound for neutral zone.</param>
    /// <param name="upperThreshold">Upper bound for neutral zone.</param>
    /// <returns>This builder for chaining.</returns>
    public SentimentAnalyzerBuilder WithNeutralThresholds(double lowerThreshold, double upperThreshold)
    {
        _options = new SentimentAnalyzerOptions
        {
            NeutralLowerThreshold = lowerThreshold,
            NeutralUpperThreshold = upperThreshold,
            EnableNeutral = true
        };
        return this;
    }

    /// <summary>
    /// Sets a custom engine factory. Used by extension packages like SentimentAnalyzer.Onnx.
    /// </summary>
    /// <param name="engineFactory">Factory function that creates the engine.</param>
    /// <returns>This builder for chaining.</returns>
    public SentimentAnalyzerBuilder WithEngine(Func<SentimentAnalyzerOptions, ISentimentEngine> engineFactory)
    {
        _engineFactory = engineFactory ?? throw new ArgumentNullException(nameof(engineFactory));
        return this;
    }

    /// <summary>
    /// Builds the configured SentimentAnalyzer.
    /// </summary>
    /// <returns>A new SentimentAnalyzer instance.</returns>
    public SentimentAnalyzer Build()
    {
        var engine = _engineFactory?.Invoke(_options) ?? new VaderSentimentEngine(_options);
        return new SentimentAnalyzer(engine);
    }
}
