namespace AntiTrendForecast.Execution.Python;

/// <summary>
/// Interface for executing Python scripts in the Execution layer.
/// </summary>
public interface IPythonRunnerService
{
    /// <summary>
    /// Executes a python script by name from the predefined scripts directory.
    /// </summary>
    /// <param name="scriptName">The filename of the script (e.g. "scraper.py")</param>
    /// <param name="arguments">Arguments to pass to the script.</param>
    /// <returns>The standard output of the script as a string.</returns>
    Task<string> ExecuteScriptAsync(string scriptName, string arguments = "");
}
