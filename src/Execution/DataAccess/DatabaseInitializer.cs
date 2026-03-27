using Microsoft.Data.Sqlite;

namespace AntiTrendForecast.Execution.DataAccess;

/// <summary>
/// Initializes the SQLite database schema. Creates tables if they don't exist.
/// </summary>
public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(string databasePath = "antitrendforecast.db")
    {
        _connectionString = $"Data Source={databasePath}";
    }

    /// <summary>
    /// Creates the database schema (TrendData table).
    /// </summary>
    public async Task InitializeAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            CREATE TABLE IF NOT EXISTS TrendData (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                Keyword         TEXT    NOT NULL,
                ScrapedAt       TEXT    NOT NULL,
                MentionVolume   REAL    NOT NULL DEFAULT 0,
                SentimentScore  REAL    NOT NULL DEFAULT 0,
                FatigueIndex    REAL    NOT NULL DEFAULT 0,
                Source          TEXT    NOT NULL DEFAULT ''
            );

            CREATE INDEX IF NOT EXISTS IX_TrendData_Keyword ON TrendData(Keyword);
            CREATE INDEX IF NOT EXISTS IX_TrendData_ScrapedAt ON TrendData(ScrapedAt);
            """;

        await using var command = new SqliteCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
