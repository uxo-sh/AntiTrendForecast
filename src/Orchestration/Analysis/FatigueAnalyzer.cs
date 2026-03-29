using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using AntiTrendForecast.Execution.Models;
using Microsoft.Extensions.Logging;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Service responsible for parsing raw scraper output and applying saturation algorithms.
/// </summary>
public class FatigueAnalyzer : IFatigueAnalyzer
{
    private readonly ILogger<FatigueAnalyzer> _logger;
    private readonly IPivotScoreCalculator _pivotCalculator;
    private readonly ITrendSynthesizer _synthesizer;

    public FatigueAnalyzer(
        ILogger<FatigueAnalyzer> logger, 
        IPivotScoreCalculator pivotCalculator,
        ITrendSynthesizer synthesizer)
    {
        _logger = logger;
        _pivotCalculator = pivotCalculator;
        _synthesizer = synthesizer;
    }

    public FatigueResult Analyze(string rawJson) => Analyze(rawJson, Enumerable.Empty<TrendData>());

    public FatigueResult Analyze(string rawJson, IEnumerable<TrendData> history)
    {
        var historyList = history.ToList();
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
            
            double score = CalculateScore(data);
            
            // Phase 3: Trend Synthesis (Confidence)
            var confidence = _synthesizer.CalculateConfidence(historyList);
            
            // Refined Level labeling: Account for confidence
            string level = DetermineLevel(score, confidence);

            _logger.LogInformation("Calculated composite fatigue score: {Score} for keyword: {Keyword}", score, data.Keyword);

            var initialResult = new FatigueResult(data.Keyword, score, level, data.Source);
            
            // Phase 2: Pivot Analysis
            var pivot = _pivotCalculator.CalculatePivot(initialResult, historyList);

            return initialResult with 
            { 
                PivotScore = pivot.PivotScore, 
                HistoricalTrend = pivot.TrendSummary,
                ConfidenceScore = confidence
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error during analysis.");
            throw new InvalidOperationException("Failed to parse scraper output.", ex);
        }
    }

    private double CalculateScore(ScraperData data)
    {
        // Cross-Source Synthesis:
        // 1. Primary Volume (HN)
        double primaryVolume = Math.Min(data.Mentions / 100000.0, 1.0);
        
        // 2. Secondary Volume (Simulated Reddit/GitHub Trends)
        double secondaryVolume = Math.Min(data.SecondaryMentions / 100000.0, 1.0);
        
        // 3. Hype Intensity (Normalized point density)
        double hypeFactor = data.SentimentScore * 0.5; 

        // 4. Momentum (Growth scale)
        double momentumFactor = (data.GrowthRate + 0.5); 

        // Weighted Score: 40% Primary, 20% Secondary, 20% Hype, 20% Momentum
        double compositeScore = (primaryVolume * 0.4 + secondaryVolume * 0.2 + hypeFactor * 0.2 + momentumFactor * 0.2) * 100.0;
        
        return Math.Clamp(Math.Round(compositeScore, 1), 0, 100);
    }

    private string DetermineLevel(double score, double confidence)
    {
        string baseStatus = score switch
        {
            >= 80 => "Critical Saturation",
            >= 60 => "High Saturation",
            >= 40 => "Moderate Saturation",
            >= 20 => "Low Saturation",
            _ => "Emerging Niche"
        };

        if (confidence >= 85) return $"Verified {baseStatus}";
        if (confidence >= 50) return $"Developing {baseStatus} Verdict";
        if (confidence >= 20) return $"Emerging {baseStatus} Pattern";
        return $"Initial Scan - {baseStatus} (Low Confidence)";
    }

    // Internal model for parsing
    private record ScraperData(
        [property: JsonPropertyName("keyword")] string Keyword, 
        [property: JsonPropertyName("mentions")] int Mentions, 
        [property: JsonPropertyName("sentiment_score")] double SentimentScore, 
        [property: JsonPropertyName("growth_rate")] double GrowthRate,
        [property: JsonPropertyName("secondary_mentions")] int SecondaryMentions,
        [property: JsonPropertyName("secondary_sentiment")] double SecondarySentiment,
        [property: JsonPropertyName("source")] string Source);
}
