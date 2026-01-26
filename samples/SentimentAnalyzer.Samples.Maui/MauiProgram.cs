using CommunityToolkit.Maui;
using global::SentimentAnalyzer.Onnx;
using global::SentimentAnalyzer.Onnx.Multilingual;
using SA = global::SentimentAnalyzer;

namespace SentimentAnalyzer.Samples.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddTransient<App>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainViewModel>();

        // Register SentimentAnalyzerFactory for multiple engine support
        builder.Services.AddSingleton<SentimentAnalyzerFactory>();

        return builder.Build();
    }
}
