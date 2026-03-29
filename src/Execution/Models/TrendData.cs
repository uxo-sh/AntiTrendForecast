using System;

namespace AntiTrendForecast.Execution.Models;

/// <summary>
/// Domain model for a trend analysis record.
/// </summary>
public class TrendData
{
    public int Id { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public double FatigueIndex { get; set; }
    public double SentimentScore { get; set; }
    public double GrowthRate { get; set; }
    public string Source { get; set; } = "HackerNews";
    public string RawJson { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; }
}
