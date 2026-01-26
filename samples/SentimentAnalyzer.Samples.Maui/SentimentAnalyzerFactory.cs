using global::SentimentAnalyzer.Onnx;
using global::SentimentAnalyzer.Onnx.Multilingual;
using SA = global::SentimentAnalyzer;

namespace SentimentAnalyzer.Samples.Maui;

/// <summary>
/// Factory for creating SentimentAnalyzer instances with different engines.
/// </summary>
public class SentimentAnalyzerFactory
{
    private readonly Dictionary<string, Lazy<SA.SentimentAnalyzer>> _analyzers = new();
    private SA.SentimentAnalyzer? _distilBertAnalyzer;

    public SentimentAnalyzerFactory()
    {
        // VADER - Fast, rule-based (always available)
        _analyzers["VADER"] = new Lazy<SA.SentimentAnalyzer>(() =>
            SA.SentimentAnalyzer.CreateBuilder().Build());

        // TinyBERT - Fast ML model
        _analyzers["TinyBERT"] = new Lazy<SA.SentimentAnalyzer>(() =>
            SA.SentimentAnalyzer.CreateBuilder().UseTinyBert().Build());
    }

    public List<string> AvailableEngines => new() { "VADER", "TinyBERT", "DistilBERT" };

    /// <summary>
    /// Checks if DistilBERT model is already downloaded.
    /// </summary>
    public bool IsDistilBertCached => ModelDownloader.IsModelCached();

    public SA.SentimentAnalyzer GetAnalyzer(string engineName)
    {
        if (engineName == "DistilBERT")
        {
            if (_distilBertAnalyzer == null)
            {
                throw new InvalidOperationException("DistilBERT must be loaded async. Use GetAnalyzerAsync instead.");
            }
            return _distilBertAnalyzer;
        }

        if (_analyzers.TryGetValue(engineName, out var lazy))
        {
            return lazy.Value;
        }
        throw new ArgumentException($"Unknown engine: {engineName}");
    }

    /// <summary>
    /// Gets an analyzer asynchronously with progress reporting (required for DistilBERT).
    /// </summary>
    public async Task<SA.SentimentAnalyzer> GetAnalyzerAsync(
        string engineName,
        IProgress<(double progress, string status)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (engineName != "DistilBERT")
        {
            return GetAnalyzer(engineName);
        }

        // Return cached instance if available
        if (_distilBertAnalyzer != null)
        {
            return _distilBertAnalyzer;
        }

        // Check if model is already downloaded
        if (ModelDownloader.IsModelCached())
        {
            progress?.Report((0.9, "Loading model..."));
            _distilBertAnalyzer = SA.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build();
            progress?.Report((1.0, "Ready"));
            return _distilBertAnalyzer;
        }

        // Download with progress
        progress?.Report((0.0, "Downloading DistilBERT model (~270MB)..."));

        try
        {
            var downloadProgress = new Progress<double>(p =>
            {
                var percent = (int)(p * 100);
                progress?.Report((p * 0.9, $"Downloading model... {percent}%"));
            });

            await ModelDownloader.EnsureModelAsync(downloadProgress, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Network error downloading model: {ex.Message}\nInner: {ex.InnerException?.Message}", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                "Download timed out. The model is ~270MB. Please try again with a stable connection.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to download: {ex.GetType().Name}: {ex.Message}", ex);
        }

        progress?.Report((0.9, "Loading model..."));
        _distilBertAnalyzer = SA.SentimentAnalyzer.CreateBuilder().UseDistilBert().Build();
        progress?.Report((1.0, "Ready"));

        return _distilBertAnalyzer;
    }
}
