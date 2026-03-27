using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Result of the fatigue analysis.
/// </summary>
public record FatigueResult(string Keyword, double FatigueScore, string SaturationLevel, string RawJson);

/// <summary>
/// Interface for analyzing market fatigue from raw scraper data.
/// </summary>
public interface IFatigueAnalyzer
{
    /// <summary>
    /// Analyzes raw JSON data and calculates the saturation level.
    /// </summary>
    /// <param name="rawJson">The raw JSON string from the scraper.</param>
    /// <returns>A FatigueResult containing the saturation score and level.</returns>
    FatigueResult Analyze(string rawJson);
}
