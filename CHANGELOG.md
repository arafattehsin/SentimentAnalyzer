# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
| Platform | Supported |
|----------|-----------|
| .NET Framework 4.6.1+ | ✅ (via netstandard2.0) |
| .NET Core 2.0+ | ✅ (via netstandard2.0) |
| .NET 5/6/7 | ✅ (via netstandard2.0) |
| .NET 8 (LTS) | ✅ Native |
| .NET 10 (LTS) | ✅ Native |
| .NET MAUI | ✅ |
| Blazor (Server/WASM) | ✅ |
| Unity | ✅ (via netstandard2.0) |

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
