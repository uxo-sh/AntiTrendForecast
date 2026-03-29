using AntiTrendForecast.Execution.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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
        if (!list.Any()) return 5.0; // Initial seed confidence

        // Base confidence: 10% per data point, max 40% (Requires 4 points for base maturity)
        double volumeScore = Math.Min(list.Count * 10.0, 40.0);

        // Stability score: Lower variance in fatigue results = higher confidence
        double stabilityScore = 0;
        if (list.Count > 2) // Require at least 3 points for stability bonus
        {
            var values = list.Select(h => h.FatigueIndex).ToList();
            double avg = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
            double stdDev = Math.Sqrt(sumOfSquares / values.Count);

            // Stability is max 45% if stdDev is 0. 
            // Total max = 40 + 45 = 85% (Verified)
            stabilityScore = Math.Max(0, 45.0 - (stdDev * 4.0));
        }

        double finalScore = Math.Round(volumeScore + stabilityScore, 1);
        _logger.LogDebug("Calculated Trend Confidence: {Score}%", finalScore);
        
        return Math.Clamp(finalScore, 5, 100);
    }
}
