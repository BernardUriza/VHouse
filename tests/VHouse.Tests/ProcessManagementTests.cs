using System.Diagnostics;
using Xunit;

namespace VHouse.Tests;

/// <summary>
/// TDD tests for process management
/// These tests ensure we can properly start/stop the application without file locking issues
/// </summary>
public class ProcessManagementTests
{
    [Fact]
    public void Should_Have_ProcessManagement_Script()
    {
        // Arrange
        var projectRoot = GetProjectRoot();
        var scriptPath = Path.Combine(projectRoot, "scripts", "manage-processes.ps1");
        
        // Act & Assert
        Assert.True(File.Exists(scriptPath), $"Process management script should exist at: {scriptPath}");
    }

    [Fact]
    public void Should_Be_Able_To_Check_Process_Status()
    {
        // Arrange
        var scriptPath = GetScriptPath();
        
        // Act
        var result = RunPowerShellScript(scriptPath, "status");
        
        // Assert
        Assert.True(result.ExitCode == 0, $"Status check should succeed. Output: {result.Output}");
    }

    [Fact]
    public void Should_Be_Able_To_Stop_Existing_Processes()
    {
        // Arrange
        var scriptPath = GetScriptPath();
        
        // Act - This should work even if no processes are running
        var result = RunPowerShellScript(scriptPath, "stop");
        
        // Assert
        Assert.True(result.ExitCode == 0, $"Stop command should succeed. Output: {result.Output}");
    }

    [Theory]
    [InlineData("VHouse.Tests.dll")] // This process should not conflict
    [InlineData("dotnet.exe")] // This is a valid executable
    public void Should_Not_Kill_Unrelated_Processes(string processName)
    {
        // Arrange
        var currentProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
        var initialCount = currentProcesses.Length;

        var scriptPath = GetScriptPath();
        
        // Act
        var result = RunPowerShellScript(scriptPath, "stop");
        
        // Assert
        var finalProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
        
        // The script should not kill unrelated processes
        // (We can't be exact because system processes can start/stop, but major changes indicate a problem)
        var countDifference = Math.Abs(finalProcesses.Length - initialCount);
        Assert.True(countDifference <= 1, 
            $"Should not significantly affect unrelated {processName} processes. " +
            $"Initial: {initialCount}, Final: {finalProcesses.Length}");
    }

    [Fact]
    public void Process_Management_Script_Should_Have_Execution_Policy_Friendly_Syntax()
    {
        // Arrange
        var scriptPath = GetScriptPath();
        var scriptContent = File.ReadAllText(scriptPath);
        
        // Act & Assert - Check for PowerShell best practices
        Assert.Contains("param(", scriptContent); // Should use proper parameters
        Assert.Contains("[ValidateSet(", scriptContent); // Should validate inputs
        Assert.Contains("try {", scriptContent); // Should have error handling
        Assert.Contains("catch {", scriptContent); // Should have error handling
    }

    private string GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        // Walk up the directory tree to find the project root (where .sln file is)
        var dir = new DirectoryInfo(currentDir);
        while (dir != null && !dir.GetFiles("*.sln").Any())
        {
            dir = dir.Parent;
        }
        
        if (dir == null)
        {
            throw new InvalidOperationException("Could not find project root directory");
        }
        
        return dir.FullName;
    }

    private string GetScriptPath()
    {
        var projectRoot = GetProjectRoot();
        return Path.Combine(projectRoot, "scripts", "manage-processes.ps1");
    }

    private (int ExitCode, string Output) RunPowerShellScript(string scriptPath, string action)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -Action {action}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            
            process.WaitForExit(10000); // 10 second timeout
            
            var fullOutput = $"STDOUT: {output}\nSTDERR: {error}";
            
            return (process.ExitCode, fullOutput);
        }
        catch (Exception ex)
        {
            return (-1, $"Exception running PowerShell: {ex.Message}");
        }
    }
}