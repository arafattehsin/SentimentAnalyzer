using System.Diagnostics;
using SentimentAnalyzer;
using SentimentAnalyzer.Onnx;
using SentimentAnalyzer.Onnx.Multilingual;
using Spectre.Console;

// Sample texts for demonstration
var sampleTexts = new[]
{
    "I absolutely love this product! It exceeded all my expectations.",
    "This is the worst experience I've ever had. Completely disappointed.",
    "The weather is nice today.",
    "I'm not sure how I feel about this.",
    "¡Este producto es increíble! Lo recomiendo mucho.",  // Spanish: "This product is amazing! I highly recommend it."
    "Je déteste attendre. C'est vraiment frustrant.",      // French: "I hate waiting. It's really frustrating."
    "Das Essen war okay, nichts Besonderes.",               // German: "The food was okay, nothing special."
};

AnsiConsole.Write(new FigletText("Sentiment Analyzer").Centered().Color(Color.Aqua));
AnsiConsole.MarkupLine("[grey]Offline Sentiment Analysis for .NET[/]\n");

// Menu loop
while (true)
{
    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]Choose an option:[/]")
            .AddChoices(
                "🚀 Quick Demo (All Engines)",
                "⌨️  Interactive Mode",
                "📊 Performance Comparison",
                "⏱️  Detailed Performance Test (10+ Sentences)",
                "📦 Package Info",
                "🚪 Exit"));

    switch (choice)
    {
        case "🚀 Quick Demo (All Engines)":
            await RunQuickDemo();
            break;
        case "⌨️  Interactive Mode":
            await RunInteractiveMode();
            break;
        case "📊 Performance Comparison":
            await RunPerformanceComparison();
            break;
        case "⏱️  Detailed Performance Test (10+ Sentences)":
            await RunDetailedPerformanceTest();
            break;
        case "📦 Package Info":
            ShowPackageInfo();
            break;
        case "🚪 Exit":
            AnsiConsole.MarkupLine("[green]Goodbye![/]");
            return;
    }

    AnsiConsole.WriteLine();
}

async Task RunQuickDemo()
{
    AnsiConsole.MarkupLine("\n[bold cyan]Quick Demo - Analyzing sample texts with all engines[/]\n");

    // VADER (Core)
    using var vaderAnalyzer = SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().Build();

    // TinyBERT (Onnx)
    using var tinyBertAnalyzer = SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseTinyBert().Build();

    // DistilBERT (Onnx.Multilingual) - may need to download model
    SentimentAnalyzer.SentimentAnalyzer? distilBertAnalyzer = null;
    try
    {
        await AnsiConsole.Status()
            .StartAsync("Loading DistilBERT model (may download on first use)...", async ctx =>
            {
                distilBertAnalyzer = await Task.Run(() =>
                    SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build());
            });
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[yellow]DistilBERT unavailable: {ex.Message}[/]");
    }

    var table = new Table()
        .Border(TableBorder.Rounded)
        .AddColumn(new TableColumn("[bold]Text[/]").Width(45))
        .AddColumn(new TableColumn("[bold]VADER[/]").Centered())
        .AddColumn(new TableColumn("[bold]TinyBERT[/]").Centered())
        .AddColumn(new TableColumn("[bold]DistilBERT[/]").Centered());

    foreach (var text in sampleTexts)
    {
        var displayText = text.Length > 42 ? text[..42] + "..." : text;

        var vaderResult = vaderAnalyzer.Analyze(text);
        var tinyBertResult = tinyBertAnalyzer.Analyze(text);
        var distilBertResult = distilBertAnalyzer?.Analyze(text);

        table.AddRow(
            displayText,
            FormatSentiment(vaderResult.Label, vaderResult.Confidence),
            FormatSentiment(tinyBertResult.Label, tinyBertResult.Confidence),
            distilBertResult != null
                ? FormatSentiment(distilBertResult.Label, distilBertResult.Confidence)
                : "[grey]N/A[/]");
    }

    AnsiConsole.Write(table);
    distilBertAnalyzer?.Dispose();
}

async Task RunInteractiveMode()
{
    var engineChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[yellow]Select engine:[/]")
            .AddChoices("VADER (Core - Fastest)", "TinyBERT (ONNX - Balanced)", "DistilBERT (Multilingual)"));

    SentimentAnalyzer.SentimentAnalyzer analyzer;

    if (engineChoice.Contains("VADER"))
    {
        analyzer = SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().Build();
    }
    else if (engineChoice.Contains("TinyBERT"))
    {
        analyzer = SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseTinyBert().Build();
    }
    else
    {
        await AnsiConsole.Status()
            .StartAsync("Loading DistilBERT...", async ctx =>
            {
                await Task.Delay(100); // Allow UI update
            });
        analyzer = SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build();
    }

    AnsiConsole.MarkupLine($"\n[green]Using {analyzer.Name}[/]");
    AnsiConsole.MarkupLine("[grey]Type text to analyze (or 'exit' to return):[/]\n");

    while (true)
    {
        AnsiConsole.Markup("[blue]>[/] ");
        var input = Console.ReadLine();

        if (string.IsNullOrEmpty(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            break;

        var sw = Stopwatch.StartNew();
        var result = analyzer.Analyze(input);
        sw.Stop();

        // Calculate derived scores from the main score for display
        var positiveScore = result.Label == SentimentLabel.Positive ? result.Confidence : (1 - result.Confidence) * 0.3;
        var negativeScore = result.Label == SentimentLabel.Negative ? result.Confidence : (1 - result.Confidence) * 0.3;
        var neutralScore = result.Label == SentimentLabel.Neutral ? result.Confidence : (1 - result.Confidence) * 0.4;

        var panel = new Panel(
            $"[bold]{result.Label}[/] ({result.Confidence:P1})\n" +
            $"[grey]Score: {result.Score:F3}[/]\n" +
            $"[dim]Time: {sw.Elapsed.TotalMilliseconds:F2}ms[/]")
        {
            Header = new PanelHeader($"[{GetSentimentColor(result.Label)}]{result.Label}[/]"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    analyzer.Dispose();
}

async Task RunDetailedPerformanceTest()
{
    AnsiConsole.MarkupLine("\n[bold cyan]Detailed Performance Test - Individual Sentence Analysis[/]\n");

    // Test sentences (10 sentences)
    var testSentences = new[]
    {
        "This is absolutely the best thing I've ever experienced!",
        "I'm extremely disappointed and frustrated with this service.",
        "The weather today is quite pleasant.",
        "I have mixed feelings about this situation.",
        "This product exceeded all my expectations! Highly recommended!",
        "Terrible quality, waste of money. Do not buy!",
        "It's okay, nothing special but not bad either.",
        "I'm not sure how I feel about this yet.",
        "Outstanding performance! Worth every penny!",
        "Complete disaster. I want my money back immediately."
    };

    var iterations = AnsiConsole.Prompt(
        new TextPrompt<int>("[yellow]Number of iterations per engine:[/]")
            .DefaultValue(100)
            .Validate(n => n is > 0 and <= 1000
                ? ValidationResult.Success()
                : ValidationResult.Error("Enter a number between 1 and 1000")));

    // Initialize engines
    AnsiConsole.Status()
        .Start("Initializing engines...", ctx =>
        {
            ctx.Status("Loading VADER engine...");
            Thread.Sleep(100);
        });

    using var vaderAnalyzer = SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().Build();
    using var tinyBertAnalyzer = SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseTinyBert().Build();

    SentimentAnalyzer.SentimentAnalyzer? distilBertAnalyzer = null;
    try
    {
        await AnsiConsole.Status()
            .StartAsync("Loading DistilBERT model...", async ctx =>
            {
                distilBertAnalyzer = await Task.Run(() =>
                    SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build());
            });
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[yellow]DistilBERT unavailable: {ex.Message}[/]");
    }

    AnsiConsole.MarkupLine($"\n[green]Testing {testSentences.Length} sentences with {iterations} iterations each[/]\n");

    // Results table for individual sentences
    var resultsTable = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold]Individual Sentence Results[/]")
        .AddColumn(new TableColumn("[bold]#[/]").Width(3).RightAligned())
        .AddColumn(new TableColumn("[bold]Sentence[/]").Width(40))
        .AddColumn(new TableColumn("[bold]Engine[/]").Width(12))
        .AddColumn(new TableColumn("[bold]Sentiment[/]").Width(10).Centered())
        .AddColumn(new TableColumn("[bold]Confidence[/]").Width(10).RightAligned())
        .AddColumn(new TableColumn("[bold]Time (ms)[/]").Width(10).RightAligned());

    var performanceData = new List<(string Engine, List<double> Times)>
    {
        ("VADER (Core)", new List<double>()),
        ("TinyBERT (ONNX)", new List<double>()),
    };

    if (distilBertAnalyzer != null)
    {
        performanceData.Add(("DistilBERT (Multilingual)", new List<double>()));
    }

    // Test each sentence
    for (int i = 0; i < testSentences.Length; i++)
    {
        var sentence = testSentences[i];
        var displayText = sentence.Length > 37 ? sentence[..37] + "..." : sentence;

        // VADER
        var vaderTime = MeasureAnalysisTime(vaderAnalyzer, sentence, iterations);
        var vaderResult = vaderAnalyzer.Analyze(sentence);
        performanceData[0].Times.Add(vaderTime);

        resultsTable.AddRow(
            $"{i + 1}",
            $"[dim]{displayText}[/]",
            "VADER",
            FormatSentimentSimple(vaderResult.Label),
            $"{vaderResult.Confidence:P0}",
            $"{vaderTime:F3}");

        // TinyBERT
        var tinyBertTime = MeasureAnalysisTime(tinyBertAnalyzer, sentence, iterations);
        var tinyBertResult = tinyBertAnalyzer.Analyze(sentence);
        performanceData[1].Times.Add(tinyBertTime);

        resultsTable.AddRow(
            "",
            "",
            "TinyBERT",
            FormatSentimentSimple(tinyBertResult.Label),
            $"{tinyBertResult.Confidence:P0}",
            $"{tinyBertTime:F3}");

        // DistilBERT
        if (distilBertAnalyzer != null)
        {
            var distilBertTime = MeasureAnalysisTime(distilBertAnalyzer, sentence, iterations);
            var distilBertResult = distilBertAnalyzer.Analyze(sentence);
            performanceData[2].Times.Add(distilBertTime);

            resultsTable.AddRow(
                "",
                "",
                "DistilBERT",
                FormatSentimentSimple(distilBertResult.Label),
                $"{distilBertResult.Confidence:P0}",
                $"{distilBertTime:F3}");
        }

        if (i < testSentences.Length - 1)
        {
            resultsTable.AddEmptyRow();
        }
    }

    AnsiConsole.Write(resultsTable);

    // Calculate and display performance summary
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("\n[bold yellow]═══════════════════════════════════════════════════════════════════════════[/]");
    AnsiConsole.MarkupLine("[bold yellow]                            Performance Results                              [/]");
    AnsiConsole.MarkupLine("[bold yellow]═══════════════════════════════════════════════════════════════════════════[/]\n");

    var summaryTable = new Table()
        .Border(TableBorder.Rounded)
        .Title($"[bold]Number of iterations per engine: ({iterations}): {testSentences.Length}[/]")
        .AddColumn(new TableColumn("[bold]Engine[/]").Width(28))
        .AddColumn(new TableColumn("[bold]Cold Start[/]").Width(12).RightAligned())
        .AddColumn(new TableColumn("[bold]Warm Avg[/]").Width(12).RightAligned())
        .AddColumn(new TableColumn("[bold]Throughput[/]").Width(12).RightAligned())
        .AddColumn(new TableColumn("[bold]Accuracy[/]").Width(12).Centered());

    foreach (var (engine, times) in performanceData)
    {
        var coldStart = times.First();
        var warmAvg = times.Skip(1).Average();
        var throughput = 1000.0 / warmAvg; // predictions per second

        // Get accuracy based on engine
        var accuracy = engine switch
        {
            "VADER (Core)" => "~75%",
            "TinyBERT (ONNX)" => "94.2%",
            "DistilBERT (Multilingual)" => "~88%",
            _ => "N/A"
        };

        summaryTable.AddRow(
            engine,
            $"{coldStart:F2}ms",
            $"{warmAvg:F3}ms",
            $"{throughput:F0}/sec",
            accuracy);
    }

    AnsiConsole.Write(summaryTable);

    // Additional insights
    AnsiConsole.WriteLine();
    var fastestEngine = performanceData.MinBy(p => p.Times.Skip(1).Average());
    var highestThroughput = performanceData.MaxBy(p => 1000.0 / p.Times.Skip(1).Average());

    AnsiConsole.MarkupLine($"[green]⚡ Fastest average:[/] {fastestEngine.Engine} at {fastestEngine.Times.Skip(1).Average():F3}ms");
    AnsiConsole.MarkupLine($"[green]🚀 Highest throughput:[/] {highestThroughput.Engine} at {1000.0 / highestThroughput.Times.Skip(1).Average():F0} predictions/sec");

    distilBertAnalyzer?.Dispose();
}

double MeasureAnalysisTime(SentimentAnalyzer.SentimentAnalyzer analyzer, string text, int iterations)
{
    var sw = Stopwatch.StartNew();
    for (int i = 0; i < iterations; i++)
    {
        _ = analyzer.Analyze(text);
    }
    sw.Stop();
    return sw.Elapsed.TotalMilliseconds / iterations;
}

string FormatSentimentSimple(SentimentLabel label)
{
    var color = GetSentimentColor(label);
    var emoji = label switch
    {
        SentimentLabel.Positive => "😊",
        SentimentLabel.Negative => "😞",
        _ => "😐"
    };
    return $"[{color}]{emoji} {label}[/]";
}

async Task RunPerformanceComparison()
{
    AnsiConsole.MarkupLine("\n[bold cyan]Performance Comparison[/]\n");

    var iterations = AnsiConsole.Prompt(
        new TextPrompt<int>("[yellow]Number of iterations per engine:[/]")
            .DefaultValue(100)
            .Validate(n => n is > 0 and <= 10000
                ? ValidationResult.Success()
                : ValidationResult.Error("Enter a number between 1 and 10000")));

    var testText = "This product is absolutely fantastic! I love everything about it.";

    var results = new List<(string Engine, double ColdStart, double WarmAvg, double ThroughputPerSec)>();

    // VADER
    await AnsiConsole.Status()
        .StartAsync("Benchmarking VADER...", async ctx =>
        {
            var (cold, warm, throughput) = await BenchmarkEngine(
                () => SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().Build(),
                testText, iterations);
            results.Add(("VADER (Core)", cold, warm, throughput));
        });

    // TinyBERT
    await AnsiConsole.Status()
        .StartAsync("Benchmarking TinyBERT...", async ctx =>
        {
            var (cold, warm, throughput) = await BenchmarkEngine(
                () => SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseTinyBert().Build(),
                testText, iterations);
            results.Add(("TinyBERT (ONNX)", cold, warm, throughput));
        });

    // DistilBERT
    try
    {
        await AnsiConsole.Status()
            .StartAsync("Benchmarking DistilBERT...", async ctx =>
            {
                var (cold, warm, throughput) = await BenchmarkEngine(
                    () => SentimentAnalyzer.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build(),
                    testText, iterations);
                results.Add(("DistilBERT (Multilingual)", cold, warm, throughput));
            });
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[yellow]DistilBERT skipped: {ex.Message}[/]");
    }

    // Display results
    var table = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold]Performance Results[/]")
        .AddColumn(new TableColumn("[bold]Engine[/]"))
        .AddColumn(new TableColumn("[bold]Cold Start[/]").RightAligned())
        .AddColumn(new TableColumn("[bold]Warm Avg[/]").RightAligned())
        .AddColumn(new TableColumn("[bold]Throughput[/]").RightAligned());

    foreach (var (engine, cold, warm, throughput) in results)
    {
        table.AddRow(
            engine,
            $"{cold:F2}ms",
            $"{warm:F3}ms",
            $"{throughput:F0}/sec");
    }

    AnsiConsole.Write(table);

    // Summary
    AnsiConsole.WriteLine();
    var fastest = results.MinBy(r => r.WarmAvg);
    var highest = results.MaxBy(r => r.ThroughputPerSec);
    AnsiConsole.MarkupLine($"[green]⚡ Fastest:[/] {fastest.Engine} at {fastest.WarmAvg:F3}ms per prediction");
    AnsiConsole.MarkupLine($"[green]🚀 Highest throughput:[/] {highest.Engine} at {highest.ThroughputPerSec:F0} predictions/sec");
}

void ShowPackageInfo()
{
    var table = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold]SentimentAnalyzer Packages[/]")
        .AddColumn("[bold]Package[/]")
        .AddColumn("[bold]Size[/]")
        .AddColumn("[bold]Engine[/]")
        .AddColumn("[bold]Languages[/]")
        .AddColumn("[bold]Accuracy[/]");

    table.AddRow(
        "[cyan]SentimentAnalyzer.Core[/]",
        "[green]<1 MB[/]",
        "VADER (Lexicon)",
        "English",
        "~75%");

    table.AddRow(
        "[cyan]SentimentAnalyzer.Onnx[/]",
        "[yellow]~18 MB[/]",
        "TinyBERT (Transformer)",
        "English",
        "[green]94.2%[/]");

    table.AddRow(
        "[cyan]SentimentAnalyzer.Onnx.Multilingual[/]",
        "[red]~270 MB*[/]",
        "DistilBERT (Transformer)",
        "[green]104 languages[/]",
        "~88%");

    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("\n[dim]* Model downloaded on first use, cached locally for offline use[/]");

    var features = new Table()
        .Border(TableBorder.Rounded)
        .Title("[bold]Key Features[/]")
        .AddColumn("[bold]Feature[/]")
        .AddColumn("[bold]Description[/]");

    features.AddRow("🔌 Fully Offline", "No internet required after package install (Core/Onnx) or first use (Multilingual)");
    features.AddRow("⚡ Fast Inference", "Sub-millisecond predictions with VADER, <50ms with transformers");
    features.AddRow("📱 Cross-Platform", ".NET Standard 2.0+, .NET 8/9/10, MAUI, Blazor WASM");
    features.AddRow("🧠 Pluggable Engines", "Swap engines without changing code via builder pattern");
    features.AddRow("🔒 Privacy-First", "All processing happens on-device, no data leaves your app");

    AnsiConsole.Write(features);
}

async Task<(double ColdStart, double WarmAvg, double ThroughputPerSec)> BenchmarkEngine(
    Func<SentimentAnalyzer.SentimentAnalyzer> factory,
    string text,
    int iterations)
{
    return await Task.Run(() =>
    {
        // Cold start
        var coldSw = Stopwatch.StartNew();
        using var analyzer = factory();
        _ = analyzer.Analyze(text);
        coldSw.Stop();
        var coldStart = coldSw.Elapsed.TotalMilliseconds;

        // Warm iterations
        var warmSw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = analyzer.Analyze(text);
        }
        warmSw.Stop();

        var warmAvg = warmSw.Elapsed.TotalMilliseconds / iterations;
        var throughput = iterations / warmSw.Elapsed.TotalSeconds;

        return (coldStart, warmAvg, throughput);
    });
}

string FormatSentiment(SentimentLabel label, double confidence)
{
    var color = GetSentimentColor(label);
    var emoji = label switch
    {
        SentimentLabel.Positive => "😊",
        SentimentLabel.Negative => "😞",
        _ => "😐"
    };
    return $"[{color}]{emoji} {confidence:P0}[/]";
}

string GetSentimentColor(SentimentLabel label) => label switch
{
    SentimentLabel.Positive => "green",
    SentimentLabel.Negative => "red",
    _ => "yellow"
};
