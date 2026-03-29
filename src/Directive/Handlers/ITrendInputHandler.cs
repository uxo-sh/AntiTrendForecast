using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntiTrendForecast.Directive.Handlers;

/// <summary>
/// Interface for handling the coordination between high-level user input and low-level trend processing.
/// </summary>
public interface ITrendInputHandler
{
    /// <summary>
    /// Processes a keyword and returns the analyzed fatigue result.
    /// </summary>
    Task<FatigueResult> ProcessTrendAsync(string keyword);

    /// <summary>
    /// Gets the most recently analyzed unique keywords.
    /// </summary>
    Task<IEnumerable<string>> GetRecentKeywordsAsync();
    
    /// <summary>
    /// Gets the historical trend data for a keyword.
    /// </summary>
    Task<IEnumerable<TrendData>> GetHistoryAsync(string keyword);
}
