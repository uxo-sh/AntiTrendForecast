namespace AntiTrendForecast.Execution.Models;

/// <summary>
/// Represents the calculated Pivot Score for a keyword, indicating whether
/// the market is ready for a strategic shift.
/// </summary>
public class PivotScoreResult
{
    public string Keyword { get; set; } = string.Empty;

    /// <summary>
    /// The pivot score (0.0 – 100.0). Higher = stronger signal to pivot.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Human-readable interpretation of the score.
    /// </summary>
    public string Interpretation =>
        Score switch
        {
            >= 80 => "🔴 Critical saturation — strong pivot signal",
            >= 60 => "🟠 High fatigue — consider alternatives",
            >= 40 => "🟡 Moderate fatigue — monitor closely",
            >= 20 => "🟢 Low fatigue — trend is still viable",
            _     => "🔵 Fresh trend — early adopter opportunity"
        };

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
