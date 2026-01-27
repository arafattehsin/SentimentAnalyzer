# SentimentAnalyzer

[![NuGet](https://img.shields.io/nuget/v/SentimentAnalyzer.svg)](https://www.nuget.org/packages/SentimentAnalyzer/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SentimentAnalyzer.svg)](https://www.nuget.org/packages/SentimentAnalyzer/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**On-device (offline) Sentiment Analysis for .NET applications**

A cross-platform, privacy-first library for sentiment analysis that runs entirely on-device without requiring internet connectivity.

## ✨ Features

- 🔒 **Privacy-first** - All processing happens on-device
- 📴 **Fully offline** - No internet connection required
- 🚀 **Cross-platform** - .NET Standard 2.0, .NET 8, .NET 10, MAUI, Blazor
- ⚡ **Multiple engines** - Choose between speed, accuracy, or both
- 🌍 **Multilingual** - Support for 104 languages

---

## 🚀 Quick Start

### Option A: Lightweight (VADER) - Recommended for most apps

```bash
dotnet add package SentimentAnalyzer.Core
```

```csharp
using SentimentAnalyzer;

// Simple usage
var analyzer = new SentimentAnalyzer();
var result = analyzer.Analyze("I love this product!");

Console.WriteLine($"Sentiment: {result.Label}");     // Positive
Console.WriteLine($"Score: {result.Score:F2}");      // 0.87
Console.WriteLine($"Confidence: {result.Confidence:F2}");  // 0.74
```

### Option B: High Accuracy (TinyBERT) - English Only

```bash
dotnet add package SentimentAnalyzer.Core
dotnet add package SentimentAnalyzer.Onnx
```

```csharp
using SentimentAnalyzer;
using SentimentAnalyzer.Onnx;

// Use TinyBERT for higher accuracy (embedded model, ~18MB)
var analyzer = SentimentAnalyzer.CreateBuilder()
    .UseTinyBert()
    .Build();

var result = analyzer.Analyze("This product is amazing!");
Console.WriteLine($"Sentiment: {result.Label}");  // Positive
Console.WriteLine($"Confidence: {result.Confidence:P0}");  // 99%
```

### Option C: Multilingual (DistilBERT) - 104 Languages

```bash
dotnet add package SentimentAnalyzer.Core
dotnet add package SentimentAnalyzer.Onnx.Multilingual
```

```csharp
using SentimentAnalyzer;
using SentimentAnalyzer.Onnx.Multilingual;

// Use DistilBERT for multilingual support (downloads ~270MB model on first use)
var analyzer = SentimentAnalyzer.CreateBuilder()
    .UseDistilBert()
    .Build();

// Supports 104 languages!
var result = analyzer.Analyze("Ce produit est fantastique!");  // French
Console.WriteLine($"Sentiment: {result.Label}");  // Positive

// Also works with: Arabic, Chinese, German, Spanish, Hindi, Japanese, etc.
```

> **📥 Note on DistilBERT Multilingual**: The ~270MB model is automatically downloaded from GitHub Releases on first use and cached locally at `%LOCALAPPDATA%\SentimentAnalyzer\models\`. Subsequent uses are fully offline. [See full list of 104 supported languages](https://github.com/google-research/bert/blob/master/multilingual.md#list-of-languages).

### Option D: Legacy API (Backward Compatible)

```bash
dotnet add package SentimentAnalyzer
```

```csharp
using SentimentAnalyzer;

// v1.x/v2.x API still works
var result = Sentiments.Predict("This product is amazing!");
Console.WriteLine($"Positive: {result.Prediction}");  // true
```

---

## � Choosing the Right Package

### Package Comparison

| Package                                 | Size   | Engine                   | Languages     | Accuracy | Speed      | Best For                         |
| --------------------------------------- | ------ | ------------------------ | ------------- | -------- | ---------- | -------------------------------- |
| **SentimentAnalyzer.Core**              | <1MB   | VADER (rule-based)       | English       | ~80%     | ⚡⚡⚡⚡⚡ | Mobile, IoT, speed-critical apps |
| **SentimentAnalyzer.Onnx**              | ~18MB  | TinyBERT (transformer)   | English       | ~94%     | ⚡⚡⚡⚡   | High accuracy, embedded model    |
| **SentimentAnalyzer.Onnx.Multilingual** | ~541MB | DistilBERT (transformer) | 104 languages | ~83%     | ⚡⚡⚡     | Multilingual, highest accuracy   |
| **SentimentAnalyzer**                   | ~15MB  | ML.NET (traditional ML)  | English       | ~85%     | ⚡⚡⚡⚡   | Backward compatibility           |

_Speed ratings: ⚡⚡⚡⚡⚡ (fastest) to ⚡ (slowest)_

### Platform Support

| Platform          | Core | Onnx (TinyBERT) | Onnx.Multilingual | Legacy |
| ----------------- | ---- | --------------- | ----------------- | ------ |
| .NET Standard 2.0 | ✅   | ✅              | ✅                | ✅     |
| .NET 8 (LTS)      | ✅   | ✅              | ✅                | ✅     |
| .NET 10 (LTS)     | ✅   | ✅              | ✅                | ✅     |
| .NET MAUI         | ✅   | ✅              | ✅                | ✅     |
| Blazor WASM       | ✅   | ❌              | ❌                | ✅     |
| Blazor Server     | ✅   | ✅              | ✅                | ✅     |
| Unity             | ✅   | ⚠️              | ❌                | ✅     |

> **Note**: ONNX models require native C++ libraries and cannot run in Blazor WebAssembly. Use Core (VADER) for WASM or Blazor Server for ML models.

### Use Cases

| Use Case                    | Recommended Package                                  |
| --------------------------- | ---------------------------------------------------- |
| Mobile apps (MAUI, Xamarin) | Core (VADER) or Onnx (TinyBERT)                      |
| IoT/Edge devices            | Core (VADER)                                         |
| Blazor WebAssembly          | Core (VADER)                                         |
| Speed-critical applications | Core (VADER)                                         |
| High accuracy (English)     | Onnx (TinyBERT)                                      |
| Multilingual support        | Onnx.Multilingual (DistilBERT)                       |
| Social media analysis       | Core (VADER - optimized for social text with emojis) |
| Customer reviews            | Onnx (TinyBERT) or Onnx.Multilingual                 |
| Enterprise/existing apps    | Legacy (ML.NET)                                      |

---

## �📖 API Reference

### Core Package

#### SentimentAnalyzer Class

```csharp
// Default constructor (uses VADER engine)
var analyzer = new SentimentAnalyzer();

// With options
var analyzer = new SentimentAnalyzer(SentimentAnalyzerOptions.Binary);

// Builder pattern
var analyzer = SentimentAnalyzer.CreateBuilder()
    .WithNeutralThresholds(0.3, 0.7)  // Customize neutral zone
    .Build();
```

#### SentimentResult

| Property     | Type           | Description                                     |
| ------------ | -------------- | ----------------------------------------------- |
| `Text`       | string         | Original input text                             |
| `Label`      | SentimentLabel | Positive, Neutral, or Negative                  |
| `Score`      | double         | Normalized score (0-1)                          |
| `Confidence` | double         | Prediction confidence (0-1)                     |
| `Engine`     | string         | Engine name ("Vader", "TinyBert", "DistilBert") |
| `IsPositive` | bool           | Shorthand for Label == Positive                 |
| `IsNegative` | bool           | Shorthand for Label == Negative                 |
| `IsNeutral`  | bool           | Shorthand for Label == Neutral                  |

#### SentimentAnalyzerOptions

```csharp
// Presets
SentimentAnalyzerOptions.Default   // Neutral zone: 0.4-0.6
SentimentAnalyzerOptions.Binary    // No neutral (positive/negative only)
SentimentAnalyzerOptions.Strict    // Narrow neutral: 0.45-0.55
SentimentAnalyzerOptions.Lenient   // Wide neutral: 0.3-0.7
```

#### Batch Processing

```csharp
var analyzer = new SentimentAnalyzer();

var reviews = new[] {
    "Great product!",
    "Terrible service.",
    "It's okay I guess."
};

var results = analyzer.AnalyzeBatch(reviews);
foreach (var result in results)
{
    Console.WriteLine($"{result.Label}: {result.Text}");
}
```

---

## 🛠️ Advanced Usage

### Custom Engine

````csharp
// Implement ISentimentEngine for custom engines
public class MyCustomEngine : ISentimentEngine
{
    public string Name => "Custom";

    public SentimentResult Analyze(string text) { /* ... */ }
    public IReadOnlyList<SentimentResult> AnalyzeBatch(IEnumerable<string> texts) { /* ... */ }
    public void Dispose() { }
}

// Use with builder
var analyzer = SentimentAnalyzer.CreateBuilder()
    .WithEngine(_ => new MyCustomEngine())
    .Build();
```

### TinyBERT with Custom Options

```csharp
using SentimentAnalyzer.Onnx;
using Microsoft.ML.OnnxRuntime;

// Configure ONNX Runtime session options
var sessionOptions = new SessionOptions
{
    GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
    ExecutionMode = ExecutionMode.ORT_PARALLEL
};

var analyzer = SentimentAnalyzer.CreateBuilder()
    .UseTinyBert(sessionOptions)
    .WithBinaryClassification()  // Disable neutral classification
    .Build();
````

### DistilBERT Multilingual with Progress

```csharp
using SentimentAnalyzer.Onnx.Multilingual;

var progress = new Progress<double>(p =>
    Console.WriteLine($"Downloading model: {p:P0}"));

var analyzer = await SentimentAnalyzer.CreateBuilder()
    .UseDistilBertMultilingualAsync(progress: progress);

// Model is now cached locally, subsequent uses are offline
```

---

## ⚡ Performance Comparison

| Engine          | Package           | Accuracy | Speed      | Size   | Languages |
| --------------- | ----------------- | -------- | ---------- | ------ | --------- |
| VADER           | Core              | ~80%     | ⚡⚡⚡⚡⚡ | <1MB   | English   |
| TinyBERT        | Onnx              | ~94%     | ⚡⚡⚡⚡   | ~18MB  | English   |
| DistilBERT      | Onnx.Multilingual | ~83%     | ⚡⚡⚡     | ~541MB | 104       |
| ML.NET (Legacy) | SentimentAnalyzer | ~85%     | ⚡⚡⚡⚡   | ~15MB  | English   |

_Speed ratings: ⚡⚡⚡⚡⚡ (fastest) to ⚡ (slowest). Accuracy tested on various datasets._
� Migration from v2.x

### If using `Sentiments.Predict()`:

No changes needed! The legacy API continues to work:

```xml
<PackageReference Include="SentimentAnalyzer" Version="3.0.0" />
```

### To use new engines (smaller, faster, or more accurate):

```csharp
// Before (ML.NET, ~15MB)
var result = Sentiments.Predict("I love this!");

// After (VADER, <1MB - fastest)
var analyzer = new SentimentAnalyzer();
var result = analyzer.Analyze("I love this!");

// Result now has more properties:
// result.Label (enum), result.Confidence, result.IsPositive, etc.
```

---

## 📚 Learn More

- [Blog Post: SentimentAnalyzer v3.0 Modernization](https://arafattehsin.com/blog/sentiment-analyzer-modernization)
- [Blog Post: On-device Machine Learning with SentimentAnalyzer](https://www.arafattehsin.com/blog/sentimentanalyzer-ondevice-machine-learning/)
- [Sample Applications](./samples/) - Console, Blazor, MAUI examples
- [CHANGELOG](CHANGELOG.md) - Version history and migration guides
- [GitHub Releases](https://github.com/arafattehsin/SentimentAnalyzer/releases) - Model downloads

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
