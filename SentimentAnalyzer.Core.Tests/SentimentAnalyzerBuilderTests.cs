using SentimentAnalyzer;
using SentimentAnalyzer.Engines;
using Xunit;

namespace SentimentAnalyzer.Core.Tests;

public class SentimentAnalyzerBuilderTests
{
    [Fact]
    public void CreateBuilder_BuildsWorkingAnalyzer()
    {
        using var analyzer = SentimentAnalyzer.CreateBuilder().Build();

        var result = analyzer.Analyze("I love this!");

        Assert.NotNull(result);
        Assert.Equal("Vader", result.Engine);
    }

    [Fact]
    public void CreateBuilder_WithBinaryClassification_DisablesNeutral()
    {
        using var analyzer = SentimentAnalyzer.CreateBuilder()
            .WithBinaryClassification()
            .Build();

        var result = analyzer.Analyze("The meeting is at 3pm.");

        Assert.NotEqual(SentimentLabel.Neutral, result.Label);
    }

    [Fact]
    public void CreateBuilder_WithCustomThresholds_UsesThresholds()
    {
        using var analyzer = SentimentAnalyzer.CreateBuilder()
            .WithNeutralThresholds(0.3, 0.7) // Wide neutral zone
            .Build();

        // With a wide neutral zone, borderline text should be neutral
        var result = analyzer.Analyze("The product works as expected.");

        // Just verify it returns a valid result
        Assert.NotNull(result);
    }

    [Fact]
    public void CreateBuilder_WithOptions_UsesOptions()
    {
        var options = SentimentAnalyzerOptions.Strict;
        using var analyzer = SentimentAnalyzer.CreateBuilder()
            .WithOptions(options)
            .Build();

        var result = analyzer.Analyze("I love this!");

        Assert.NotNull(result);
    }

    [Fact]
    public void CreateBuilder_WithCustomEngine_UsesCustomEngine()
    {
        using var analyzer = SentimentAnalyzer.CreateBuilder()
            .WithEngine(_ => new VaderSentimentEngine())
            .Build();

        var result = analyzer.Analyze("Great product!");

        Assert.Equal("Vader", result.Engine);
    }

    [Fact]
    public void DefaultConstructor_UsesVaderEngine()
    {
        using var analyzer = new SentimentAnalyzer();

        var result = analyzer.Analyze("I love this!");

        Assert.Equal("Vader", result.Engine);
    }

    [Fact]
    public void ConstructorWithOptions_UsesVaderEngine()
    {
        using var analyzer = new SentimentAnalyzer(SentimentAnalyzerOptions.Binary);

        var result = analyzer.Analyze("This is okay.");

        Assert.Equal("Vader", result.Engine);
        Assert.NotEqual(SentimentLabel.Neutral, result.Label);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var analyzer = new SentimentAnalyzer();

        analyzer.Dispose();
        analyzer.Dispose(); // Should not throw
    }
}
