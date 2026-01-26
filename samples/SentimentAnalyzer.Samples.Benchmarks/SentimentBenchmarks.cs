using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using global::SentimentAnalyzer.Onnx;
using global::SentimentAnalyzer.Onnx.Multilingual;
using SA = global::SentimentAnalyzer;
using SentimentResult = global::SentimentAnalyzer.SentimentResult;

namespace SentimentAnalyzer.Samples.Benchmarks;

/// <summary>
/// Benchmarks comparing all SentimentAnalyzer engines.
/// Run with: dotnet run -c Release
/// </summary>
[MemoryDiagnoser]
[SimpleJob] // Uses current runtime (.NET 10)
[RPlotExporter]
[MarkdownExporterAttribute.GitHub]
public class SentimentBenchmarks
{
    private SA.SentimentAnalyzer _vaderAnalyzer = null!;
    private SA.SentimentAnalyzer _tinyBertAnalyzer = null!;
    private SA.SentimentAnalyzer _distilBertAnalyzer = null!;

    private readonly Consumer _consumer = new();

    // Test data
    private const string ShortText = "I love this!";
    private const string MediumText = "This product is absolutely fantastic! I would definitely recommend it to anyone looking for quality.";
    private const string LongText = "I've been using this product for about three months now, and I have to say that my experience has been overwhelmingly positive. The quality is exceptional, the customer service is responsive, and the price point is very reasonable for what you get. I would highly recommend this to anyone who is looking for a reliable solution. The only minor issue I had was with the initial setup, but once I got past that, everything worked flawlessly.";

    private string[] _batchTexts = null!;

    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine("Setting up analyzers...");

        // Initialize all analyzers
        _vaderAnalyzer = SA.SentimentAnalyzer.CreateBuilder().Build();
        _tinyBertAnalyzer = SA.SentimentAnalyzer.CreateBuilder().UseTinyBert().Build();

        try
        {
            _distilBertAnalyzer = SA.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DistilBERT unavailable (will skip): {ex.Message}");
            // Create a dummy VADER analyzer to prevent null reference
            _distilBertAnalyzer = SA.SentimentAnalyzer.CreateBuilder().Build();
        }

        // Batch texts (50 items)
        var texts = new string[50];
        for (int n = 0; n < 50; n++)
        {
            int mod = n % 3;
            if (mod == 0)
                texts[n] = "I absolutely love this product! It's amazing!";
            else if (mod == 1)
                texts[n] = "This is terrible. I want my money back.";
            else
                texts[n] = "It's okay, nothing special really.";
        }
        _batchTexts = texts;

        // Warm up all analyzers
        _ = _vaderAnalyzer.Analyze(ShortText);
        _ = _tinyBertAnalyzer.Analyze(ShortText);
        _ = _distilBertAnalyzer.Analyze(ShortText);

        Console.WriteLine("Setup complete.");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _vaderAnalyzer?.Dispose();
        _tinyBertAnalyzer?.Dispose();
        _distilBertAnalyzer?.Dispose();
    }

    // ===== Single Prediction Benchmarks =====

    [Benchmark(Baseline = true, Description = "VADER - Short")]
    [BenchmarkCategory("Single", "Short")]
    public SentimentResult Vader_Short() => _vaderAnalyzer.Analyze(ShortText);

    [Benchmark(Description = "TinyBERT - Short")]
    [BenchmarkCategory("Single", "Short")]
    public SentimentResult TinyBert_Short() => _tinyBertAnalyzer.Analyze(ShortText);

    [Benchmark(Description = "DistilBERT - Short")]
    [BenchmarkCategory("Single", "Short")]
    public SentimentResult DistilBert_Short() => _distilBertAnalyzer.Analyze(ShortText);

    [Benchmark(Description = "VADER - Medium")]
    [BenchmarkCategory("Single", "Medium")]
    public SentimentResult Vader_Medium() => _vaderAnalyzer.Analyze(MediumText);

    [Benchmark(Description = "TinyBERT - Medium")]
    [BenchmarkCategory("Single", "Medium")]
    public SentimentResult TinyBert_Medium() => _tinyBertAnalyzer.Analyze(MediumText);

    [Benchmark(Description = "DistilBERT - Medium")]
    [BenchmarkCategory("Single", "Medium")]
    public SentimentResult DistilBert_Medium() => _distilBertAnalyzer.Analyze(MediumText);

    [Benchmark(Description = "VADER - Long")]
    [BenchmarkCategory("Single", "Long")]
    public SentimentResult Vader_Long() => _vaderAnalyzer.Analyze(LongText);

    [Benchmark(Description = "TinyBERT - Long")]
    [BenchmarkCategory("Single", "Long")]
    public SentimentResult TinyBert_Long() => _tinyBertAnalyzer.Analyze(LongText);

    [Benchmark(Description = "DistilBERT - Long")]
    [BenchmarkCategory("Single", "Long")]
    public SentimentResult DistilBert_Long() => _distilBertAnalyzer.Analyze(LongText);

    // ===== Batch Processing Benchmarks =====

    [Benchmark(Description = "VADER - Batch 50")]
    [BenchmarkCategory("Batch")]
    public void Vader_Batch()
    {
        _vaderAnalyzer.AnalyzeBatch(_batchTexts).Consume(_consumer);
    }

    [Benchmark(Description = "TinyBERT - Batch 50")]
    [BenchmarkCategory("Batch")]
    public void TinyBert_Batch()
    {
        _tinyBertAnalyzer.AnalyzeBatch(_batchTexts).Consume(_consumer);
    }

    [Benchmark(Description = "DistilBERT - Batch 50")]
    [BenchmarkCategory("Batch")]
    public void DistilBert_Batch()
    {
        _distilBertAnalyzer.AnalyzeBatch(_batchTexts).Consume(_consumer);
    }
}

/// <summary>
/// Cold start benchmarks - measures initialization time.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 0, iterationCount: 5)] // Uses current runtime (.NET 10)
[MarkdownExporterAttribute.GitHub]
public class ColdStartBenchmarks
{
    private const string TestText = "I love this product!";

    [Benchmark(Baseline = true, Description = "VADER Cold Start")]
    public SentimentResult Vader_ColdStart()
    {
        using var analyzer = SA.SentimentAnalyzer.CreateBuilder().Build();
        return analyzer.Analyze(TestText);
    }

    [Benchmark(Description = "TinyBERT Cold Start")]
    public SentimentResult TinyBert_ColdStart()
    {
        using var analyzer = SA.SentimentAnalyzer.CreateBuilder().UseTinyBert().Build();
        return analyzer.Analyze(TestText);
    }

    [Benchmark(Description = "DistilBERT Cold Start")]
    public SentimentResult DistilBert_ColdStart()
    {
        using var analyzer = SA.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build();
        return analyzer.Analyze(TestText);
    }
}
