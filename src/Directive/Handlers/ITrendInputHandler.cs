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
    /// <param name="keyword">The keyword to analyze.</param>
    /// <returns>A FatigueResult containing the analysis outcome.</returns>
    Task<FatigueResult> ProcessTrendAsync(string keyword);
}
