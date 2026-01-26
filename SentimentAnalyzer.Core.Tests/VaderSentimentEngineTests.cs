using SentimentAnalyzer;
using SentimentAnalyzer.Engines;
using Xunit;

namespace SentimentAnalyzer.Core.Tests;

public class VaderSentimentEngineTests
{
    [Fact]
    public void Analyze_PositiveText_ReturnsPositiveLabel()
    {
        using var engine = new VaderSentimentEngine();
        var result = engine.Analyze("I love this product! It's absolutely amazing and wonderful!");

        Assert.Equal(SentimentLabel.Positive, result.Label);
        Assert.True(result.IsPositive);
        Assert.False(result.IsNegative);
        Assert.Equal("Vader", result.Engine);
    }

    [Fact]
    public void Analyze_NegativeText_ReturnsNegativeLabel()
    {
        using var engine = new VaderSentimentEngine();
        var result = engine.Analyze("This is terrible! I hate it and it's the worst thing ever.");

        Assert.Equal(SentimentLabel.Negative, result.Label);
        Assert.True(result.IsNegative);
        Assert.False(result.IsPositive);
    }

    [Fact]
    public void Analyze_NeutralText_ReturnsNeutralLabel()
    {
        using var engine = new VaderSentimentEngine();
        var result = engine.Analyze("The package arrived on Tuesday.");

        Assert.Equal(SentimentLabel.Neutral, result.Label);
        Assert.True(result.IsNeutral);
        Assert.False(result.IsPositive);
        Assert.False(result.IsNegative);
    }

    [Fact]
    public void Analyze_WithBinaryOptions_NeverReturnsNeutral()
    {
        var options = SentimentAnalyzerOptions.Binary;
        using var engine = new VaderSentimentEngine(options);

        var result = engine.Analyze("The package arrived on Tuesday.");

        Assert.NotEqual(SentimentLabel.Neutral, result.Label);
    }

    [Fact]
    public void Analyze_ReturnsScoreBetweenZeroAndOne()
    {
        using var engine = new VaderSentimentEngine();
        var result = engine.Analyze("This is a test sentence.");

        Assert.InRange(result.Score, 0, 1);
    }

    [Fact]
    public void Analyze_ReturnsConfidenceBetweenZeroAndOne()
    {
        using var engine = new VaderSentimentEngine();
        var result = engine.Analyze("This is amazing!");

        Assert.InRange(result.Confidence, 0, 1);
    }

    [Fact]
    public void Analyze_NullText_ThrowsArgumentNullException()
    {
        using var engine = new VaderSentimentEngine();

        Assert.Throws<ArgumentNullException>(() => engine.Analyze(null!));
    }

    [Fact]
    public void AnalyzeBatch_MultipleTexts_ReturnsResultsForAll()
    {
        using var engine = new VaderSentimentEngine();
        var texts = new[] { "I love it!", "I hate it!", "It's okay." };

        var results = engine.AnalyzeBatch(texts);

        Assert.Equal(3, results.Count);
        Assert.Equal(SentimentLabel.Positive, results[0].Label);
        Assert.Equal(SentimentLabel.Negative, results[1].Label);
    }

    [Fact]
    public void AnalyzeBatch_SkipsNullTexts()
    {
        using var engine = new VaderSentimentEngine();
        var texts = new[] { "I love it!", null!, "I hate it!" };

        var results = engine.AnalyzeBatch(texts);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void AnalyzeBatch_NullCollection_ThrowsArgumentNullException()
    {
        using var engine = new VaderSentimentEngine();

        Assert.Throws<ArgumentNullException>(() => engine.AnalyzeBatch(null!));
    }

    [Fact]
    public void Analyze_AfterDispose_ThrowsObjectDisposedException()
    {
        var engine = new VaderSentimentEngine();
        engine.Dispose();

        Assert.Throws<ObjectDisposedException>(() => engine.Analyze("test"));
    }

    [Fact]
    public void Engine_Name_ReturnsVader()
    {
        using var engine = new VaderSentimentEngine();

        Assert.Equal("Vader", engine.Name);
    }

    [Fact]
    public void Analyze_SocialMediaText_HandleEmojisAndSlang()
    {
        using var engine = new VaderSentimentEngine();

        // VADER is specifically designed for social media
        var result = engine.Analyze("OMG this is sooo good!!! :) :) :)");

        Assert.Equal(SentimentLabel.Positive, result.Label);
    }
}
