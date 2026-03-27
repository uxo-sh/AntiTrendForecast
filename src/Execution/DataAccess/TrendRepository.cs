using Microsoft.Data.Sqlite;
using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Execution.DataAccess;

/// <summary>
/// SQLite repository for storing and retrieving historical trend data.
/// </summary>
public class TrendRepository
{
    private readonly string _connectionString;

    public TrendRepository(string databasePath = "antitrendforecast.db")
    {
        _connectionString = $"Data Source={databasePath}";
    }

    /// <summary>
    /// Inserts a trend data point into the database.
    /// </summary>
    public async Task InsertAsync(TrendData data)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = """
            INSERT INTO TrendData (Keyword, ScrapedAt, MentionVolume, SentimentScore, FatigueIndex, Source)
            VALUES (@Keyword, @ScrapedAt, @MentionVolume, @SentimentScore, @FatigueIndex, @Source)
            """;

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", data.Keyword);
        command.Parameters.AddWithValue("@ScrapedAt", data.ScrapedAt.ToString("o"));
        command.Parameters.AddWithValue("@MentionVolume", data.MentionVolume);
        command.Parameters.AddWithValue("@SentimentScore", data.SentimentScore);
        command.Parameters.AddWithValue("@FatigueIndex", data.FatigueIndex);
        command.Parameters.AddWithValue("@Source", data.Source);

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Retrieves all trend data for a given keyword, ordered by scrape time.
    /// </summary>
    public async Task<List<TrendData>> GetByKeywordAsync(string keyword)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "SELECT * FROM TrendData WHERE Keyword = @Keyword ORDER BY ScrapedAt DESC";

        await using var command = new SqliteCommand(sql, connection);
        command.Parameters.AddWithValue("@Keyword", keyword);

        var results = new List<TrendData>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new TrendData
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Keyword = reader.GetString(reader.GetOrdinal("Keyword")),
                ScrapedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("ScrapedAt"))),
                MentionVolume = reader.GetDouble(reader.GetOrdinal("MentionVolume")),
                SentimentScore = reader.GetDouble(reader.GetOrdinal("SentimentScore")),
                FatigueIndex = reader.GetDouble(reader.GetOrdinal("FatigueIndex")),
                Source = reader.GetString(reader.GetOrdinal("Source"))
            });
        }

        return results;
    }
}
