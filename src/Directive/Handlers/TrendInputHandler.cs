using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Python;
using AntiTrendForecast.Execution.Persistence;
using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Directive.Handlers;

/// <summary>
/// Handles the coordination between high-level user input and low-level trend processing.
/// </summary>
public class TrendInputHandler : ITrendInputHandler
{
    private readonly ILogger<TrendInputHandler> _logger;
    private readonly IFatigueAnalyzer _analyzer;
    private readonly IPythonRunnerService _pythonRunner;
    private readonly ITrendRepository _repository;

    public TrendInputHandler(
        ILogger<TrendInputHandler> logger, 
        IFatigueAnalyzer analyzer, 
        IPythonRunnerService pythonRunner,
        ITrendRepository repository)
    {
        _logger = logger;
        _analyzer = analyzer;
        _pythonRunner = pythonRunner;
        _repository = repository;
    }

    public async Task<FatigueResult> ProcessTrendAsync(string keyword)
    {
        _logger.LogInformation("Processing trend analysis for keyword: {Keyword}", keyword);

        // 1. Fetch History from SQLite
        var history = await _repository.GetHistoryAsync(keyword);

        // 2. Fetch Fresh Data from Python Scraper
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        // Search up for the script if we are in bin/Debug... Or expect a standard layout.
        string scriptPath = Path.Combine(baseDir, "Execution/Scripts/scraper.py");
        if (!File.Exists(scriptPath)) 
        {
            // Fallback for dev environment where EXE is in bin/Debug/net10.0-windows
            scriptPath = Path.GetFullPath(Path.Combine(baseDir, "../../../../src/Execution/Scripts/scraper.py"));
        }
        
        string rawJson = await _pythonRunner.ExecuteScriptAsync(scriptPath, keyword);

        // 3. Analyze with Context
        var result = _analyzer.Analyze(rawJson, history);

        // 4. Persist to SQLite
        var trendData = new TrendData
        {
            Keyword = keyword,
            FatigueIndex = result.FatigueScore,
            SentimentScore = 0.5, // Simplified mapping back
            GrowthRate = 0,       // Simplified mapping back
            Source = result.Source,
            RawJson = rawJson
        };
        await _repository.SaveTrendAsync(trendData);

        return result;
    }

    public async Task<IEnumerable<string>> GetRecentKeywordsAsync() => await _repository.GetRecentKeywordsAsync();
    public async Task<IEnumerable<TrendData>> GetHistoryAsync(string keyword) => await _repository.GetHistoryAsync(keyword);
}
