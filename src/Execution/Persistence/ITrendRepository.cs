using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Execution.Persistence;

/// <summary>
/// Interface for trend data persistence in the Execution layer.
/// </summary>
public interface ITrendRepository
{
    /// <summary>
    /// Saves a new trend analysis result to the database.
    /// </summary>
    /// <param name="data">The trend data to save.</param>
    Task<int> SaveTrendAsync(TrendData data);

    /// <summary>
    /// Retrieves the historical analysis results for a specific keyword.
    /// </summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <param name="limit">Max number of results to return.</param>
    Task<IEnumerable<TrendData>> GetHistoryAsync(string keyword, int limit = 10);

    /// <summary>
    /// Gets all unique keywords that have been analyzed.
    /// </summary>
    Task<IEnumerable<string>> GetRecentKeywordsAsync(int limit = 20);

    /// <summary>
    /// Initializes the database schema.
    /// </summary>
    Task InitializeAsync();
}
