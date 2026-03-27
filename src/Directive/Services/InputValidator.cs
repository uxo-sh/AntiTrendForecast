namespace AntiTrendForecast.Directive.Services;

/// <summary>
/// Result of input validation.
/// </summary>
public record ValidationResult(bool IsValid, string? ErrorMessage = null)
{
    public static ValidationResult Success() => new(true);
    public static ValidationResult Failure(string error) => new(false, error);
}

/// <summary>
/// Validates user input keywords and timeframe parameters.
/// </summary>
public class InputValidator
{
    private const int MinKeywordLength = 2;
    private const int MaxKeywordLength = 100;

    /// <summary>
    /// Validates a search keyword.
    /// </summary>
    public ValidationResult ValidateKeyword(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return ValidationResult.Failure("Keyword cannot be empty.");

        if (keyword.Length < MinKeywordLength)
            return ValidationResult.Failure($"Keyword must be at least {MinKeywordLength} characters.");

        if (keyword.Length > MaxKeywordLength)
            return ValidationResult.Failure($"Keyword must be at most {MaxKeywordLength} characters.");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a timeframe string (e.g., "7d", "30d", "6m", "1y").
    /// </summary>
    public ValidationResult ValidateTimeframe(string timeframe)
    {
        if (string.IsNullOrWhiteSpace(timeframe))
            return ValidationResult.Failure("Timeframe cannot be empty.");

        var validSuffixes = new[] { "d", "w", "m", "y" };
        var suffix = timeframe[^1..].ToLowerInvariant();

        if (!validSuffixes.Contains(suffix))
            return ValidationResult.Failure("Timeframe must end with d (day), w (week), m (month), or y (year).");

        if (!int.TryParse(timeframe[..^1], out var value) || value <= 0)
            return ValidationResult.Failure("Timeframe must start with a positive number.");

        return ValidationResult.Success();
    }
}
