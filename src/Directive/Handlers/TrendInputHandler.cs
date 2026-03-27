using Microsoft.Extensions.Logging;
using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Python;
using AntiTrendForecast.Execution.Persistence;
using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Directive.Handlers;

/// <summary>
/// Orchestrates the end-to-end vertical slice from keyword input to analysis results.
/// </summary>
public class TrendInputHandler : ITrendInputHandler
{
    private readonly ILogger<TrendInputHandler> _logger;
    private readonly IFatigueAnalyzer _fatigueAnalyzer;
    private readonly IPythonRunnerService _pythonRunner;
    private readonly ITrendRepository _trendRepository;

    public TrendInputHandler(
        ILogger<TrendInputHandler> logger,
        IFatigueAnalyzer fatigueAnalyzer,
        IPythonRunnerService pythonRunner,
        ITrendRepository trendRepository)
    {
        _logger = logger;
        _fatigueAnalyzer = fatigueAnalyzer;
        _pythonRunner = pythonRunner;
        _trendRepository = trendRepository;
    }

    public async Task<FatigueResult> ProcessTrendAsync(string keyword)
    {
        _logger.LogInformation("Processing trend analysis for keyword: {Keyword}", keyword);

        try
        {
            // Layer 3: Execution (Get History)
            var history = await _trendRepository.GetHistoryAsync(keyword);

            // Layer 3: Execution (Get Current Live Data from Python)
            string rawData = await _pythonRunner.ExecuteScriptAsync("scraper.py", keyword);

            // Layer 2: Orchestration (Brain Analysis with History Context)
            var result = _fatigueAnalyzer.Analyze(rawData, history);

            // Layer 3: Execution (Persist fresh results)
            await _trendRepository.SaveTrendAsync(new TrendData
            {
                Keyword = result.Keyword,
                FatigueIndex = result.FatigueScore,
                MentionVolume = 0.5, // Extraction from rawData could be more precise
                SentimentScore = 0.0,
                Source = "Integrated Pipeline",
                ScrapedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Trend analysis & persistence completed for {Keyword}", keyword);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trend analysis for {Keyword}", keyword);
            throw; 
        }
    }
}
