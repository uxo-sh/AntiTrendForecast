using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AntiTrendForecast.Execution.Python;

/// <summary>
/// Service responsible for executing Python scripts using ProcessStartInfo.
/// </summary>
public class PythonRunnerService : IPythonRunnerService
{
    private readonly ILogger<PythonRunnerService> _logger;
    private readonly string _scriptsPath;
    private const string PythonExecutable = "python"; 

    public PythonRunnerService(ILogger<PythonRunnerService> logger)
    {
        _logger = logger;
        
        // Robust and simple: Scripts are copied to the output directory by the Execution project.
        _scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");
        
        // Ensure directory existence
        if (!Directory.Exists(_scriptsPath))
        {
            Directory.CreateDirectory(_scriptsPath);
            _logger.LogWarning("Scripts directory was missing in output, created empty: {Path}", _scriptsPath);
        }
        else
        {
            _logger.LogInformation("Python script path confirmed at: {Path}", _scriptsPath);
        }
    }

    public async Task<string> ExecuteScriptAsync(string scriptName, string arguments = "")
    {
        var fullScriptPath = Path.Combine(_scriptsPath, scriptName);
        
        _logger.LogDebug("Looking for script at: {Path}", fullScriptPath);

        if (!File.Exists(fullScriptPath))
        {
            _logger.LogError("Python script not found at {Path}", fullScriptPath);
            throw new FileNotFoundException($"Python script not found: {scriptName}. Rebuild the solution to copy scripts to the output directory.", fullScriptPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = PythonExecutable,
            Arguments = $"\"{fullScriptPath}\" \"{arguments}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            var finished = await Task.WhenAny(process.WaitForExitAsync(), Task.Delay(15000));
            if (finished != process.WaitForExitAsync())
            {
                process.Kill();
                throw new TimeoutException("The scraper took too long (15s timeout).");
            }

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("Python script failed (Code {Code}). Error: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"Python script failed: {error}");
            }

            return output;
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 2)
        {
            _logger.LogError(ex, "Python executable not found in PATH.");
            throw new InvalidOperationException("Python could not be found. Please ensure Python is installed and added to your system's PATH.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing {FileName}", scriptName);
            throw;
        }
    }
}
