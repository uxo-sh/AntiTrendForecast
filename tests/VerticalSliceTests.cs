using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AntiTrendForecast.Directive.Handlers;
using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Python;
using AntiTrendForecast.Execution.Persistence;
using AntiTrendForecast.Execution.Models;

namespace AntiTrendForecast.Tests;

public class VerticalSliceTests
{
    [Fact]
    public async Task VerticalSlice_Coordination_Works()
    {
        // 1. Arrange
        var mockLoggerHandler = new Mock<ILogger<TrendInputHandler>>();
        var mockLoggerAnalyzer = new Mock<ILogger<FatigueAnalyzer>>();
        var mockLoggerRunner = new Mock<ILogger<PythonRunnerService>>();
        var mockLoggerPivot = new Mock<ILogger<PivotScoreCalculator>>();

        var mockRepo = new Mock<ITrendRepository>();
        mockRepo.Setup(r => r.GetHistoryAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(Enumerable.Empty<TrendData>());

        var pivotCalculator = new PivotScoreCalculator(mockLoggerPivot.Object);
        var analyzer = new FatigueAnalyzer(mockLoggerAnalyzer.Object, pivotCalculator);
        var mockRunner = new Mock<IPythonRunnerService>();

        string mockJson = "{\"keyword\": \"AI Agents\", \"mentions\": 450, \"sentiment_score\": -0.5, \"growth_rate\": -0.2}";
        mockRunner.Setup(r => r.ExecuteScriptAsync(It.IsAny<string>(), It.IsAny<string>()))
                  .ReturnsAsync(mockJson);

        var handler = new TrendInputHandler(mockLoggerHandler.Object, analyzer, mockRunner.Object, mockRepo.Object);

        // 2. Act
        var result = await handler.ProcessTrendAsync("AI Agents");

        // 3. Assert
        Assert.NotNull(result);
        Assert.Equal("AI Agents", result.Keyword);
        Assert.Contains("Saturation", result.SaturationLevel); 
        Assert.True(result.FatigueScore > 75);
        
        // Ensure it saved the result
        mockRepo.Verify(r => r.SaveTrendAsync(It.IsAny<TrendData>()), Times.Once);
    }

    [Fact]
    public void PivotScoreCalculator_DetectsDrift()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PivotScoreCalculator>>();
        var calculator = new PivotScoreCalculator(mockLogger.Object);

        var current = new FatigueResult("Test", 90, "Critical", "{}");
        var history = new List<TrendData>
        {
            new TrendData { Keyword = "Test", FatigueIndex = 50 },
            new TrendData { Keyword = "Test", FatigueIndex = 55 }
        };

        // Act
        var result = calculator.CalculatePivot(current, history);

        // Assert
        // Avg = 52.5. Current = 90. Drift = 37.5
        Assert.True(result.PivotScore > 30);
        Assert.Equal("Rapid Saturation - Market Fatigue Accelerating", result.TrendSummary);
    }
}
