using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AntiTrendForecast.Directive.Handlers;
using AntiTrendForecast.Orchestration.Analysis;
using AntiTrendForecast.Execution.Python;
using AntiTrendForecast.Execution.Persistence;
using AntiTrendForecast.Execution.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntiTrendForecast.Tests;

public class VerticalSliceTests
{
    [Fact]
    public async Task VerticalSlice_Coordination_Works()
    {
        // 1. Arrange
        var mockLoggerHandler = new Mock<ILogger<TrendInputHandler>>();
        var mockLoggerAnalyzer = new Mock<ILogger<FatigueAnalyzer>>();
        var mockLoggerPivot = new Mock<ILogger<PivotScoreCalculator>>();
        var mockLoggerSynth = new Mock<ILogger<TrendSynthesizer>>();

        var mockRepo = new Mock<ITrendRepository>();
        mockRepo.Setup(r => r.GetHistoryAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<TrendData> { new TrendData { FatigueIndex = 50 } });

        var pivotCalculator = new PivotScoreCalculator(mockLoggerPivot.Object);
        var synthesizer = new TrendSynthesizer(mockLoggerSynth.Object);
        var analyzer = new FatigueAnalyzer(mockLoggerAnalyzer.Object, pivotCalculator, synthesizer);
        var mockRunner = new Mock<IPythonRunnerService>();

        // Multi-source JSON (Phase 4)
        string mockJson = "{\"keyword\": \"AI\", \"mentions\": 450, \"sentiment_score\": 0.5, \"growth_rate\": 0.1, \"secondary_mentions\": 400, \"secondary_sentiment\": 0.4, \"source\": \"Test\"}";
        mockRunner.Setup(r => r.ExecuteScriptAsync(It.IsAny<string>(), It.IsAny<string>()))
                  .ReturnsAsync(mockJson);

        var handler = new TrendInputHandler(mockLoggerHandler.Object, analyzer, mockRunner.Object, mockRepo.Object);

        // 2. Act
        var result = await handler.ProcessTrendAsync("AI");

        // 3. Assert
        Assert.NotNull(result);
        Assert.Equal("AI", result.Keyword);
        Assert.True(result.ConfidenceScore > 0);
        mockRepo.Verify(r => r.SaveTrendAsync(It.IsAny<TrendData>()), Times.Once);
    }

    [Fact]
    public void TrendSynthesizer_CalculatesConfidence()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<TrendSynthesizer>>();
        var synthesizer = new TrendSynthesizer(mockLogger.Object);

        var history = new List<TrendData>
        {
            new TrendData { FatigueIndex = 50 },
            new TrendData { FatigueIndex = 52 },
            new TrendData { FatigueIndex = 49 }
        };

        // Act
        var score = synthesizer.CalculateConfidence(history);

        // Assert
        // In Phase 4:
        // 3 points = 30% volume. 
        // Stability Score ~ 40%.
        // Total should be around 70%.
        Assert.True(score >= 60); 
    }
}
