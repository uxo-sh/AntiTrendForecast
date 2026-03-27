using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Result of the fatigue analysis, including historical context.
/// </summary>
public record FatigueResult(
    string Keyword, 
    double FatigueScore, 
    string SaturationLevel, 
    string RawJson,
    double PivotScore = 0,
    string HistoricalTrend = "N/A");

/// <summary>
/// Interface for analyzing market fatigue from raw scraper data.
/// </summary>
public interface IFatigueAnalyzer
{
    /// <summary>
    /// Analyzes raw JSON data and calculates the saturation level, optionally considering history.
    /// </summary>
    /// <param name="rawJson">The raw JSON string from the scraper.</param>
    /// <param name="history">Optional historical trend records.</param>
    /// <returns>A FatigueResult containing the saturation score, level, and pivot analysis.</returns>
    FatigueResult Analyze(string rawJson, IEnumerable<TrendData> history);
}
