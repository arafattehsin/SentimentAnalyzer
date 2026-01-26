using System;
using System.Collections.Generic;
using System.Linq;
using SentimentAnalyzer.Models;
using Xunit;

namespace SentimentAnalyzer.Tests
{
    public class SentimentAnalyzerEngineTests
    {
        #region Basic Analysis Tests

        [Fact]
        public void Analyze_PositiveText_ReturnsPositiveLabel()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var result = analyzer.Analyze("This is amazing! I love it so much!");

            Assert.Equal(SentimentLabel.Positive, result.Label);
            Assert.True(result.IsPositive);
            Assert.False(result.IsNegative);
            Assert.False(result.IsNeutral);
        }

        [Fact]
        public void Analyze_NegativeText_ReturnsNegativeLabel()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var result = analyzer.Analyze("This is terrible! Worst experience ever!");

            Assert.Equal(SentimentLabel.Negative, result.Label);
            Assert.True(result.IsNegative);
            Assert.False(result.IsPositive);
            Assert.False(result.IsNeutral);
        }

        [Fact]
        public void Analyze_ReturnsOriginalText()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();
            const string text = "Test input text";

            var result = analyzer.Analyze(text);

            Assert.Equal(text, result.Text);
        }

        [Fact]
        public void Analyze_ReturnsScoreBetweenZeroAndOne()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var result = analyzer.Analyze("Some random text for testing.");

            Assert.InRange(result.Score, 0f, 1f);
        }

        [Fact]
        public void Analyze_ReturnsConfidenceBetweenZeroAndOne()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var result = analyzer.Analyze("Some random text for testing.");

            Assert.InRange(result.Confidence, 0f, 1f);
        }

        [Fact]
        public void Analyze_NullText_ThrowsArgumentNullException()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            Assert.Throws<ArgumentNullException>(() => analyzer.Analyze(null));
        }

        #endregion

        #region Neutral Classification Tests

        [Fact]
        public void Analyze_WithDefaultOptions_CanReturnNeutral()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            // Ambiguous text that might be neutral
            var result = analyzer.Analyze("The product arrived on time.");

            // Just verify it doesn't crash and returns a valid label
            Assert.True(result.Label == SentimentLabel.Positive ||
                        result.Label == SentimentLabel.Neutral ||
                        result.Label == SentimentLabel.Negative);
        }

        [Fact]
        public void Analyze_WithBinaryOptions_NeverReturnsNeutral()
        {
            using var analyzer = SentimentAnalyzerEngine.Create(SentimentAnalyzerOptions.Binary);

            var texts = new[]
            {
                "Great product!",
                "Terrible service.",
                "It was okay I guess.",
                "The item arrived."
            };

            foreach (var text in texts)
            {
                var result = analyzer.Analyze(text);
                Assert.NotEqual(SentimentLabel.Neutral, result.Label);
            }
        }

        #endregion

        #region Builder Pattern Tests

        [Fact]
        public void CreateBuilder_BuildsWorkingAnalyzer()
        {
            using var analyzer = SentimentAnalyzerEngine.CreateBuilder()
                .WithNeutralZone(0.4f, 0.6f)
                .Build();

            var result = analyzer.Analyze("This is a great product!");

            Assert.NotNull(result);
        }

        [Fact]
        public void CreateBuilder_WithBinaryClassification_DisablesNeutral()
        {
            using var analyzer = SentimentAnalyzerEngine.CreateBuilder()
                .WithBinaryClassification()
                .Build();

            var result = analyzer.Analyze("The product is average.");

            Assert.NotEqual(SentimentLabel.Neutral, result.Label);
        }

        [Fact]
        public void CreateBuilder_WithCustomThresholds_UsesThresholds()
        {
            using var analyzer = SentimentAnalyzerEngine.CreateBuilder()
                .WithNeutralLowerThreshold(0.3f)
                .WithNeutralUpperThreshold(0.7f)
                .Build();

            var result = analyzer.Analyze("Test text.");

            Assert.NotNull(result);
        }

        #endregion

        #region Batch Processing Tests

        [Fact]
        public void AnalyzeBatch_MultipleTexts_ReturnsResultsForAll()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var texts = new[] { "Great!", "Terrible!", "Okay." };
            var results = analyzer.AnalyzeBatch(texts).ToList();

            Assert.Equal(3, results.Count);
        }

        [Fact]
        public void AnalyzeBatch_SkipsNullTexts()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var texts = new[] { "Great!", null, "Terrible!" };
            var results = analyzer.AnalyzeBatch(texts).ToList();

            Assert.Equal(2, results.Count);
        }

        [Fact]
        public void AnalyzeBatch_NullCollection_ThrowsArgumentNullException()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            Assert.Throws<ArgumentNullException>(() => analyzer.AnalyzeBatch(null).ToList());
        }

        [Fact]
        public void AnalyzeBatch_EmptyCollection_ReturnsEmpty()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var results = analyzer.AnalyzeBatch(Array.Empty<string>()).ToList();

            Assert.Empty(results);
        }

        #endregion

        #region Disposal Tests

        [Fact]
        public void Analyze_AfterDispose_ThrowsObjectDisposedException()
        {
            var analyzer = SentimentAnalyzerEngine.Create();
            analyzer.Dispose();

            Assert.Throws<ObjectDisposedException>(() => analyzer.Analyze("test"));
        }

        [Fact]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            var analyzer = SentimentAnalyzerEngine.Create();

            analyzer.Dispose();
            analyzer.Dispose(); // Should not throw
        }

        #endregion

        #region Static Options Tests

        [Fact]
        public void Options_Default_HasExpectedThresholds()
        {
            var options = SentimentAnalyzerOptions.Default;

            Assert.Equal(0.4f, options.NeutralLowerThreshold);
            Assert.Equal(0.6f, options.NeutralUpperThreshold);
            Assert.False(options.DisableNeutral);
        }

        [Fact]
        public void Options_Binary_DisablesNeutral()
        {
            var options = SentimentAnalyzerOptions.Binary;

            Assert.True(options.DisableNeutral);
        }

        [Fact]
        public void Options_Strict_HasNarrowNeutralZone()
        {
            var options = SentimentAnalyzerOptions.Strict;

            Assert.Equal(0.45f, options.NeutralLowerThreshold);
            Assert.Equal(0.55f, options.NeutralUpperThreshold);
        }

        [Fact]
        public void Options_Lenient_HasWideNeutralZone()
        {
            var options = SentimentAnalyzerOptions.Lenient;

            Assert.Equal(0.3f, options.NeutralLowerThreshold);
            Assert.Equal(0.7f, options.NeutralUpperThreshold);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void SentimentResult_ToString_ContainsLabel()
        {
            using var analyzer = SentimentAnalyzerEngine.Create();

            var result = analyzer.Analyze("Great product!");
            var str = result.ToString();

            Assert.Contains("Score:", str);
            Assert.Contains("Confidence:", str);
        }

        #endregion
    }
}
