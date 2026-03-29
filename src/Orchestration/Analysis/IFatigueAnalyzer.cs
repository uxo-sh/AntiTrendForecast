using AntiTrendForecast.Execution.Models;
using System.Collections.Generic;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Result of a fatigue analysis, including saturation and potential pivot markers.
/// </summary>
public record FatigueResult(
    string Keyword, 
    double FatigueScore, 
    string SaturationLevel, 
    string Source,
    double PivotScore = 0,
    string HistoricalTrend = "N/A",
    double ConfidenceScore = 0);

/// <summary>
/// Interface for analyzing market fatigue from raw scraper data.
/// </summary>
public interface IFatigueAnalyzer
{
    /// <summary>
    /// Analyzes raw JSON data and produces a fatigue result.
    /// </summary>
    FatigueResult Analyze(string rawJson);

    /// <summary>
    /// Analyzes raw JSON data with historical context.
    /// </summary>
    FatigueResult Analyze(string rawJson, IEnumerable<TrendData> history);
}
