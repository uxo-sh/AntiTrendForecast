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
    private const string PythonExecutable = "python"; // Assumes python is in PATH

    public PythonRunnerService(ILogger<PythonRunnerService> logger)
    {
        _logger = logger;
        // Scripts are located in the source tree for development
        _scriptsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Execution", "Scripts");
        
        // Ensure directory exists
        if (!Directory.Exists(_scriptsPath))
        {
            Directory.CreateDirectory(_scriptsPath);
        }
    }

    public async Task<string> ExecuteScriptAsync(string scriptName, string arguments = "")
    {
        var fullScriptPath = Path.Combine(_scriptsPath, scriptName);
        
        if (!File.Exists(fullScriptPath))
        {
            _logger.LogError("Python script not found at {Path}", fullScriptPath);
            throw new FileNotFoundException("Python script not found", fullScriptPath);
        }

        _logger.LogInformation("Executing Python script: {FileName} with args: {Args}", scriptName, arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = PythonExecutable,
            Arguments = $"\"{fullScriptPath}\" {arguments}",
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

            await process.WaitForExitAsync();

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("Python script exited with code {Code}. Error: {Error}", process.ExitCode, error);
                throw new InvalidOperationException($"Python script failed: {error}");
            }

            return output;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            _logger.LogCritical(ex, "Python environment not detected or python executable not in PATH.");
            throw new InvalidOperationException("Python environment not detected. Please ensure Python is installed and added to PATH.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing Python script.");
            throw;
        }
    }
}
