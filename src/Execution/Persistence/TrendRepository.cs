using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dapper;
using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Execution.Persistence;

/// <summary>
/// Repository for persisting trend results in a SQLite database.
/// </summary>
public class TrendRepository : ITrendRepository
{
    private readonly string _connectionString;

    public TrendRepository(string dbPath = "antitrend.db")
    {
        _connectionString = $"Data Source={dbPath}";
    }

    public async Task InitializeAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
            CREATE TABLE IF NOT EXISTS Trends (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Keyword TEXT NOT NULL,
                FatigueIndex REAL,
                SentimentScore REAL,
                GrowthRate REAL,
                Source TEXT,
                RawJson TEXT,
                ScrapedAt DATETIME DEFAULT CURRENT_TIMESTAMP
            );";

        await connection.ExecuteAsync(sql);

        // Simple migration for Phase 4
        try { await connection.ExecuteAsync("ALTER TABLE Trends ADD COLUMN Source TEXT;"); } catch { /* Column likely exists */ }
    }

    public async Task<int> SaveTrendAsync(TrendData data)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = @"
            INSERT INTO Trends (Keyword, FatigueIndex, SentimentScore, GrowthRate, Source, RawJson) 
            VALUES (@Keyword, @FatigueIndex, @SentimentScore, @GrowthRate, @Source, @RawJson);
            SELECT last_insert_rowid();";
        return await connection.ExecuteScalarAsync<int>(sql, data);
    }

    public async Task<IEnumerable<TrendData>> GetHistoryAsync(string keyword, int limit = 50)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "SELECT * FROM Trends WHERE Keyword = @keyword ORDER BY ScrapedAt DESC LIMIT @limit";
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
