using AntiTrendForecast.Execution.Models;
using Microsoft.Extensions.Logging;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Implementation of the Pivot Score algorithm.
/// Measures the drift between current fatigue and historical averages.
/// </summary>
public class PivotScoreCalculator : IPivotScoreCalculator
{
    private readonly ILogger<PivotScoreCalculator> _logger;

    public PivotScoreCalculator(ILogger<PivotScoreCalculator> logger)
    {
        _logger = logger;
    }

    public PivotAnalysis CalculatePivot(FatigueResult current, IEnumerable<TrendData> history)
    {
        var historyList = history.ToList();
        if (!historyList.Any())
        {
            return new PivotAnalysis(0, "First Analysis - Baseline Established");
        }

        // Calculate average fatigue over the last 5 snapshots
        double avgFatigue = historyList.Average(h => h.FatigueIndex);
        double fatigueDrift = current.FatigueScore - avgFatigue;

        // Current Pivot Logic:
        // + Score: Fatigue is accelerating (Danger - Peak approached)
        // - Score: Fatigue is decelerating (Opportunity - Potential Pivot)
        
        string summary;
        if (fatigueDrift > 15)
            summary = "Rapid Saturation - Market Fatigue Accelerating";
        else if (fatigueDrift > 5)
            summary = "Increasing Fatigue - Trend approaching maturity";
        else if (fatigueDrift < -15)
            summary = "Cooling Down - Potential Pivot Opportunity";
        else if (fatigueDrift < -5)
            summary = "Stable Interest - Fatigue levels normalizing";
        else
            summary = "Sustained Momentum - Minimal drift detected";

        _logger.LogInformation("Calculated Pivot Score: {Drift} for {Keyword}", fatigueDrift, current.Keyword);

        return new PivotAnalysis(fatigueDrift, summary);
    }
}
