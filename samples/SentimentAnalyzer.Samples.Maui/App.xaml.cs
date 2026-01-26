namespace SentimentAnalyzer.Samples.Maui;

public partial class App : Application
{
    private readonly MainPage _mainPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();
        _mainPage = mainPage;

        // Apply theme colors based on system theme
        ApplyThemeColors();
        Application.Current!.RequestedThemeChanged += (s, e) => ApplyThemeColors();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(_mainPage));
    }

    private void ApplyThemeColors()
    {
        var isDark = Application.Current!.RequestedTheme == AppTheme.Dark;
        var resources = Application.Current.Resources;

        resources["PageBackgroundColor"] = resources[isDark ? "PageBackgroundColorDark" : "PageBackgroundColorLight"];
        resources["CardBackgroundColor"] = resources[isDark ? "CardBackgroundColorDark" : "CardBackgroundColorLight"];
        resources["TextColor"] = resources[isDark ? "TextColorDark" : "TextColorLight"];
        resources["TextSecondaryColor"] = resources[isDark ? "TextSecondaryColorDark" : "TextSecondaryColorLight"];
        resources["EditorBackgroundColor"] = resources[isDark ? "EditorBackgroundColorDark" : "EditorBackgroundColorLight"];
        resources["EditorBorderColor"] = resources[isDark ? "EditorBorderColorDark" : "EditorBorderColorLight"];
    }
}
