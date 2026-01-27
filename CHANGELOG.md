# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0] - 2026-01-27

### 🎉 Major Release: Multi-Engine Architecture

This release transforms SentimentAnalyzer from a single ML.NET-based package into a modular, multi-engine sentiment analysis suite with 4 distinct NuGet packages.

### New Packages

#### SentimentAnalyzer.Core 3.0.0

- **New Package**: Lightweight VADER-based sentiment analysis (<1 MB)
- Uses VaderSharp2 for rule-based sentiment scoring
- ~75% accuracy on general sentiment tasks
- No ML model files required
- Perfect for size-constrained applications

#### SentimentAnalyzer.Onnx 3.0.0

- **New Package**: High-accuracy TinyBERT transformer model (~18 MB)
- 94.2% accuracy on sentiment classification
- Embedded ONNX model (no external downloads)
- Supports .NET Standard 2.0, .NET 8, and .NET 10
- Cross-platform (Windows, Linux, macOS, iOS, Android)

#### SentimentAnalyzer.Onnx.Multilingual 3.0.0

- **New Package**: Multilingual DistilBERT model supporting 104 languages
- ~88% accuracy across multiple languages
- Downloads 270 MB model on first use (cached locally)
- Async download support with progress reporting
- Fixed HuggingFace ONNX export bug (correctly exported model hosted on GitHub Releases)

### SentimentAnalyzer 3.0.0 (Legacy)

- Maintained for backward compatibility
- Upgraded ML.NET from 1.5.5 to 5.0.0
- No API changes - drop-in replacement for v2.x
- Multi-targeting: .NET Standard 2.0, .NET 8, .NET 10

### New Features

- **Fluent Builder API**: `SentimentAnalyzer.CreateBuilder().UseVader().Build()`
- **ISentimentEngine Interface**: Create custom sentiment engines
- **Thread-Safe**: All engines are thread-safe and can be reused
- **Comprehensive Test Suite**: 58 tests across Core.Tests and integration tests
- **Sample Applications**: Console, Blazor WASM, MAUI, BenchmarkDotNet samples

### Architecture Changes

- Modular design allowing mix-and-match of engines
- Shared core abstractions (`ISentimentEngine`, `SentimentResult`, `SentimentLabel`)
- Package size optimization (choose what you need)

### Documentation

- Comprehensive README with engine comparison table
- Performance benchmarks and accuracy metrics
- Migration guide for v2.x users
- API reference for all packages

### Breaking Changes from 2.x

- New namespace structure (see migration guide)
- Builder pattern required for new engines
- Legacy `Sentiments.Predict()` API moved to SentimentAnalyzer package

### Model Hosting

- TinyBERT model: Embedded in SentimentAnalyzer.Onnx package
- DistilBERT model: Hosted on GitHub Releases (correctly exported ONNX model)
- Vocab files: Downloaded from HuggingFace

## [2.0.0] - 2026-01-22

### Added

- Multi-targeting support for `.NET Standard 2.0`, `.NET 8`, and `.NET 10`
- Package README included in NuGet package
- Additional package tags for better discoverability

### Changed

- **Upgraded ML.NET from 1.5.5 to 5.0.0** - This is a major dependency upgrade
- Updated package description and metadata
- Test project upgraded to .NET 8 with latest test framework versions

### Compatibility

This release maintains **full backward compatibility** with v1.x:

- The API (`Sentiments.Predict(text)`) remains unchanged
- The `SentimentPrediction` model is unchanged
- Existing code will work without modifications

### Migration Guide

No code changes required. Simply update your package reference:

```xml
<!-- Before -->
<PackageReference Include="SentimentAnalyzer" Version="1.2.3" />

<!-- After -->
<PackageReference Include="SentimentAnalyzer" Version="2.0.0" />
```

### Platform Support

| Platform              | Supported               |
| --------------------- | ----------------------- |
| .NET Framework 4.6.1+ | ✅ (via netstandard2.0) |
| .NET Core 2.0+        | ✅ (via netstandard2.0) |
| .NET 5/6/7            | ✅ (via netstandard2.0) |
| .NET 8 (LTS)          | ✅ Native               |
| .NET 10 (LTS)         | ✅ Native               |
| .NET MAUI             | ✅                      |
| Blazor (Server/WASM)  | ✅                      |
| Unity                 | ✅ (via netstandard2.0) |

---

## [1.2.3] - 2021-xx-xx

### Changed

- Performance improvements using singleton pattern
- Based on ML.NET version 1.5.5

## [1.0.0] - 2020-xx-xx

### Added

- Initial release
- Binary sentiment analysis (positive/negative)
- Offline, on-device prediction
- Thread-safe singleton pattern
