using Android.App;
using Android.Runtime;

namespace SentimentAnalyzer.Samples.Maui;

[Application(
    UsesCleartextTraffic = false,
    NetworkSecurityConfig = "@xml/network_security_config")]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
