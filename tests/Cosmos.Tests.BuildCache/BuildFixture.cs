using System.Diagnostics;
using System.Reflection;

namespace Cosmos.Tests.BuildCache;

/// <summary>
/// Shared fixture for build cache tests.
/// Resolves paths and provides build/marker helpers.
/// </summary>
public class BuildFixture
{
    private const string Arch = "x64";
    private const string Rid = "linux-x64";
    private const string Define = "ARCH_X64";

    // Sourced from $(VersionPrefix) at build time via [AssemblyMetadata("CosmosVersion", ...)]
    // injected by Cosmos.Tests.BuildCache.csproj. Single source of truth = Directory.Build.props.
    private static readonly string CosmosVersion = typeof(BuildFixture).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .First(a => a.Key == "CosmosVersion").Value!;

    public string RootDir { get; }
    public string ObjDir { get; }
    public string OutputDir { get; }
    public string ElfFile { get; }
    public string IsoFile { get; }
    public string PatcherHashFile { get; }
    public string IlcHashFile { get; }
    public string LinkHashFile { get; }
    public string IsoHashFile { get; }
    public string IlcOutput { get; }
    public string AsmObjDir { get; }
    public string CObjDir { get; }

    // Source files for change tests
    public string DevKernelCs { get; }
    public string AsmFile { get; }
    public string CFile { get; }
    public string DevKernelCDir { get; }

    private string DevKernelCsproj { get; }

    public BuildFixture()
    {
        // Walk up from test bin dir to repo root
        RootDir = FindRepoRoot();

        string objBase = Path.Combine(RootDir, "artifacts", "obj", "DevKernel", $"debug_{Rid}");
        string binBase = Path.Combine(RootDir, "artifacts", "bin", "DevKernel", $"debug_{Rid}");

        ObjDir = objBase;
        OutputDir = Path.Combine(RootDir, $"output-{Arch}");
        ElfFile = Path.Combine(binBase, "DevKernel.elf");
        IsoFile = Path.Combine(binBase, "cosmos", "DevKernel.iso");
        PatcherHashFile = Path.Combine(objBase, "cosmos", ".patcher-hash");
        IlcHashFile = Path.Combine(objBase, "cosmos", "native", ".ilc-hash");
        LinkHashFile = Path.Combine(objBase, "cosmos", ".link-hash");
        IsoHashFile = Path.Combine(objBase, "cosmos", ".iso-hash");
        IlcOutput = Path.Combine(objBase, "cosmos", "native", "DevKernel.o");
        AsmObjDir = Path.Combine(objBase, "cosmos", "asm");
        CObjDir = Path.Combine(objBase, "cosmos", "cobj");

        DevKernelCsproj = Path.Combine(RootDir, "examples", "DevKernel", "DevKernel.csproj");
        DevKernelCs = Path.Combine(RootDir, "examples", "DevKernel", "Kernel.cs");

        // ASM source: DevKernel uses NuGet packages, so the ASM file the build actually
        // reads lives inside the user's NuGet cache, not the repo source tree.
        string nugetHome = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
                           ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        AsmFile = Path.Combine(nugetHome, "cosmos.kernel.native.x64", CosmosVersion, "build", "Runtime", "Runtime.s");

        CFile = Path.Combine(RootDir, "examples", "DevKernel", "src", "C", "test.c");
        DevKernelCDir = Path.Combine(RootDir, "examples", "DevKernel", "src", "C");
    }

    /// <summary>
    /// Run dotnet publish on the DevKernel project.
    /// </summary>
    public BuildResult Build()
    {
        ProcessStartInfo psi = new()
        {
            FileName = "dotnet",
            Arguments = $"publish -c Debug -r {Rid} " +
                        $"-p:DefineConstants=\"{Define}\" -p:CosmosArch={Arch} " +
                        $"\"{DevKernelCsproj}\" -o \"{OutputDir}\"",
            WorkingDirectory = RootDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Ensure dotnet tools are on PATH
        string path = Environment.GetEnvironmentVariable("PATH") ?? "";
        string toolsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools");
        if (!path.Contains(toolsDir))
        {
            psi.Environment["PATH"] = $"{toolsDir}:{path}";
        }

        using Process process = Process.Start(psi)!;

        // Read stdout/stderr concurrently to avoid deadlocks
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit((int)TimeSpan.FromMinutes(5).TotalMilliseconds);

        string stdout = stdoutTask.GetAwaiter().GetResult();
        string stderr = stderrTask.GetAwaiter().GetResult();

        return new BuildResult(process.ExitCode == 0, stdout, stderr);
    }

    /// <summary>
    /// Injects a marker into a source file that actually affects the compiled output.
    /// Comment-only changes don't change .o/.obj bytes (compilers strip comments), so we
    /// inject something the assembler/compiler keeps in the output. Returns an IDisposable
    /// that reverts the change.
    /// </summary>
    public IDisposable InjectMarker(string filePath, string fileType)
    {
        string original = File.ReadAllText(filePath);
        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string marker = fileType switch
        {
            // C# comment + a new field — affects metadata + IL
            "cs" => $"// CACHE_TEST_{ts}\n",
            // GAS: append a unique global symbol so the .obj actually differs
            "asm" => $"\n.global cache_test_{ts}\n.section .cache_test_{ts}\n.byte {ts & 0xff}\n",
            // C: append a unique function so the .o actually differs
            "c" => $"\nvoid cache_test_{ts}(void) {{ (void)0; }}\n",
            _ => $"// CACHE_TEST_{ts}\n"
        };

        // For asm/c: append at end (avoid breaking top-of-file directives)
        // For cs: prepend (top-of-file comment is fine in C#)
        string newContent = fileType == "cs" ? marker + original : original + marker;
        File.WriteAllText(filePath, newContent);
        return new FileRestorer(filePath, original);
    }

    private static string FindRepoRoot()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "CLAUDE.md")) &&
                Directory.Exists(Path.Combine(dir, "examples", "DevKernel")))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir)!;
        }

        throw new InvalidOperationException(
            "Could not find repo root. Run tests from within the Cosmos repository.");
    }

    private sealed class FileRestorer : IDisposable
    {
        private readonly string _path;
        private readonly string _original;

        public FileRestorer(string path, string original)
        {
            _path = path;
            _original = original;
        }

        public void Dispose()
        {
            File.WriteAllText(_path, _original);
        }
    }
}

public record BuildResult(bool Success, string Stdout, string Stderr)
{
    /// <summary>Combined stdout+stderr for assertion messages (MSBuild writes errors to stdout).</summary>
    public string Output => string.IsNullOrEmpty(Stderr) ? Stdout : $"{Stdout}\n--- stderr ---\n{Stderr}";
}
