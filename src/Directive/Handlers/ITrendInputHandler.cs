using AntiTrendForecast.Orchestration.Analysis;

namespace AntiTrendForecast.Directive.Handlers;

/// <summary>
/// Interface for handling trend keyword inputs and coordinating analysis.
/// </summary>
public interface ITrendInputHandler
{
    /// <summary>
    /// Processes a single trend keyword through the 3-layer pipeline.
    /// </summary>
    Task<FatigueResult> ProcessTrendAsync(string keyword);

    /// <summary>
    /// Gets the most recently analyzed unique keywords.
    /// </summary>
    Task<IEnumerable<string>> GetRecentKeywordsAsync();
}
