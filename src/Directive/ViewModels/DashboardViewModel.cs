using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AntiTrendForecast.Directive.Handlers;

namespace AntiTrendForecast.Directive.ViewModels;

/// <summary>
/// ViewModel for the main dashboard. Manages keyword input and analysis results.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly ITrendInputHandler _handler;

    [ObservableProperty]
    private string _keyword = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Enter a keyword above to begin analysis.";

    [ObservableProperty]
    private string _saturationLevel = string.Empty;

    [ObservableProperty]
    private double _saturationScore;

    [ObservableProperty]
    private bool _isAnalyzing;

    public DashboardViewModel(ITrendInputHandler handler)
    {
        _handler = handler;
    }

    [RelayCommand]
    private async Task AnalyzeAsync()
    {
        if (string.IsNullOrWhiteSpace(Keyword))
        {
            StatusMessage = "⚠️ Please enter a keyword.";
            return;
        }

        IsAnalyzing = true;
        StatusMessage = $"🔄 Analyzing market fatigue for \"{Keyword}\"...";
        SaturationLevel = string.Empty;

        try
        {
            var result = await _handler.ProcessTrendAsync(Keyword);
            
            SaturationLevel = result.SaturationLevel;
            SaturationScore = Math.Round(result.FatigueScore, 1);
            StatusMessage = $"✅ Analysis complete for \"{Keyword}\". Score: {SaturationScore}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }
}
