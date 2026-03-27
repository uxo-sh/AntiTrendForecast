using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Orchestration;

/// <summary>
/// Orchestrates the full analysis pipeline: Scrape → Analyze Sentiment → Calculate Fatigue → Pivot Score.
/// This is the "Brain" of the application.
/// </summary>
public class PipelineOrchestrator
{
    private readonly FatigueIndexAnalyzer _fatigueAnalyzer;
    private readonly PivotScoreCalculator _pivotCalculator;

    public PipelineOrchestrator(
        FatigueIndexAnalyzer fatigueAnalyzer,
        PivotScoreCalculator pivotCalculator)
    {
        _fatigueAnalyzer = fatigueAnalyzer;
        _pivotCalculator = pivotCalculator;
    }

    /// <summary>
    /// Runs the full analysis pipeline for a given keyword.
    /// </summary>
    public async Task<PivotScoreResult> RunAsync(string keyword, CancellationToken cancellationToken = default)
    {
        // Step 1: Scrape data (via Execution layer)
        // TODO: Call IScraperService to fetch social data
        var scrapedData = new List<TrendData>();

        // Step 2: Analyze sentiment (via Execution layer)
        // TODO: Call PythonRunner for ML inference on scraped data

        // Step 3: Calculate fatigue index
        foreach (var data in scrapedData)
        {
            data.FatigueIndex = _fatigueAnalyzer.Analyze(new[] { data });
        }

        // Step 4: Calculate pivot score
        var pivotScore = _pivotCalculator.Calculate(keyword, scrapedData);

        // Step 5: Store results (via Execution layer)
        // TODO: Call ITrendRepository to persist results

        return pivotScore;
    }
}
