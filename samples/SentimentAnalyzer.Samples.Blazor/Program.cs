using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SentimentAnalyzer.Samples.Blazor;
using SA = global::SentimentAnalyzer;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register SentimentAnalyzer with VADER engine (optimized for WASM)
// Note: ONNX engines (TinyBERT, DistilBERT) require native dependencies
// and are not compatible with Blazor WASM. Use Blazor Server for ML models.
builder.Services.AddSingleton<SA.SentimentAnalyzer>(_ =>
    SA.SentimentAnalyzer.CreateBuilder()
        .Build()); // VADER engine: <1MB, fast, WASM-compatible

await builder.Build().RunAsync();
