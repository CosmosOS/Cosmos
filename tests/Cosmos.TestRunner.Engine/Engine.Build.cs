using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Cosmos.TestRunner.Engine;

/// <summary>
/// Engine build functionality - compiles test kernels to bootable ISOs
/// </summary>
public partial class Engine
{
    /// <summary>
    /// Build the test kernel using dotnet publish
    /// </summary>
    private async Task<string> BuildKernelAsync()
    {
        if (!Directory.Exists(_config.KernelProjectPath))
        {
            throw new DirectoryNotFoundException($"Kernel project not found: {_config.KernelProjectPath}");
        }

        // Find the .csproj file
        var csprojFiles = Directory.GetFiles(_config.KernelProjectPath, "*.csproj");
        if (csprojFiles.Length == 0)
        {
            throw new FileNotFoundException($"No .csproj file found in {_config.KernelProjectPath}");
        }

        string projectFile = csprojFiles[0];
        string projectName = Path.GetFileNameWithoutExtension(projectFile);

        // Determine output directory
        string outputDir = _config.OutputDirectory;
        if (string.IsNullOrEmpty(outputDir))
        {
            outputDir = Path.Combine(_config.KernelProjectPath, $"output-{_config.Architecture}");
        }

        // The kernel is always published for linux-<arch>, whatever the host, as cosmos build does: a
        // win- or osx- runtime compiles CoreLib's interop for that OS in, and its first P/Invoke recurses
        // through the lazy resolver until the stack overruns the IDT.
        string runtime = _config.Architecture.ToLowerInvariant() switch
        {
            "x64" => "linux-x64",
            "arm64" => "linux-arm64",
            _ => throw new ArgumentException($"Unsupported architecture: {_config.Architecture}")
        };

        string defineConstants = _config.Architecture.ToUpperInvariant() switch
        {
            "X64" => "ARCH_X64",
            "ARM64" => "ARCH_ARM64",
            _ => throw new ArgumentException($"Unsupported architecture: {_config.Architecture}")
        };

        string cosmosArch = _config.Architecture.ToLowerInvariant();

        string coverageArg = _config.CoverageEnabled ? "-p:CosmosCoverage=true " : "";

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"publish " +
                       $"-c {_config.BuildConfiguration} " +
                       $"-r {runtime} " +
                       $"-p:DefineConstants=\"{defineConstants}\" " +
                       $"-p:CosmosArch=\"{cosmosArch}\" " +
                       $"{coverageArg}" +
                       $"\"{projectFile}\" " +
                       $"-o \"{outputDir}\"",
            WorkingDirectory = _config.KernelProjectPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Add dotnet tools to PATH
        var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var homeDir = Environment.GetEnvironmentVariable("HOME") ?? string.Empty;
        var dotnetToolsPath = Path.Combine(homeDir, ".dotnet", "tools");

        if (!pathEnv.Contains(dotnetToolsPath))
        {
            startInfo.EnvironmentVariables["PATH"] = $"{pathEnv}:{dotnetToolsPath}";
        }

        using var process = new Process { StartInfo = startInfo };

        var outputBuilder = new System.Text.StringBuilder();
        var errorBuilder = new System.Text.StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
                Console.WriteLine($"[Build] {e.Data}");
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);
                Console.WriteLine($"[Build Error] {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Build failed with exit code {process.ExitCode}:\n{errorBuilder}");
        }

        // Find the ISO file
        string isoPath = Path.Combine(outputDir, $"{projectName}.iso");
        if (!File.Exists(isoPath))
        {
            throw new FileNotFoundException($"ISO file not found after build: {isoPath}");
        }

        return isoPath;
    }
}
