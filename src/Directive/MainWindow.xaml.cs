using Microsoft.UI.Xaml;
using AntiTrendForecast.Directive.Services;
using Serilog;

namespace AntiTrendForecast.Directive;

/// <summary>
/// Main dashboard window. Captures user keyword input and triggers analysis.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly InputValidator _validator = new();

    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void OnAnalyzeClicked(object sender, RoutedEventArgs e)
    {
        var keyword = KeywordInput.Text?.Trim() ?? string.Empty;

        var validationResult = _validator.ValidateKeyword(keyword);
        if (!validationResult.IsValid)
        {
            StatusText.Text = $"⚠️ {validationResult.ErrorMessage}";
            Log.Warning("Invalid keyword input: {Keyword} — {Error}", keyword, validationResult.ErrorMessage);
            return;
        }

        StatusText.Text = $"🔄 Analyzing trend: \"{keyword}\"...";
        Log.Information("Analysis requested for keyword: {Keyword}", keyword);

        // TODO: Wire up Orchestration layer pipeline
    }
}
