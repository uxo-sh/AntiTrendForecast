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

            _logger.LogInformation("Calculated base fatigue score: {Score} for keyword: {Keyword}", score, data.Keyword);

            var initialResult = new FatigueResult(data.Keyword, score, level, rawJson);
            
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
        // 1. Volume Influence (0-100k normalized)
        double volumeFactor = Math.Min(data.Mentions / 100000.0, 1.0);
        
        // 2. Hype Intensity (Average points per hit - 0-600 normalized)
        double hypeFactor = data.SentimentScore * 0.5; 

        // 3. Momentum (Growth rate from scraper)
        double momentumFactor = (data.GrowthRate + 0.5); 

        // Weighted Score: Volume is anchor (50%), Hype (30%), Momentum (20%)
        return Math.Round((volumeFactor * 0.5 + hypeFactor * 0.3 + momentumFactor * 0.2) * 100.0, 1);
    }

    private string DetermineLevel(double score, double confidence)
    {
        if (confidence < 15) return "Initial Scan - Data Breadth Required";

        string baseStatus = score switch
        {
            >= 80 => "Critical Saturation",
            >= 60 => "High Saturation",
            >= 40 => "Moderate Saturation",
            >= 20 => "Low Saturation",
            _ => "Emerging Niche"
        };

        return confidence >= 60 ? $"Verified {baseStatus}" : $"Estimated {baseStatus}";
    }

    // Internal model for parsing
    private record ScraperData(
        [property: JsonPropertyName("keyword")] string Keyword, 
        [property: JsonPropertyName("mentions")] int Mentions, 
        [property: JsonPropertyName("sentiment_score")] double SentimentScore, 
        [property: JsonPropertyName("growth_rate")] double GrowthRate);
}
