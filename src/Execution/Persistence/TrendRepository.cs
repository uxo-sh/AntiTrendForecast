using Microsoft.Data.Sqlite;
using Dapper;
using AntiTrendForecast.Execution.Models;
using Microsoft.Extensions.Logging;

namespace AntiTrendForecast.Execution.Persistence;

/// <summary>
/// Implementation of ITrendRepository using SQLite and Dapper.
/// </summary>
public class TrendRepository : ITrendRepository
{
    private readonly ILogger<TrendRepository> _logger;
    private readonly string _connectionString;

    public TrendRepository(ILogger<TrendRepository> logger)
    {
        _logger = logger;
        // SQLite database file is relative to the execution directory
        var dbPath = Path.Combine(AppContext.BaseDirectory, "antitrend.db");
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing SQLite database at {Path}", _connectionString);

        using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Trends (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Keyword TEXT NOT NULL,
                ScrapedAt DATETIME NOT NULL,
                MentionVolume REAL NOT NULL,
                SentimentScore REAL NOT NULL,
                FatigueIndex REAL NOT NULL,
                Source TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS IDX_Trends_Keyword ON Trends(Keyword);
        ");
    }

    public async Task<int> SaveTrendAsync(TrendData data)
    {
        _logger.LogDebug("Saving trend analysis for keyword '{Keyword}'", data.Keyword);

        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            INSERT INTO Trends (Keyword, ScrapedAt, MentionVolume, SentimentScore, FatigueIndex, Source)
            VALUES (@Keyword, @ScrapedAt, @MentionVolume, @SentimentScore, @FatigueIndex, @Source);
            SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(sql, data);
    }

    public async Task<IEnumerable<TrendData>> GetHistoryAsync(string keyword, int limit = 10)
    {
        _logger.LogDebug("Retrieving history for keyword '{Keyword}'", keyword);

        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            SELECT * FROM Trends 
            WHERE Keyword = @keyword 
            ORDER BY ScrapedAt DESC 
            LIMIT @limit";

        return await connection.QueryAsync<TrendData>(sql, new { keyword, limit });
    }

    public async Task<IEnumerable<string>> GetRecentKeywordsAsync(int limit = 20)
    {
        using var connection = new SqliteConnection(_connectionString);
        // Get the latest unique keywords by looking at the max ScrapedAt for each
        const string sql = @"
            SELECT Keyword 
            FROM Trends 
            GROUP BY Keyword 
            ORDER BY MAX(ScrapedAt) DESC 
            LIMIT @limit";
        return await connection.QueryAsync<string>(sql, new { limit });
    }
}
