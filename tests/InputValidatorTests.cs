using AntiTrendForecast.Directive.Services;

namespace AntiTrendForecast.Tests;

/// <summary>
/// Tests for the InputValidator service.
/// </summary>
public class InputValidatorTests
{
    private readonly InputValidator _validator = new();

    [Fact]
    public void ValidateKeyword_EmptyString_ReturnsFailure()
    {
        var result = _validator.ValidateKeyword("");
        Assert.False(result.IsValid);
        Assert.Contains("empty", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateKeyword_TooShort_ReturnsFailure()
    {
        var result = _validator.ValidateKeyword("A");
        Assert.False(result.IsValid);
        Assert.Contains("at least", result.ErrorMessage!);
    }

    [Fact]
    public void ValidateKeyword_ValidKeyword_ReturnsSuccess()
    {
        var result = _validator.ValidateKeyword("AI Agents");
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateKeyword_TooLong_ReturnsFailure()
    {
        var longKeyword = new string('x', 101);
        var result = _validator.ValidateKeyword(longKeyword);
        Assert.False(result.IsValid);
        Assert.Contains("at most", result.ErrorMessage!);
    }

    [Theory]
    [InlineData("7d")]
    [InlineData("30d")]
    [InlineData("6m")]
    [InlineData("1y")]
    [InlineData("2w")]
    public void ValidateTimeframe_ValidFormats_ReturnsSuccess(string timeframe)
    {
        var result = _validator.ValidateTimeframe(timeframe);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("0d")]
    [InlineData("-1m")]
    public void ValidateTimeframe_InvalidFormats_ReturnsFailure(string timeframe)
    {
        var result = _validator.ValidateTimeframe(timeframe);
        Assert.False(result.IsValid);
    }
}
