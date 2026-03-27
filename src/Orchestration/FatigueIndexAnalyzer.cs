using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Orchestration;

/// <summary>
/// Analyzes trend data to calculate a fatigue index measuring market saturation.
/// </summary>
public class FatigueIndexAnalyzer
{
    /// <summary>
    /// Calculates the fatigue index for a set of trend data.
    /// Considers mention volume trends and sentiment decay.
    /// </summary>
    public double Analyze(IEnumerable<TrendData> dataPoints)
    {
        var points = dataPoints.OrderBy(d => d.ScrapedAt).ToList();
        if (points.Count == 0) return 0;

        // High mention volume + declining sentiment = fatigue
        var avgVolume = points.Average(d => d.MentionVolume);
        var avgSentiment = points.Average(d => d.SentimentScore);

        // Volume saturation (high volume = more fatigued)
        var volumeFatigue = avgVolume * 60; // Scale to 0–60

        // Sentiment fatigue (negative sentiment = more fatigued)
        var sentimentFatigue = (1 - ((avgSentiment + 1) / 2)) * 40; // Scale to 0–40

        return Math.Clamp(Math.Round(volumeFatigue + sentimentFatigue, 2), 0, 100);
    }
}
