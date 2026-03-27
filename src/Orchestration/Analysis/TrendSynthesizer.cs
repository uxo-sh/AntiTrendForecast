using AntiTrendForecast.Execution.Models;
using Microsoft.Extensions.Logging;

namespace AntiTrendForecast.Orchestration.Analysis;

/// <summary>
/// Interface for synthesizing historical trend data into a confidence score.
/// </summary>
public interface ITrendSynthesizer
{
    /// <summary>
    /// Calculates the confidence score based on the volume and stability of history.
    /// </summary>
    double CalculateConfidence(IEnumerable<TrendData> history);
}

/// <summary>
/// Implementation of TrendSynthesizer.
/// High confidence = Many data points with low variance.
/// </summary>
public class TrendSynthesizer : ITrendSynthesizer
{
    private readonly ILogger<TrendSynthesizer> _logger;

    public TrendSynthesizer(ILogger<TrendSynthesizer> logger)
    {
        _logger = logger;
    }

    public double CalculateConfidence(IEnumerable<TrendData> history)
    {
        var list = history.ToList();
        if (!list.Any()) return 0;

        // Base confidence: 15% per data point, max 45% (Requires more points for 'High' confidence)
        double volumeScore = Math.Min(list.Count * 15.0, 45.0);

        // Stability score: Lower variance in fatigue results = higher confidence
        double stabilityScore = 0;
        if (list.Count > 1)
        {
            var values = list.Select(h => h.FatigueIndex).ToList();
            double avg = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            double stdDev = Math.Sqrt(sumOfSquares / values.Count);

            // Stability is max 35% if stdDev is 0. 
            // This caps max confidence at 80% for small identical datasets (like instant re-analysis)
            stabilityScore = Math.Max(0, 35.0 - (stdDev * 3.0));
        }

        double finalScore = Math.Round(volumeScore + stabilityScore, 1);
        _logger.LogDebug("Calculated Trend Confidence: {Score}%", finalScore);
        
        return finalScore;
    }
}
