using Microsoft.UI.Xaml;
using Serilog;

namespace AntiTrendForecast.Directive;

/// <summary>
/// Application entry point. Bootstraps Serilog logging and launches the main window.
/// </summary>
public partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs/antitrendforecast-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        Log.Information("AntiTrendForecast application starting.");
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();
    }

    private Window? _mainWindow;
}
