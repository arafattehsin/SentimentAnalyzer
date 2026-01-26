# SentimentAnalyzer Samples

This folder contains sample applications demonstrating the capabilities of the SentimentAnalyzer packages.

## 📦 Packages Overview

| Package                                 | Size             | Engine                 | Languages     | Accuracy | Best For             |
| --------------------------------------- | ---------------- | ---------------------- | ------------- | -------- | -------------------- |
| **SentimentAnalyzer.Core**              | <1 MB            | VADER (Lexicon)        | English       | ~75%     | WASM, Edge, IoT      |
| **SentimentAnalyzer.Onnx**              | ~18 MB           | TinyBERT (Transformer) | English       | 94.2%    | Mobile, Desktop      |
| **SentimentAnalyzer.Onnx.Multilingual** | ~5 MB + 270 MB\* | DistilBERT             | 104 languages | ~88%     | Server, Multilingual |

\* Model downloaded on first use, cached for offline use

---

## 🎮 Sample Applications

### 1. Console Demo (`SentimentAnalyzer.Samples.Console`)

Interactive console application with a beautiful terminal UI (powered by Spectre.Console).

**Features:**

- Quick demo comparing all 3 engines side-by-side
- Interactive mode to analyze your own text
- Built-in performance comparison tool
- Package information display

**Run it:**

```bash
cd samples/SentimentAnalyzer.Samples.Console
dotnet run
```

---

### 2. Benchmarks (`SentimentAnalyzer.Samples.Benchmarks`)

Professional-grade benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure:

- **Latency**: Time per prediction (short, medium, long texts)
- **Throughput**: Predictions per second
- **Memory**: Allocations per operation
- **Cold Start**: Initialization time

**Run it:**

```bash
cd samples/SentimentAnalyzer.Samples.Benchmarks
dotnet run -c Release
```

**Sample Results:**

| Engine     | Short Text | Medium Text | Throughput   |
| ---------- | ---------- | ----------- | ------------ |
| VADER      | ~0.01ms    | ~0.05ms     | ~100,000/sec |
| TinyBERT   | ~5ms       | ~8ms        | ~150/sec     |
| DistilBERT | ~15ms      | ~25ms       | ~50/sec      |

_(Actual results will vary based on hardware)_

---

### 3. Blazor WebAssembly (`SentimentAnalyzer.Samples.Blazor`)

A fully offline sentiment analyzer running entirely in the browser via WebAssembly.

**Features:**

- Real-time sentiment analysis
- No server required - 100% client-side
- Privacy-first: data never leaves the browser
- Uses VADER engine (perfect for WASM due to small size)

**Run it:**

```bash
cd samples/SentimentAnalyzer.Samples.Blazor
dotnet run
```

Then open https://localhost:5001

**Note:** Currently uses VADER engine only. ONNX engines require native dependencies not yet compatible with Blazor WASM.

---

### 4. MAUI Cross-Platform (`SentimentAnalyzer.Samples.Maui`)

A native cross-platform sentiment analyzer for Windows, macOS, iOS, and Android.

**Features:**

- Beautiful native UI
- On-device AI inference
- Uses TinyBERT for better accuracy (falls back to VADER if needed)
- Works offline after installation
- CommunityToolkit.Mvvm for clean architecture

**Run it:**

```bash
cd samples/SentimentAnalyzer.Samples.Maui

# Windows
dotnet run -f net10.0-windows10.0.19041.0

# macOS
dotnet run -f net10.0-maccatalyst

# Android (requires Android SDK)
dotnet run -f net10.0-android

# iOS (requires macOS + Xcode)
dotnet run -f net10.0-ios
```

---

## 🚀 Quick Start

1. **Clone the repository:**

   ```bash
   git clone https://github.com/arafattehsin/SentimentAnalyzer.git
   cd SentimentAnalyzer
   ```

2. **Restore and build:**

   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run your preferred sample:**

   ```bash
   # Console (easiest to start with)
   dotnet run --project samples/SentimentAnalyzer.Samples.Console

   # Benchmarks
   dotnet run -c Release --project samples/SentimentAnalyzer.Samples.Benchmarks

   # Blazor
   dotnet run --project samples/SentimentAnalyzer.Samples.Blazor
   ```

---

## 📊 Why Offline Sentiment Analysis?

| Feature      | Cloud APIs           | SentimentAnalyzer  |
| ------------ | -------------------- | ------------------ |
| **Latency**  | 100-500ms            | <1ms - 25ms        |
| **Privacy**  | Data sent to server  | 100% on-device     |
| **Offline**  | ❌ Requires internet | ✅ Works anywhere  |
| **Cost**     | Per-request pricing  | Free after install |
| **Edge/IoT** | ❌ Limited           | ✅ Perfect fit     |
| **WASM**     | ❌ Not possible      | ✅ Browser-native  |

---

## 🔧 Requirements

- .NET 10.0 SDK or later
- For MAUI: appropriate platform SDK (Windows SDK, Xcode, Android SDK)
- For Benchmarks: Run in Release mode for accurate results

---

## 📝 License

MIT License - See [LICENSE](../LICENSE) for details.
