using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AntiTrendForecast.Directive.Services;

namespace AntiTrendForecast.Directive.ViewModels;

/// <summary>
/// ViewModel for the main dashboard. Manages keyword input and analysis state.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly InputValidator _validator = new();

    [ObservableProperty]
    public partial string Keyword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = "Enter a keyword above to begin analysis.";

    [ObservableProperty]
    public partial bool IsAnalyzing { get; set; }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        var result = _validator.ValidateKeyword(Keyword);
        if (!result.IsValid)
        {
            StatusMessage = $"⚠️ {result.ErrorMessage}";
            return;
        }

        IsAnalyzing = true;
        StatusMessage = $"🔄 Analyzing trend: \"{Keyword}\"...";

        // TODO: Call PipelineOrchestrator from Orchestration layer
        await Task.Delay(1000); // Placeholder

        StatusMessage = $"✅ Analysis complete for \"{Keyword}\".";
        IsAnalyzing = false;
    }
}
