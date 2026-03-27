using Microsoft.Extensions.Logging;
using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Python;

namespace AntiTrendForecast.Directive.Handlers;

/// <summary>
/// Orchestrates the end-to-end vertical slice from keyword input to analysis results.
/// </summary>
public class TrendInputHandler : ITrendInputHandler
{
    private readonly ILogger<TrendInputHandler> _logger;
    private readonly IFatigueAnalyzer _fatigueAnalyzer;
    private readonly IPythonRunnerService _pythonRunner;

    public TrendInputHandler(
        ILogger<TrendInputHandler> logger,
        IFatigueAnalyzer fatigueAnalyzer,
        IPythonRunnerService pythonRunner)
    {
        _logger = logger;
        _fatigueAnalyzer = fatigueAnalyzer;
        _pythonRunner = pythonRunner;
    }

    public async Task<FatigueResult> ProcessTrendAsync(string keyword)
    {
        _logger.LogInformation("Processing trend analysis for keyword: {Keyword}", keyword);

        try
        {
            // Layer 1 calls Layer 2 (well, technically it coordinates across layers, 
            // but we follow downward flow in implementation).
            
            // Layer 3: Execution (Doing the work)
            string rawData = await _pythonRunner.ExecuteScriptAsync("scraper.py", keyword);

            // Layer 2: Orchestration (Brain - Fatigue Analysis)
            var result = _fatigueAnalyzer.Analyze(rawData);

            _logger.LogInformation("Trend analysis completed successfully for {Keyword}", keyword);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing trend analysis for {Keyword}", keyword);
            throw; // Propagate to UI level for display
        }
    }
}
