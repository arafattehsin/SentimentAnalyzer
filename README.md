# SentimentAnalyzer

[![NuGet](https://img.shields.io/nuget/v/SentimentAnalyzer.svg)](https://www.nuget.org/packages/SentimentAnalyzer/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/SentimentAnalyzer.svg)](https://www.nuget.org/packages/SentimentAnalyzer/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**On-device (offline) Sentiment Analysis for .NET applications**

A cross-platform, privacy-first library for sentiment analysis that runs entirely on-device without requiring internet connectivity. Powered by [ML.NET 5.0](https://dot.net/ml).

## ✨ Features

- 🔒 **Privacy-first** - All processing happens on-device, no data leaves your app
- 📴 **Fully offline** - No internet connection required
- 🚀 **Cross-platform** - Works on .NET Standard 2.0, .NET 8, .NET 10, MAUI, Blazor, Unity
- ⚡ **Fast** - Thread-safe singleton pattern for optimal performance
- 📦 **Lightweight** - Single NuGet package, no external dependencies

## 📦 Installation

```bash
dotnet add package SentimentAnalyzer
```

Or via Package Manager:

```powershell
Install-Package SentimentAnalyzer
```

## 🚀 Quick Start

```csharp
using SentimentAnalyzer;

// Analyze sentiment
var result = Sentiments.Predict("This product is amazing!");

Console.WriteLine($"Sentiment: {(result.Prediction ? "Positive" : "Negative")}");
Console.WriteLine($"Confidence: {result.Score:P2}");
```

## 📖 API Reference

### `Sentiments.Predict(string text)`

Analyzes the sentiment of the provided text.

**Parameters:**

- `text` - The text to analyze

**Returns:** `SentimentPrediction`

- `Prediction` (`bool`) - `true` for Positive sentiment, `false` for Negative sentiment
- `Score` (`float`) - Confidence score (0.0 to 1.0)

### Example Usage

```csharp
using SentimentAnalyzer;

// Positive sentiment
var positive = Sentiments.Predict("I love this! Best purchase ever!");
// positive.Prediction = true, positive.Score ≈ 0.95

// Negative sentiment
var negative = Sentiments.Predict("Terrible experience, very disappointed.");
// negative.Prediction = false, negative.Score ≈ 0.12
```

## 🎯 Platform Support

| Platform              | Supported | Target Framework |
| --------------------- | --------- | ---------------- |
| .NET Framework 4.6.1+ | ✅        | netstandard2.0   |
| .NET Core 2.0+        | ✅        | netstandard2.0   |
| .NET 5/6/7            | ✅        | netstandard2.0   |
| .NET 8 (LTS)          | ✅        | net8.0           |
| .NET 10 (LTS)         | ✅        | net10.0          |
| .NET MAUI             | ✅        | All targets      |
| Blazor Server         | ✅        | All targets      |
| Blazor WebAssembly    | ✅        | netstandard2.0   |
| Unity                 | ✅        | netstandard2.0   |

## 📋 Use Cases

- **Customer Feedback Analysis** - Automatically categorize reviews and feedback
- **Social Media Monitoring** - Track brand sentiment in real-time
- **Chatbot Intelligence** - Detect user mood and respond appropriately
- **Content Moderation** - Flag negative or toxic content
- **IoT/Edge Devices** - Run sentiment analysis on resource-constrained devices
- **Air-gapped Environments** - Perfect for defense, healthcare, and enterprise use

## 🔄 Migration from v1.x

Version 2.0 maintains **full backward compatibility**. No code changes required:

```xml
<!-- Just update the version -->
<PackageReference Include="SentimentAnalyzer" Version="2.0.0" />
```

See [CHANGELOG.md](CHANGELOG.md) for detailed release notes.

## 📚 Learn More

- [Blog Post: On-device Machine Learning with SentimentAnalyzer](https://www.arafattehsin.com/blog/sentimentanalyzer-ondevice-machine-learning/)
- [ML.NET Documentation](https://docs.microsoft.com/dotnet/machine-learning/)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
