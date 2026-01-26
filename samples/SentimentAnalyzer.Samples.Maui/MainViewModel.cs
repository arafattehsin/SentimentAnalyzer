using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SentimentAnalyzer;
using SA = global::SentimentAnalyzer;

namespace SentimentAnalyzer.Samples.Maui;

public partial class MainViewModel : ObservableObject
{
    private readonly SentimentAnalyzerFactory _factory;
    private SA.SentimentAnalyzer _analyzer;

    public MainViewModel(SentimentAnalyzerFactory factory)
    {
        _factory = factory;
        _selectedEngine = "VADER"; // Default to fast engine
        _analyzer = _factory.GetAnalyzer(_selectedEngine);
    }

    public IEnumerable<string> AvailableEngines => _factory.AvailableEngines;

    [ObservableProperty]
    private string _selectedEngine;

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private bool _isDownloading;

    partial void OnSelectedEngineChanged(string value)
    {
        // Fire and forget - we handle errors internally
        _ = LoadEngineAsync(value);
    }

    private async Task LoadEngineAsync(string engineName)
    {
        try
        {
            ErrorText = string.Empty;
            IsLoading = true;
            IsDownloading = engineName == "DistilBERT" && !_factory.IsDistilBertCached;
            DownloadProgress = 0;
            LoadingText = $"Loading {engineName}...";

            var progress = new Progress<(double progress, string status)>(p =>
            {
                DownloadProgress = p.progress;
                LoadingText = p.status;
            });

            // Load analyzer async (handles download for DistilBERT)
            _analyzer = await _factory.GetAnalyzerAsync(engineName, progress);
            OnPropertyChanged(nameof(EngineName));

            // Re-analyze current text if we have a result
            if (HasResult && !string.IsNullOrWhiteSpace(InputText))
            {
                Analyze();
            }
        }
        catch (Exception ex)
        {
            ErrorText = $"Failed to load {engineName}: {ex.Message}";
            _selectedEngine = "VADER";
            _analyzer = _factory.GetAnalyzer("VADER");
            OnPropertyChanged(nameof(SelectedEngine));
            OnPropertyChanged(nameof(EngineName));
        }
        finally
        {
            IsLoading = false;
            IsDownloading = false;
        }
    }

    public string EngineName => $"Engine: {_analyzer.Name}";

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _hasResult;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _loadingText = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorText = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorText);

    [ObservableProperty]
    private string _resultLabel = string.Empty;

    [ObservableProperty]
    private string _resultEmoji = string.Empty;

    [ObservableProperty]
    private string _confidenceText = string.Empty;

    [ObservableProperty]
    private double _score;

    [ObservableProperty]
    private string _scoreText = string.Empty;

    [ObservableProperty]
    private string _timingText = string.Empty;

    [ObservableProperty]
    private Color _resultBackgroundColor = Colors.Transparent;

    [RelayCommand]
    private void Analyze()
    {
        if (string.IsNullOrWhiteSpace(InputText))
            return;

        ErrorText = string.Empty;
        var sw = Stopwatch.StartNew();
        var result = _analyzer.Analyze(InputText);
        sw.Stop();

        // Update UI
        HasResult = true;
        ResultLabel = result.Label.ToString();
        ConfidenceText = $"Confidence: {result.Confidence:P0}";

        ResultEmoji = result.Label switch
        {
            SentimentLabel.Positive => "😊",
            SentimentLabel.Negative => "😞",
            _ => "😐"
        };

        // Use theme-aware colors
        var isDark = Application.Current?.RequestedTheme == AppTheme.Dark;
        ResultBackgroundColor = result.Label switch
        {
            SentimentLabel.Positive => Color.FromArgb(isDark ? "#1B3D2F" : "#d4edda"),
            SentimentLabel.Negative => Color.FromArgb(isDark ? "#3D1F22" : "#f8d7da"),
            _ => Color.FromArgb(isDark ? "#3D3520" : "#fff3cd")
        };

        Score = result.Score;
        ScoreText = $"Score: {result.Score:F3}";

        TimingText = $"⏱️ Analyzed in {sw.Elapsed.TotalMilliseconds:F2}ms";
    }

    [RelayCommand]
    private void Clear()
    {
        InputText = string.Empty;
        HasResult = false;
        ErrorText = string.Empty;
    }

    [RelayCommand]
    private void TryExample(string example)
    {
        InputText = example;
        Analyze();
    }
}
