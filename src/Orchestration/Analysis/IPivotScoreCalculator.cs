using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Interface for calculating the "Pivot Score" – a measure of how a trend's fatigue 
/// is changing over time based on historical data.
/// </summary>
public interface IPivotScoreCalculator
{
    /// <summary>
    /// Calculates the pivot score based on the current data point and previous history.
    /// </summary>
    /// <param name="currentResult">The most recent analysis result.</param>
    /// <param name="history">List of previous trend results for the same keyword.</param>
    PivotAnalysis CalculatePivot(FatigueResult currentResult, IEnumerable<TrendData> history);
}

public record PivotAnalysis(double PivotScore, string TrendSummary);
