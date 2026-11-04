using System;
using System.IO;
using System.Threading.Tasks;
using Cosmos.TestRunner.Engine;

namespace Cosmos.TestRunner.Engine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("Cosmos Test Runner Engine");
        Console.WriteLine("=========================\n");

        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Cosmos.TestRunner.Engine <kernel-project-path> [architecture] [timeout] [xml-output] [mode] [--coverage]");
            Console.WriteLine("  kernel-project-path: Path to test kernel project directory");
            Console.WriteLine("  architecture: x64, arm64 or riscv64 (default: x64)");
            Console.WriteLine("  timeout: Timeout in seconds (default: 30)");
            Console.WriteLine("  xml-output: Optional path for JUnit XML output (use '-' to skip)");
            Console.WriteLine("  mode: ci or dev (default: ci)");
            Console.WriteLine("        ci  = headless, automated, fast");
            Console.WriteLine("        dev = visual display, interactive, debugging");
            Console.WriteLine("  --coverage: Enable code coverage instrumentation");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  Cosmos.TestRunner.Engine tests/Kernels/Cosmos.Kernel.Tests.HelloWorld x64 30");
            Console.WriteLine("  Cosmos.TestRunner.Engine tests/Kernels/Cosmos.Kernel.Tests.HelloWorld x64 30 results.xml ci --coverage");
            return 1;
        }

        // Check for --coverage flag anywhere in args
        bool coverageEnabled = args.Any(a => a == "--coverage");
        var positionalArgs = args.Where(a => !a.StartsWith("--")).ToArray();

        string kernelPath = positionalArgs[0];
        string architecture = positionalArgs.Length > 1 ? positionalArgs[1] : "x64";
        int timeout = positionalArgs.Length > 2 ? int.Parse(positionalArgs[2]) : 30;
        string xmlOutput = positionalArgs.Length > 3 && positionalArgs[3] != "-" ? positionalArgs[3] : string.Empty;
        string modeStr = positionalArgs.Length > 4 ? positionalArgs[4] : "ci";

        TestRunnerMode mode = modeStr.ToLowerInvariant() switch
        {
            "dev" => TestRunnerMode.Dev,
            "ci" => TestRunnerMode.CI,
            _ => TestRunnerMode.CI
        };

        // Resolve to absolute path
        if (!Path.IsPathRooted(kernelPath))
        {
            kernelPath = Path.GetFullPath(kernelPath);
        }

        var config = new TestConfiguration
        {
            KernelProjectPath = kernelPath,
            Architecture = architecture,
            TimeoutSeconds = timeout,
            KeepBuildArtifacts = true, // Keep artifacts for debugging
            XmlOutputPath = xmlOutput,
            Mode = mode,
            CoverageEnabled = coverageEnabled
        };

        if (coverageEnabled)
        {
            Console.WriteLine("📊 Code coverage instrumentation enabled");
        }

        Console.WriteLine($"Mode: {mode}");
        if (mode == TestRunnerMode.Dev)
        {
            Console.WriteLine("⚠️  Dev mode: QEMU display window will open");
        }

        var engine = new Engine(config);

        try
        {
            var results = await engine.ExecuteAsync();

            // Output handlers have already displayed results
            // Just save UART log and return exit code

            // Save UART log
            if (!string.IsNullOrEmpty(results.UartLog))
            {
                string uartLogFile = "uart-output.log";
                await File.WriteAllTextAsync(uartLogFile, results.UartLog);
                Console.WriteLine($"\nUART log saved to: {uartLogFile}");
            }

            return results.AllTestsPassed ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ FATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }
}
