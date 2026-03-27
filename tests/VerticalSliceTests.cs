using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AntiTrendForecast.Directive.Handlers;
using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Python;

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

        // We use the real analyzer and mock the Python runner for this unit test
        // because we want to verify the logic flow and the analyzer's output.
        var analyzer = new FatigueAnalyzer(mockLoggerAnalyzer.Object);
        var mockRunner = new Mock<IPythonRunnerService>();

        string mockJson = "{\"keyword\": \"AI Agents\", \"mentions\": 450, \"sentiment_score\": -0.5, \"growth_rate\": -0.2}";
        mockRunner.Setup(r => r.ExecuteScriptAsync(It.IsAny<string>(), It.IsAny<string>()))
                  .ReturnsAsync(mockJson);

        var handler = new TrendInputHandler(mockLoggerHandler.Object, analyzer, mockRunner.Object);

        // 2. Act
        var result = await handler.ProcessTrendAsync("AI Agents");

        // 3. Assert
        Assert.NotNull(result);
        Assert.Equal("AI Agents", result.Keyword);
        Assert.Contains("Critical", result.SaturationLevel); // 450 mentions + -0.5 sentiment should be high saturation
        Assert.True(result.FatigueScore > 75);
    }

    [Fact]
    public void FatigueAnalyzer_CalculatesCorrectLevels()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<FatigueAnalyzer>>();
        var analyzer = new FatigueAnalyzer(mockLogger.Object);

        // High Mentions, Negative Sentiment, Negative Growth -> Critical
        string rawJson = "{\"keyword\": \"Test\", \"mentions\": 480, \"sentiment_score\": -0.8, \"growth_rate\": -0.1}";

        // Act
        var result = analyzer.Analyze(rawJson);

        // Assert
        Assert.Equal("Critical Saturation - Pivot Immediately", result.SaturationLevel);
    }
}
