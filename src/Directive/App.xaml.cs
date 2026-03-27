using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using AntiTrendForecast.Directive.Handlers;
using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Python;
using AntiTrendForecast.Execution.Persistence;
using Path = System.IO.Path;

namespace AntiTrendForecast.Directive;

/// <summary>
/// Interaction logic for App.xaml (WPF version)
/// </summary>
public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Setup Serilog
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "antitrendforecast-.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logPath, 
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // 2. Setup Dependency Injection
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        Services = serviceCollection.BuildServiceProvider();

        Log.Information("Anti-Trend Forecast Engine application (WPF) initialized.");

        // 3. Initialize Database and Show Main Window
        try 
        {
            var repo = Services?.GetRequiredService<ITrendRepository>();
            if (repo != null) await repo.InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize database.");
        }

        var mainWindow = Services?.GetRequiredService<MainWindow>();
        mainWindow?.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Infrastructure
        services.AddLogging(builder => 
        {
            builder.AddSerilog(dispose: true);
        });

        // Layer 1 - Directive
        services.AddTransient<ITrendInputHandler, TrendInputHandler>();
        services.AddTransient<MainWindow>();

        // Layer 2 - Orchestration
        services.AddSingleton<IPivotScoreCalculator, PivotScoreCalculator>();
        services.AddSingleton<IFatigueAnalyzer, FatigueAnalyzer>();

        // Layer 3 - Execution
        services.AddSingleton<IPythonRunnerService, PythonRunnerService>();
        services.AddSingleton<ITrendRepository, TrendRepository>();
    }
}
