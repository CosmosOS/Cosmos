using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Cosmos.Tools.Platform;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cosmos.Tools.Commands;

public class InstallSettings : CommandSettings
{
    [CommandOption("-y|--auto")]
    [Description("Automatically install without prompting")]
    public bool Auto { get; set; }

    [CommandOption("--setup <DIR>")]
    [Description("Bundle all tools into DIR for offline installer packaging")]
    public string? Setup { get; set; }

    [CommandOption("--tools")]
    [Description("Only install system tools (QEMU, clang, lld, xorriso) from the Cosmos tools release")]
    public bool Tools { get; set; }

    [CommandOption("--packages")]
    [Description("Only install Cosmos dotnet tools, templates, and VS Code extension")]
    public bool Packages { get; set; }
}

public class InstallCommand : AsyncCommand<InstallSettings>
{
    /// <summary>Repository whose <c>tools-latest</c> release carries the LLVM, QEMU, xorriso and GDB archives.</summary>
    internal const string ToolsRepo = "CosmosOS/Cosmos";

    /// <summary>Rolling release tag build-tools.yml republishes the toolchain archives under.</summary>
    internal const string ToolsReleaseTag = "tools-latest";

    public override async Task<int> ExecuteAsync(CommandContext context, InstallSettings settings)
    {
        string mode = settings.Setup != null
            ? $"Setup bundle -> {Path.GetFullPath(settings.Setup)}"
            : "Local install";
        CommandHelper.PrintHeader("Cosmos Tools Installer", mode);

        if (!settings.Auto)
        {
            bool proceed = AnsiConsole.Confirm("  Proceed with installation?", false);
            if (!proceed)
            {
                AnsiConsole.WriteLine("  Installation cancelled.");
                return 0;
            }
            AnsiConsole.WriteLine();
        }

        if (settings.Setup != null)
        {
            return await BuildSetupAsync(settings);
        }

        // When neither --tools nor --packages is specified, install everything
        bool installTools = !settings.Packages || settings.Tools;
        bool installPackages = !settings.Tools || settings.Packages;

        if (installTools)
        {
            bool toolsInstalled = await InstallToolsFromReleaseAsync();
            await PropagateToolPathsForCIAsync();
            if (!toolsInstalled)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("  [red]Installation failed: required build tools are still missing.[/]");
                return 1;
            }
        }

        if (installPackages)
        {
            await InstallDotnetToolsAsync();
            await InstallVSCodeExtensionAsync();
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("  " + new string('-', 50));
        AnsiConsole.MarkupLine("  [green]Installation complete![/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  Run [blue]cosmos check[/] to verify installation.");
        AnsiConsole.MarkupLine("  Run [blue]cosmos new[/] to create a new kernel project.");
        AnsiConsole.WriteLine();

        return 0;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Tool installation — download from GitHub release `tools-latest`
    // ═══════════════════════════════════════════════════════════════════════

    internal static async Task<bool> InstallToolsFromReleaseAsync()
    {
        string platform = GetPlatformTarget();
        string ext = OperatingSystem.IsWindows() ? "zip" : "tar.gz";
        string toolsPath = ToolChecker.GetCosmosToolsPath();
        Directory.CreateDirectory(toolsPath);

        // Check .NET SDK (manual install only)
        var dotnetStatus = await ToolChecker.CheckToolAsync(ToolDefinitions.DotNetSdk);
        if (dotnetStatus.Found)
        {
            string ver = dotnetStatus.Version != null ? $" (v{dotnetStatus.Version})" : "";
            AnsiConsole.MarkupLine($"  [dim].NET SDK{ver} found at {dotnetStatus.Path}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("  [yellow].NET SDK not found.[/] Download from: https://dot.net/download");
        }

        // Fetch release asset list once
        List<ReleaseAsset> assets;
        try
        {
            assets = await FetchReleaseAssetsAsync(ToolsRepo, ToolsReleaseTag);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"  [red]Failed to fetch release '{ToolsReleaseTag}' from {ToolsRepo}: {Markup.Escape(ex.Message)}[/]");
            return false;
        }

        // Download each unique release asset (dedupe by asset name — llvm-tools covers clang+lld, qemu covers x64+arm64)
        bool anyDownloaded = false;

        var groups = ToolDefinitions.GetAllTools()
            .OfType<CommandToolDefinition>()
            .Where(t => t.ReleaseAsset != null)
            .GroupBy(t => t.ReleaseAsset!);

        foreach (var group in groups)
        {
            string releaseAsset = group.Key;

            // Find the matching asset BEFORE checking system tools — its filename
            // is the source of truth for "what version cosmos.tools wants".
            string pattern = $"{releaseAsset}-";
            string suffix = $"-{platform}.{ext}";
            var asset = assets.FirstOrDefault(a =>
                a.Name.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) &&
                a.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            if (asset == null)
            {
                AnsiConsole.MarkupLine($"  [red]{releaseAsset}:[/] no asset matching [white]{pattern}*{suffix}[/] in release '{ToolsReleaseTag}'");
                continue;
            }

            // Asset name -> "<assetPrefix>-<version>-<platform>.<ext>"
            int versionLength = asset.Name.Length - pattern.Length - suffix.Length;
            if (versionLength <= 0)
            {
                AnsiConsole.MarkupLine($"  [red]{releaseAsset}:[/] malformed asset name [white]{asset.Name}[/] in release '{ToolsReleaseTag}'");
                continue;
            }
            string wantedVersion = asset.Name.Substring(pattern.Length, versionLength);

            bool allFoundAndMatching = true;
            foreach (var tool in group)
            {
                var status = await ToolChecker.CheckToolAsync(tool);
                if (!status.Found)
                {
                    AnsiConsole.MarkupLine($"  [dim]{tool.DisplayName} not found.[/]");
                    allFoundAndMatching = false;
                    continue;
                }
                if (!ToolResolver.VersionsMatch(wantedVersion, status.Version))
                {
                    AnsiConsole.MarkupLine(
                        $"  [yellow]{tool.DisplayName}[/] found at [white]{status.Path}[/] but version [red]{status.Version ?? "?"}[/] != wanted [green]{wantedVersion}[/] — will use bundle.");
                    allFoundAndMatching = false;
                    continue;
                }
                AnsiConsole.MarkupLine($"  [dim]{tool.DisplayName} (v{status.Version}) found at {status.Path}[/]");
            }

            if (allFoundAndMatching)
            {
                continue;
            }

            AnsiConsole.Markup($"  Downloading {releaseAsset} ({wantedVersion}) ... ");
            bool ok = await DownloadAndExtractAsync(asset.DownloadUrl, toolsPath, ext);
            AnsiConsole.MarkupLine(ok ? "[green]OK[/]" : "[red]FAILED[/]");
            if (ok)
            {
                anyDownloaded = true;
            }
        }

        // On Windows, add downloaded tool dirs to user PATH
        if (anyDownloaded && OperatingSystem.IsWindows())
        {
            AnsiConsole.Markup("  Tools -> PATH ... ");
            bool pathOk = AddToolsToWindowsPath(toolsPath);
            AnsiConsole.MarkupLine(pathOk ? "[green]OK[/]" : "[yellow]SKIPPED[/]");
        }

        // Pre-install ResolveAsync calls cached system-tool hits while the bundle
        // was still missing. Drop them so the verification below sees the freshly
        // extracted bundle binaries instead of the stale pre-download resolution.
        if (anyDownloaded)
        {
            ToolResolver.InvalidateCache();
        }

        List<CommandToolDefinition> requiredReleaseTools = ToolDefinitions.GetAllTools()
            .OfType<CommandToolDefinition>()
            .Where(static t => t.Required && t.ReleaseAsset != null)
            .ToList();

        List<CommandToolDefinition> missingRequired = new List<CommandToolDefinition>();
        foreach (CommandToolDefinition tool in requiredReleaseTools)
        {
            ToolStatus status = await ToolChecker.CheckToolAsync(tool);
            bool detected = status.Found && status.Version != null;
            if (!detected)
            {
                missingRequired.Add(tool);
            }
        }

        if (missingRequired.Count > 0)
        {
            string missingList = string.Join(", ", missingRequired.Select(static t => t.DisplayName));
            AnsiConsole.MarkupLine($"  [red]Required tools still missing after install:[/] {Markup.Escape(missingList)}");
            return false;
        }

        return true;
    }

    internal static string GetPlatformTarget()
    {
        if (OperatingSystem.IsWindows())
        {
            return "win-x64";
        }
        if (OperatingSystem.IsMacOS())
        {
            return "darwin-arm64";
        }
        return "linux-x64";
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  GitHub Release asset fetching
    // ═══════════════════════════════════════════════════════════════════════

    internal record ReleaseAsset(string Name, string DownloadUrl);

    internal static async Task<List<ReleaseAsset>> FetchReleaseAssetsAsync(string repo, string tag)
    {
        using var http = CreateHttpClient();
        string url = $"https://api.github.com/repos/{repo}/releases/tags/{tag}";
        string json = await http.GetStringAsync(url);
        var doc = JsonDocument.Parse(json);

        var result = new List<ReleaseAsset>();
        if (doc.RootElement.TryGetProperty("assets", out var assetsEl))
        {
            foreach (var asset in assetsEl.EnumerateArray())
            {
                string? name = asset.GetProperty("name").GetString();
                string? downloadUrl = asset.GetProperty("browser_download_url").GetString();
                if (name != null && downloadUrl != null)
                {
                    result.Add(new ReleaseAsset(name, downloadUrl));
                }
            }
        }
        return result;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Download & extract helpers — tar.gz for Unix, zip for Windows
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task<bool> DownloadAndExtractAsync(string url, string targetDir, string ext)
    {
        try
        {
            Directory.CreateDirectory(targetDir);
            return ext switch
            {
                "zip" => await DownloadAndExtractZipAsync(url, targetDir),
                "tar.gz" => await DownloadAndExtractTarGzAsync(url, targetDir),
                _ => false
            };
        }
        catch (Exception ex)
        {
            AnsiConsole.Markup($"[red]{Markup.Escape(ex.Message)}[/] ");
            return false;
        }
    }

    private static async Task<bool> DownloadAndExtractZipAsync(string url, string targetDir)
    {
        using var http = CreateHttpClient();
        string tempFile = Path.Combine(Path.GetTempPath(), $"cosmos-{Guid.NewGuid():N}.zip");
        try
        {
            await File.WriteAllBytesAsync(tempFile, await http.GetByteArrayAsync(url));
            ZipFile.ExtractToDirectory(tempFile, targetDir, overwriteFiles: true);
            return true;
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private static async Task<bool> DownloadAndExtractTarGzAsync(string url, string targetDir)
    {
        using var http = CreateHttpClient();
        string tempFile = Path.Combine(Path.GetTempPath(), $"cosmos-{Guid.NewGuid():N}.tar.gz");
        try
        {
            await File.WriteAllBytesAsync(tempFile, await http.GetByteArrayAsync(url));

            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "tar",
                Arguments = $"xzf \"{tempFile}\" -C \"{targetDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (proc == null)
            {
                return false;
            }

            await proc.WaitForExitAsync();
            return proc.ExitCode == 0;
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  VS Code extension
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task<(string? url, string? name)> GetVSCodeExtensionInfoAsync()
    {
        using var http = CreateHttpClient();
        string json = await http.GetStringAsync(
            "https://api.github.com/repos/valentinbreiz/CosmosVsCodeExtension/releases/latest");
        var release = JsonDocument.Parse(json);

        if (release.RootElement.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                string? name = asset.GetProperty("name").GetString();
                if (name?.EndsWith(".vsix") == true)
                {
                    return (asset.GetProperty("browser_download_url").GetString(), name);
                }
            }
        }
        return (null, null);
    }

    // Downloads the latest .vsix to destDir. Returns the written file path,
    // or null if no asset was found. Caller prints context-appropriate output.
    private static async Task<string?> DownloadVSCodeExtensionAsync(string destDir)
    {
        var (url, name) = await GetVSCodeExtensionInfoAsync();
        if (url == null || name == null)
        {
            return null;
        }
        Directory.CreateDirectory(destDir);
        string path = Path.Combine(destDir, name);
        using var http = CreateHttpClient();
        await File.WriteAllBytesAsync(path, await http.GetByteArrayAsync(url));
        return path;
    }

    internal static async Task InstallVSCodeExtensionAsync()
    {
        string? codeCommand = GetVSCodeCommand();
        if (codeCommand == null)
        {
            AnsiConsole.MarkupLine("  [yellow]VS Code not found in PATH.[/]");
            return;
        }

        AnsiConsole.Markup("  Downloading extension from GitHub... ");
        try
        {
            string? tempPath = await DownloadVSCodeExtensionAsync(Path.GetTempPath());
            if (tempPath == null)
            {
                AnsiConsole.MarkupLine("[yellow]SKIPPED (no .vsix found)[/]");
                return;
            }
            AnsiConsole.MarkupLine("[green]OK[/]");

            AnsiConsole.Markup("  Installing extension... ");
            ProcessStartInfo psi = OperatingSystem.IsWindows()
                ? new() { FileName = "cmd.exe", Arguments = $"/c {codeCommand} --install-extension \"{tempPath}\" --force", UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true }
                : new() { FileName = codeCommand, Arguments = $"--install-extension \"{tempPath}\" --force", UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };

            using var process = Process.Start(psi);
            if (process == null) { AnsiConsole.MarkupLine("[yellow]SKIPPED[/]"); return; }
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                AnsiConsole.MarkupLine("[green]OK[/]");
            }
            else
            {
                string error = await process.StandardError.ReadToEndAsync();
                AnsiConsole.MarkupLine("[red]FAILED[/]");
                if (!string.IsNullOrWhiteSpace(error))
                {
                    AnsiConsole.MarkupLine($"  [red]{Markup.Escape(error)}[/]");
                }
            }

            try { File.Delete(tempPath); } catch { }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]FAILED[/]");
            AnsiConsole.MarkupLine($"  [red]Error: {Markup.Escape(ex.Message)}[/]");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Dotnet tools
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task InstallDotnetToolsAsync()
    {
        AnsiConsole.WriteLine();

        AnsiConsole.Markup("  Installing Cosmos.Patcher... ");
        await InstallDotnetToolAsync("Cosmos.Patcher");

        AnsiConsole.Markup("  Installing Cosmos.Build.Templates... ");
        await InstallTemplateAsync("Cosmos.Build.Templates");
    }

    internal static async Task InstallDotnetToolAsync(string packageName, string extraArgs = "")
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"tool update -g {packageName}{extraArgs}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            if (process == null) { AnsiConsole.MarkupLine("[yellow]SKIPPED[/]"); return; }
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                AnsiConsole.MarkupLine("[green]OK[/]");
            }
            else
            {
                psi.Arguments = $"tool install -g {packageName}{extraArgs}";
                using var installProcess = Process.Start(psi);
                if (installProcess != null)
                {
                    await installProcess.WaitForExitAsync();
                    AnsiConsole.MarkupLine(installProcess.ExitCode == 0 ? "[green]OK[/]" : "[yellow]SKIPPED[/]");
                }
            }
        }
        catch { AnsiConsole.MarkupLine("[yellow]SKIPPED[/]"); }
    }

    internal static async Task InstallTemplateAsync(string packageName, string? version = null)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"new install {packageName}{(version != null ? $"::{version}" : "")}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using var process = Process.Start(psi);
            if (process == null) { AnsiConsole.MarkupLine("[yellow]SKIPPED[/]"); return; }
            await process.WaitForExitAsync();
            AnsiConsole.MarkupLine(process.ExitCode == 0 ? "[green]OK[/]" : "[yellow]SKIPPED[/]");
        }
        catch { AnsiConsole.MarkupLine("[yellow]SKIPPED[/]"); }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Shared helpers
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task PropagateToolPathsForCIAsync()
    {
        string? githubPath = Environment.GetEnvironmentVariable("GITHUB_PATH");
        if (githubPath == null)
        {
            return;
        }

        var addedDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var tool in ToolDefinitions.GetAllTools())
        {
            if (tool is not CommandToolDefinition cmdTool)
            {
                continue;
            }

            var status = await ToolChecker.CheckToolAsync(cmdTool);
            if (status.Path == null)
            {
                continue;
            }

            string? dir = Path.GetDirectoryName(status.Path);
            if (dir != null && addedDirs.Add(dir))
            {
                File.AppendAllText(githubPath, dir + "\n");
            }
        }
    }

    // Tool directories that should be on PATH. QEMU exes live under qemu\bin\
    // (so the bundle's qemu\share\qemu\ BIOS files are discoverable via -L,
    // which launchers add automatically).
    private static readonly string[] s_toolPathSubDirs =
    [
        Path.Combine("llvm-tools", "bin"),
        "xorriso",
        Path.Combine("qemu", "bin"),
        Path.Combine("gdb", "bin")
    ];

    internal static bool AddToolsToWindowsPath(string toolsPath)
    {
        try
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            bool changed = false;

            foreach (string sub in s_toolPathSubDirs)
            {
                string dir = Path.Combine(toolsPath, sub);
                if (!currentPath.Contains(dir, StringComparison.OrdinalIgnoreCase))
                {
                    currentPath = currentPath.Length > 0 ? $"{currentPath};{dir}" : dir;
                    changed = true;
                }
            }

            if (changed)
            {
                Environment.SetEnvironmentVariable("PATH", currentPath, EnvironmentVariableTarget.User);
            }

            return true;
        }
        catch { return false; }
    }

    internal static bool RemoveToolsFromWindowsPath(string toolsPath)
    {
        try
        {
            string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            bool changed = false;
            foreach (string sub in s_toolPathSubDirs)
            {
                string dir = Path.Combine(toolsPath, sub);
                string upper = currentPath.ToUpperInvariant();
                string upperDir = dir.ToUpperInvariant();
                int idx = upper.IndexOf(upperDir);
                if (idx >= 0)
                {
                    currentPath = currentPath.Remove(idx, dir.Length);
                    currentPath = currentPath.Replace(";;", ";").Trim(';');
                    changed = true;
                }
            }
            if (changed)
            {
                Environment.SetEnvironmentVariable("PATH", currentPath, EnvironmentVariableTarget.User);
            }

            return true;
        }
        catch { return false; }
    }

    private static HttpClient CreateHttpClient()
    {
        var http = new HttpClient();
        http.DefaultRequestHeaders.Add("User-Agent", "Cosmos-Tools");
        string? token = GetGitHubToken();
        if (!string.IsNullOrEmpty(token))
        {
            http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
        return http;
    }

    private static string? GetGitHubToken()
    {
        string? token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        token = Environment.GetEnvironmentVariable("GH_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        token = Environment.GetEnvironmentVariable("GITHUB_API_TOKEN");
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    private static string? GetVSCodeCommand()
    {
        bool isWindows = OperatingSystem.IsWindows();
        string[] commands = isWindows
            ? ["code.cmd", "code", "code-insiders.cmd", "code-insiders", "codium.cmd", "codium"]
            : ["code", "code-insiders", "codium"];

        foreach (string cmd in commands)
        {
            try
            {
                ProcessStartInfo psi = isWindows
                    ? new() { FileName = "cmd.exe", Arguments = $"/c {cmd} --version", UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true }
                    : new() { FileName = cmd, Arguments = "--version", UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    process.WaitForExit(3000);
                    if (process.ExitCode == 0)
                    {
                        return cmd;
                    }
                }
            }
            catch { }
        }
        return null;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Setup bundle mode — downloads tools-latest Windows assets for the
    //  offline installer, stages packages/extension, then runs Inno Setup.
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task<int> BuildSetupAsync(InstallSettings settings)
    {
        if (!OperatingSystem.IsWindows())
        {
            AnsiConsole.MarkupLine("  [red]Setup mode is only supported on Windows.[/]");
            return 1;
        }

        string baseDir = Path.GetFullPath(settings.Setup!);
        string toolsDir = Path.Combine(baseDir, "tools", "windows");
        Directory.CreateDirectory(toolsDir);

        List<ReleaseAsset> assets = await FetchReleaseAssetsAsync(ToolsRepo, ToolsReleaseTag);
        var releaseAssets = ToolDefinitions.GetAllTools()
            .OfType<CommandToolDefinition>()
            .Where(t => t.ReleaseAsset != null)
            .Select(t => t.ReleaseAsset!)
            .Distinct();

        foreach (string releaseAsset in releaseAssets)
        {
            string suffix = $"-win-x64.zip";
            var asset = assets.FirstOrDefault(a =>
                a.Name.StartsWith($"{releaseAsset}-", StringComparison.OrdinalIgnoreCase) &&
                a.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            {
                AnsiConsole.MarkupLine($"  [red]{releaseAsset}: no win-x64 asset in '{ToolsReleaseTag}'[/]");
                continue;
            }
            AnsiConsole.Markup($"  {releaseAsset} -> tools/windows/{releaseAsset}/ ... ");
            bool ok = await DownloadAndExtractAsync(asset.DownloadUrl, toolsDir, "zip");
            AnsiConsole.MarkupLine(ok ? "[green]OK[/]" : "[red]FAILED[/]");
        }

        AnsiConsole.Markup("  VS Code Extension ... ");
        string? vsix = await DownloadVSCodeExtensionAsync(Path.Combine(baseDir, "extensions"));
        AnsiConsole.MarkupLine(vsix != null ? "[green]OK[/]" : "[yellow]SKIPPED (no .vsix found)[/]");

        string issFile = Path.Combine(Path.GetDirectoryName(baseDir)!, "Cosmos.iss");
        if (File.Exists(issFile))
        {
            AnsiConsole.Markup("  Windows installer -> iscc ... ");
            bool ok = await BuildInnoSetupAsync(issFile, baseDir);
            AnsiConsole.MarkupLine(ok ? "[green]OK[/]" : "[red]FAILED[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("  " + new string('-', 50));
        AnsiConsole.MarkupLine("  [green]Setup bundle complete![/]");
        AnsiConsole.WriteLine();
        return 0;
    }

    private static async Task<bool> BuildInnoSetupAsync(string issFile, string baseDir)
    {
        string packagesDir = Path.Combine(baseDir, "packages");
        string? version = null;
        if (Directory.Exists(packagesDir))
        {
            var sdkPkg = Directory.GetFiles(packagesDir, "Cosmos.Sdk.*.nupkg").FirstOrDefault();
            if (sdkPkg != null)
            {
                string name = Path.GetFileNameWithoutExtension(sdkPkg);
                version = name["Cosmos.Sdk.".Length..];
            }
        }

        var psi = new ProcessStartInfo
        {
            FileName = "iscc",
            Arguments = $"\"{issFile}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        if (version != null)
        {
            psi.Environment["COSMOS_VERSION"] = version;
        }

        using var proc = Process.Start(psi);
        if (proc == null)
        {
            return false;
        }

        await proc.StandardOutput.ReadToEndAsync();
        await proc.WaitForExitAsync();
        return proc.ExitCode == 0;
    }
}
