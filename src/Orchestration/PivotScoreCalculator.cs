using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Orchestration;

/// <summary>
/// Calculates the Pivot Score based on fatigue index and sentiment data.
/// A higher score indicates stronger signal that the market is oversaturated.
/// </summary>
public class PivotScoreCalculator
{
    private const double FatigueWeight = 0.6;
    private const double SentimentDecayWeight = 0.4;

    /// <summary>
    /// Calculates the pivot score from a collection of trend data points.
    /// </summary>
    public PivotScoreResult Calculate(string keyword, IEnumerable<TrendData> trendDataPoints)
    {
        var dataPoints = trendDataPoints.ToList();
        if (dataPoints.Count == 0)
        {
            return new PivotScoreResult { Keyword = keyword, Score = 0 };
        }

        // Average fatigue index
        var avgFatigue = dataPoints.Average(d => d.FatigueIndex);

        // Sentiment decay: negative trend in sentiment over time
        var sentimentDecay = CalculateSentimentDecay(dataPoints);

        // Weighted combination
        var score = Math.Clamp(
            (avgFatigue * FatigueWeight) + (sentimentDecay * SentimentDecayWeight),
            0, 100);

        return new PivotScoreResult
        {
            Keyword = keyword,
            Score = Math.Round(score, 2)
        };
    }

    private static double CalculateSentimentDecay(List<TrendData> dataPoints)
    {
        if (dataPoints.Count < 2) return 0;

        var ordered = dataPoints.OrderBy(d => d.ScrapedAt).ToList();
        var firstHalf = ordered.Take(ordered.Count / 2).Average(d => d.SentimentScore);
        var secondHalf = ordered.Skip(ordered.Count / 2).Average(d => d.SentimentScore);

        // If sentiment is declining, that contributes to the pivot score
        var decay = firstHalf - secondHalf;
        return Math.Clamp(decay * 50 + 50, 0, 100); // Normalize to 0–100 range
    }
}
