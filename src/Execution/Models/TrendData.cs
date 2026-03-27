namespace AntiTrendForecast.Execution.Models;

/// <summary>
/// Represents a data point capturing a trend's metrics at a moment in time.
/// </summary>
public class TrendData
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Normalized mention volume (0.0 – 1.0).
    /// </summary>
    public double MentionVolume { get; set; }

    /// <summary>
    /// Sentiment score from ML model (-1.0 negative to +1.0 positive).
    /// </summary>
    public double SentimentScore { get; set; }

    /// <summary>
    /// Calculated fatigue index (0.0 – 100.0). Higher = more saturated.
    /// </summary>
    public double FatigueIndex { get; set; }

    /// <summary>
    /// Source platform (e.g., "reddit", "twitter", "hackernews").
    /// </summary>
    public string Source { get; set; } = string.Empty;
}
