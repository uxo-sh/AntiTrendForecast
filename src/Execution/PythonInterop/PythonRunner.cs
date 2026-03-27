using System.Diagnostics;

namespace AntiTrendForecast.Execution.PythonInterop;

/// <summary>
/// Launches Python scripts as child processes and captures their output.
/// Used for ML inference and web scraping via Scrapy/Selenium.
/// </summary>
public class PythonRunner
{
    private readonly string _pythonExecutable;
    private readonly string _scriptsDirectory;

    public PythonRunner(string pythonExecutable = "python", string? scriptsDirectory = null)
    {
        _pythonExecutable = pythonExecutable;
        _scriptsDirectory = scriptsDirectory
            ?? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "scripts");
    }

    /// <summary>
    /// Runs a Python script and returns its stdout output.
    /// </summary>
    /// <param name="scriptName">Name of the script file (e.g., "scraper.py").</param>
    /// <param name="arguments">Arguments to pass to the script.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stdout output of the script.</returns>
    public async Task<string> RunScriptAsync(
        string scriptName,
        string arguments = "",
        CancellationToken cancellationToken = default)
    {
        var scriptPath = Path.Combine(_scriptsDirectory, scriptName);
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Python script not found: {scriptPath}");

        var startInfo = new ProcessStartInfo
        {
            FileName = _pythonExecutable,
            Arguments = $"\"{scriptPath}\" {arguments}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
            throw new InvalidOperationException(
                $"Python script '{scriptName}' exited with code {process.ExitCode}. Error: {stderr}");

        return stdout;
    }
}
