using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace AntiTrendForecast.Execution.Python;

/// <summary>
/// Service responsible for executing Python scripts using ProcessStartInfo.
/// Handles environment-specific issues like missing python or timeouts.
/// </summary>
public class PythonRunnerService : IPythonRunnerService
{
    private readonly ILogger<PythonRunnerService> _logger;
    
    // We try 'python' first, then 'py' as a fallback on Windows
    private string _currentExecutable = "python"; 

    public PythonRunnerService(ILogger<PythonRunnerService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExecuteScriptAsync(string scriptPath, string arguments = "")
    {
        try 
        {
            return await ExecuteInternalAsync(_currentExecutable, scriptPath, arguments);
        }
        catch (TimeoutException) when (_currentExecutable == "python")
        {
            _logger.LogWarning("Python timed out. Switching to 'py' launcher fallback.");
            _currentExecutable = "py";
            return await ExecuteInternalAsync(_currentExecutable, scriptPath, arguments);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Python execution failed. Falling back to internal simulation for prototype.");
            // Return a safe mock JSON so the app doesn't crash during the prototype phase
            return CreateMockJson(arguments, $"Execution Error: {ex.Message}");
        }
    }

    private async Task<string> ExecuteInternalAsync(string executable, string scriptPath, string arguments)
    {
        // Resolve absolute path
        string fullScriptPath = Path.IsPathRooted(scriptPath) ? scriptPath : Path.GetFullPath(scriptPath);
        
        if (!File.Exists(fullScriptPath))
        {
            throw new FileNotFoundException($"Script not found: {scriptPath}", fullScriptPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = $"\"{fullScriptPath}\" \"{arguments}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        
        try 
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new InvalidOperationException($"Executable '{executable}' not found.");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        // 15 seconds is more than enough for a single Algolia request
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(15));
        var finishedTask = await Task.WhenAny(process.WaitForExitAsync(), timeoutTask);

        if (finishedTask == timeoutTask)
        {
            process.Kill();
            throw new TimeoutException("Python process hung or took too long.");
        }

        var output = await outputTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Python failed (Code {process.ExitCode}): {error}");
        }

        return output;
    }

    private string CreateMockJson(string keyword, string reason)
    {
        // Safe fallback for prototype - Shared Phase 4 Schema
        var random = new Random();
        var data = new 
        {
            keyword = keyword,
            mentions = random.Next(200, 800),
            sentiment_score = random.NextDouble() * 0.5,
            growth_rate = random.NextDouble() * 0.4 - 0.2,
            secondary_mentions = random.Next(200, 800),
            secondary_sentiment = random.NextDouble() * 0.5,
            source = $"Simulation Fallback ({reason})"
        };
        return System.Text.Json.JsonSerializer.Serialize(data);
    }
}
