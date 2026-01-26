using System;
using SentimentAnalyzer;
using SentimentAnalyzer.Onnx;
using Xunit;

namespace SentimentAnalyzer.Core.Tests;

/// <summary>
/// Tests for TinyBertSentimentEngine with real ONNX model.
/// </summary>
public class TinyBertEngineTests : IDisposable
{
    private readonly TinyBertSentimentEngine _engine;

    public TinyBertEngineTests()
    {
        _engine = new TinyBertSentimentEngine();
    }

    [Fact]
    public void Engine_Name_ReturnsTinyBert()
    {
        Assert.Equal("TinyBert", _engine.Name);
    }

    [Theory]
    [InlineData("I love this product! It's absolutely amazing!", SentimentLabel.Positive)]
    [InlineData("This is the best purchase I've ever made.", SentimentLabel.Positive)]
    [InlineData("Highly recommend, excellent quality!", SentimentLabel.Positive)]
    public void Analyze_PositiveText_ReturnsPositiveLabel(string text, SentimentLabel expected)
    {
        var result = _engine.Analyze(text);

        Assert.Equal(expected, result.Label);
        Assert.True(result.Score > 0.5, $"Score {result.Score} should be > 0.5 for positive text");
        Assert.True(result.Confidence > 0.5, $"Confidence {result.Confidence} should be > 0.5");
    }

    [Theory]
    [InlineData("This product is terrible, complete waste of money.", SentimentLabel.Negative)]
    [InlineData("Worst purchase ever. Don't buy this.", SentimentLabel.Negative)]
    [InlineData("Broke after one day. Very disappointed.", SentimentLabel.Negative)]
    public void Analyze_NegativeText_ReturnsNegativeLabel(string text, SentimentLabel expected)
    {
        var result = _engine.Analyze(text);

        Assert.Equal(expected, result.Label);
        Assert.True(result.Score < 0.5, $"Score {result.Score} should be < 0.5 for negative text");
        Assert.True(result.Confidence > 0.5, $"Confidence {result.Confidence} should be > 0.5");
    }

    [Fact]
    public void Analyze_NullText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _engine.Analyze(null!));
    }

    [Fact]
    public void AnalyzeBatch_MultipleTexts_ReturnsCorrectCount()
    {
        var texts = new[]
        {
            "Great product!",
            "Terrible experience",
            "It was okay"
        };

        var results = _engine.AnalyzeBatch(texts);

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.NotNull(r.Text));
    }

    [Fact]
    public void Analyze_ReturnsEngineName()
    {
        var result = _engine.Analyze("Test text");
        Assert.Equal("TinyBert", result.Engine);
    }

    [Fact]
    public void Analyze_ScoreIsInValidRange()
    {
        var result = _engine.Analyze("This is a test sentence.");

        Assert.InRange(result.Score, 0.0, 1.0);
        Assert.InRange(result.Confidence, 0.0, 1.0);
    }

    public void Dispose()
    {
        _engine.Dispose();
    }
}
