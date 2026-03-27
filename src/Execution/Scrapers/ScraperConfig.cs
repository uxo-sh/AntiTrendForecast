namespace AntiTrendForecast.Execution.Scrapers;

/// <summary>
/// Configuration for web scraper scripts.
/// </summary>
public class ScraperConfig
{
    /// <summary>
    /// Path to the Python executable.
    /// </summary>
    public string PythonPath { get; set; } = "python";

    /// <summary>
    /// Maximum number of retries when rate-limited.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Delay between retries in seconds.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Timeout for a single scrape operation in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Target platforms to scrape.
    /// </summary>
    public List<string> TargetPlatforms { get; set; } = new()
    {
        "reddit",
        "hackernews",
        "twitter"
    };
}
