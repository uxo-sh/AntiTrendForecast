using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Service responsible for parsing raw scraper output and applying saturation algorithms.
/// </summary>
public class FatigueAnalyzer : IFatigueAnalyzer
{
    private readonly ILogger<FatigueAnalyzer> _logger;

    public FatigueAnalyzer(ILogger<FatigueAnalyzer> logger)
    {
        _logger = logger;
    }

    public FatigueResult Analyze(string rawJson)
    {
        _logger.LogInformation("Analyzing raw scraper data: {Length} characters", rawJson.Length);

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<ScraperData>(rawJson, options);

            if (data == null)
            {
                _logger.LogError("Failed to deserialize scraper data.");
                throw new InvalidOperationException("Invalid scraper data format.");
            }

            // Simple saturation algorithm:
            // High mentions (> 300) = High Saturation
            // Low sentiment (< -0.2) + High mentions = Critical Saturation
            // High mentions + Low growth (< -0.1) = Oversalurated
            
            double score = CalculateScore(data);
            string level = DetermineLevel(score);

            _logger.LogInformation("Calculated fatigue score: {Score} for keyword: {Keyword}", score, data.Keyword);

            return new FatigueResult(data.Keyword, score, level, rawJson);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error during analysis.");
            throw new InvalidOperationException("Failed to parse scraper output.", ex);
        }
    }

    private double CalculateScore(ScraperData data)
    {
        // Example logic:
        // Mentions normalized (0-500 -> 0-1)
        double mentionNormalized = Math.Min(data.Mentions / 500.0, 1.0);
        
        // Sentiment (low sentiment increases fatigue)
        double sentimentFactor = (1.0 - data.SentimentScore) / 2.0; 

        // Growth (low growth increases fatigue)
        double growthFactor = Math.Max(0, 1.0 - (data.GrowthRate + 0.2) / 0.7);

        return (mentionNormalized * 0.4 + sentimentFactor * 0.3 + growthFactor * 0.3) * 100.0;
    }

    private string DetermineLevel(double score)
    {
        return score switch
        {
            >= 80 => "Critical Saturation - Pivot Immediately",
            >= 60 => "High Saturation - Market Fatigue Detected",
            >= 40 => "Moderate Saturation - Monitoring Recommended",
            >= 20 => "Low Saturation - Growing Trend",
            _ => "Emerging Niche - High Potential"
        };
    }

    // Internal model for parsing
    private record ScraperData(
        [property: JsonPropertyName("keyword")] string Keyword, 
        [property: JsonPropertyName("mentions")] int Mentions, 
        [property: JsonPropertyName("sentiment_score")] double SentimentScore, 
        [property: JsonPropertyName("growth_rate")] double GrowthRate);
}
