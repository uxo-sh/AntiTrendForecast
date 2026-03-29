using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private double _pivotScore;

    [ObservableProperty]
    private double _confidenceScore;

    [ObservableProperty]
    private string _historicalTrend = "N/A";

    [ObservableProperty]
    private string _source = "N/A";

    [ObservableProperty]
    private bool _isAnalyzing;

    [ObservableProperty]
    private IEnumerable<double> _trendHistoryPoints = Array.Empty<double>();

    public ObservableCollection<string> RecentKeywords { get; set; } = new();

    public DashboardViewModel(ITrendInputHandler handler)
    {
        _handler = handler;
        _ = LoadRecentKeywordsAsync();
    }

    private async Task LoadRecentKeywordsAsync()
    {
        try 
        {
            var keywords = await _handler.GetRecentKeywordsAsync();
            RecentKeywords.Clear();
            foreach (var kw in keywords) RecentKeywords.Add(kw);
        }
        catch { /* Background load fail is non-critical for startup */ }
    }

    [RelayCommand]
    private async Task SelectKeywordAsync(string keyword)
    {
        Keyword = keyword;
        await AnalyzeAsync();
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
        StatusMessage = $"🔄 Analyzing multi-source fatigue for \"{Keyword}\"...";
        SaturationLevel = string.Empty;
        TrendHistoryPoints = Array.Empty<double>();

        try
        {
            var result = await _handler.ProcessTrendAsync(Keyword);
            
            SaturationLevel = result.SaturationLevel;
            SaturationScore = Math.Round(result.FatigueScore, 1);
            PivotScore = Math.Round(result.PivotScore, 1);
            ConfidenceScore = result.ConfidenceScore;
            HistoricalTrend = result.HistoricalTrend;
            Source = result.Source;
            
            // Phase 4: Update History Chart
            await UpdateHistoryChartAsync(Keyword);

            StatusMessage = $"✅ Analysis complete for \"{Keyword}\".";
            await LoadRecentKeywordsAsync();
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

    private async Task UpdateHistoryChartAsync(string keyword)
    {
        try 
        {
            var history = await _handler.GetHistoryAsync(keyword);
            TrendHistoryPoints = history.OrderBy(h => h.ScrapedAt)
                                        .Select(h => h.FatigueIndex)
                                        .ToList();
        }
        catch { /* Non-critical UI update */ }
    }
}
