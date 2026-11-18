using System.ComponentModel;
using System.Diagnostics;
using Cosmos.Tools.Platform;
using Cosmos.Tools.Update;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cosmos.Tools.Commands;

public class UpdateSettings : CommandSettings
{
    [CommandOption("-y|--auto")]
    [Description("Automatically update without prompting")]
    public bool Auto { get; set; }

    [CommandOption("--check")]
    [Description("Report available updates without installing anything")]
    public bool Check { get; set; }

    [CommandOption("--version <VERSION>")]
    [Description("Update to a specific version instead of the latest release (must exist on your configured feeds)")]
    public string? Version { get; set; }

    [CommandOption("--prerelease")]
    [Description("Include prerelease versions")]
    public bool Prerelease { get; set; }

    [CommandOption("--project <DIR>")]
    [Description("Project directory whose Cosmos version pins should be updated (defaults to the current directory)")]
    public string? Project { get; set; }

    [CommandOption("--no-project")]
    [Description("Do not touch project files")]
    public bool NoProject { get; set; }
}

/// <summary>
/// One-step update of everything a Cosmos install is made of: the system-tools
/// bundles, the Cosmos.Patcher global tool, the project templates, the VS Code
/// extension, the current project's version pins, and — last, because replacing
/// the running tool must be the final act — the cosmos CLI itself.
/// </summary>
public class UpdateCommand : AsyncCommand<UpdateSettings>
{
    /// <summary>An update refusing to touch more files than this without an explicit --project is the monorepo-root guard.</summary>
    private const int MaxPinFilesWithoutExplicitProject = 8;

    private sealed record PinPlanEntry(string FilePath, ProjectPinUpdater.PinEdit Edit, string? SkipReason);

    public override async Task<int> ExecuteAsync(CommandContext context, UpdateSettings settings)
    {
        CommandHelper.PrintHeader("Cosmos Update", settings.Check ? "Check only" : null);

        // Validate inputs before anything runs — a bad flag must not abort the
        // update midway with half the steps already applied.
        if (settings.Version != null && !NuGetVersions.IsValidVersionRequest(settings.Version))
        {
            AnsiConsole.MarkupLine($"  [red]--version '{Markup.Escape(settings.Version)}' is not a valid version.[/]");
            return 1;
        }

        string? projectRoot = null;
        if (!settings.NoProject)
        {
            projectRoot = Path.GetFullPath(settings.Project ?? Directory.GetCurrentDirectory());
            if (!Directory.Exists(projectRoot))
            {
                AnsiConsole.MarkupLine($"  [red]--project: '{Markup.Escape(projectRoot)}' is not a directory.[/]");
                return 1;
            }
        }

        string currentCli = NuGetVersions.CurrentCliVersion();
        bool includePrerelease = settings.Prerelease || currentCli.Contains('-');

        string? latestTools;
        string? latestSdk;
        if (settings.Version != null)
        {
            latestTools = settings.Version;
            latestSdk = settings.Version;
        }
        else
        {
            // Plain client for nuget.org — InstallCommand.CreateHttpClient attaches
            // the user's GitHub token to every request and must not talk to NuGet.
            using HttpClient nuget = new HttpClient();
            nuget.DefaultRequestHeaders.Add("User-Agent", "Cosmos-Tools");
            Task<string?> toolsLookup = NuGetVersions.GetLatestVersionAsync(nuget, "Cosmos.Tools", includePrerelease);
            Task<string?> sdkLookup = NuGetVersions.GetLatestVersionAsync(nuget, "Cosmos.Sdk", includePrerelease);
            latestTools = await toolsLookup;
            latestSdk = await sdkLookup;

            if (latestTools == null && latestSdk == null)
            {
                AnsiConsole.MarkupLine("  [yellow]Could not reach nuget.org — package updates are unavailable.[/]");
            }
        }

        bool cliOutdated = latestTools != null && NuGetVersions.IsNewer(latestTools, currentCli);
        AnsiConsole.MarkupLine(cliOutdated
            ? $"  cosmos CLI: [yellow]{Markup.Escape(currentCli)}[/] -> [green]{Markup.Escape(latestTools!)}[/]"
            : $"  cosmos CLI: [green]{Markup.Escape(currentCli)}[/] [dim](up to date)[/]");

        // The pin plan is computed (and shown) before anything is confirmed or
        // written, so the user sees exactly which files an update would edit.
        List<PinPlanEntry> pinPlan = BuildPinPlan(projectRoot, latestSdk, settings);

        if (settings.Check)
        {
            await PrintBundleStatusAsync();
            PrintPinPlan(pinPlan, projectRoot, latestSdk);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  Run [blue]cosmos update[/] to apply available updates.");
            AnsiConsole.WriteLine();
            return 0;
        }

        PrintPinPlan(pinPlan, projectRoot, latestSdk);

        if (!settings.Auto)
        {
            AnsiConsole.WriteLine();
            bool proceed = AnsiConsole.Confirm("  Proceed with update?", false);
            if (!proceed)
            {
                AnsiConsole.WriteLine("  Update cancelled.");
                return 0;
            }
        }

        AnsiConsole.WriteLine();

        // System-tools bundles: InstallToolsFromReleaseAsync already compares each
        // bundle against the tools-latest asset versions and skips matches.
        bool toolsOk = await InstallCommand.InstallToolsFromReleaseAsync();

        // Pin the dotnet tools to the requested train when one was named, and to
        // the version the availability check found otherwise — an unpinned update
        // resolves "latest" through NuGet's http cache, which can lag a fresh
        // release and quietly leave the old version in place.
        string toolVersionArgs = settings.Version != null
            ? $" --version {settings.Version}"
            : latestSdk != null ? $" --version {latestSdk}" : "";
        string? templateVersion = settings.Version ?? (includePrerelease ? latestSdk : null);

        AnsiConsole.WriteLine();
        AnsiConsole.Markup("  Updating Cosmos.Patcher... ");
        await InstallCommand.InstallDotnetToolAsync("Cosmos.Patcher", toolVersionArgs);

        // `dotnet new install` refuses to touch an already-installed template
        // package, so the templates must be uninstalled first (the same shape the
        // Windows installer uses). That is only safe when a feed is reachable —
        // otherwise the reinstall fails and the templates are simply gone.
        AnsiConsole.Markup("  Updating Cosmos.Build.Templates... ");
        if (settings.Version != null || latestSdk != null || latestTools != null)
        {
            await RunQuietAsync("dotnet", "new uninstall Cosmos.Build.Templates");
            await InstallCommand.InstallTemplateAsync("Cosmos.Build.Templates", templateVersion);
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]SKIPPED (no feed reachable)[/]");
        }

        await InstallCommand.InstallVSCodeExtensionAsync();

        bool pinsOk = ApplyPinPlan(pinPlan, projectRoot);

        bool selfOk = await SelfUpdateAsync(currentCli, latestTools);

        UpdateNotifier.RecordUpdated(latestTools);

        bool ok = toolsOk && pinsOk && selfOk;
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine("  " + new string('-', 50));
        AnsiConsole.MarkupLine(ok ? "  [green]Update complete![/]" : "  [yellow]Update finished with errors (see above).[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  Run [blue]cosmos check[/] to verify the installation.");
        AnsiConsole.WriteLine();

        return ok ? 0 : 1;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  --check reporting
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task PrintBundleStatusAsync()
    {
        AnsiConsole.WriteLine();

        List<InstallCommand.ReleaseAsset> assets;
        try
        {
            assets = await InstallCommand.FetchReleaseAssetsAsync(InstallCommand.ToolsRepo, InstallCommand.ToolsReleaseTag);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"  [yellow]Could not reach the tools release: {Markup.Escape(ex.Message)}[/]");
            return;
        }

        string platform = InstallCommand.GetPlatformTarget();
        string ext = OperatingSystem.IsWindows() ? "zip" : "tar.gz";
        string toolsPath = ToolChecker.GetCosmosToolsPath();

        IEnumerable<string> releaseAssets = ToolDefinitions.GetAllTools()
            .OfType<CommandToolDefinition>()
            .Where(t => t.ReleaseAsset != null)
            .Select(t => t.ReleaseAsset!)
            .Distinct();

        foreach (string releaseAsset in releaseAssets)
        {
            string assetTarget = InstallCommand.GetAssetTarget(releaseAsset, platform);
            string pattern = $"{releaseAsset}-";
            string suffix = $"-{assetTarget}.{ext}";
            InstallCommand.ReleaseAsset? asset = assets.FirstOrDefault(a =>
                a.Name.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) &&
                a.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            int versionLength = asset == null ? 0 : asset.Name.Length - pattern.Length - suffix.Length;
            string? wanted = versionLength > 0 ? asset!.Name.Substring(pattern.Length, versionLength) : null;

            string versionFile = Path.Combine(toolsPath, releaseAsset, "VERSION");
            string? installed = File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() : null;

            if (wanted == null)
            {
                AnsiConsole.MarkupLine($"  {releaseAsset}: [yellow]no usable {assetTarget} asset in '{InstallCommand.ToolsReleaseTag}'[/]");
            }
            else if (installed == null)
            {
                AnsiConsole.MarkupLine($"  {releaseAsset}: [dim]no bundle (a matching system tool may be in use)[/] (available: {Markup.Escape(wanted)})");
            }
            else if (InstallCommand.BundleTargetDiffers(Path.Combine(toolsPath, releaseAsset), assetTarget))
            {
                AnsiConsole.MarkupLine($"  {releaseAsset}: [yellow]{Markup.Escape(installed)}, not a {assetTarget} build[/] -> [green]{Markup.Escape(wanted)}[/]");
            }
            else if (ToolResolver.VersionsMatch(wanted, installed))
            {
                AnsiConsole.MarkupLine($"  {releaseAsset}: [green]{Markup.Escape(installed)}[/] [dim](up to date)[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"  {releaseAsset}: [yellow]{Markup.Escape(installed)}[/] -> [green]{Markup.Escape(wanted)}[/]");
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Project pin updates
    // ═══════════════════════════════════════════════════════════════════════

    private static List<PinPlanEntry> BuildPinPlan(string? projectRoot, string? targetVersion, UpdateSettings settings)
    {
        List<PinPlanEntry> plan = [];
        if (projectRoot == null || targetVersion == null)
        {
            return plan;
        }

        List<string> files;
        try
        {
            files = ProjectPinUpdater.FindPinFiles(projectRoot);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"  [yellow]Could not scan {Markup.Escape(projectRoot)}: {Markup.Escape(ex.Message)}[/]");
            return plan;
        }

        if (settings.Project == null && files.Count > MaxPinFilesWithoutExplicitProject)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine(
                $"  [yellow]{files.Count} Cosmos project files found under {Markup.Escape(projectRoot)} — refusing to edit that many without an explicit --project.[/]");
            return plan;
        }

        foreach (string file in files)
        {
            ProjectPinUpdater.PinEdit edit;
            try
            {
                edit = ProjectPinUpdater.ComputeEdit(file, File.ReadAllText(file), targetVersion);
            }
            catch (Exception ex)
            {
                plan.Add(new PinPlanEntry(file, new ProjectPinUpdater.PinEdit("", 0, 0, []), $"unreadable: {ex.Message}"));
                continue;
            }

            if (edit.PinCount == 0)
            {
                // Only token/property/commented pins — nothing literal to move.
                continue;
            }

            if (edit.ChangedCount == 0)
            {
                plan.Add(new PinPlanEntry(file, edit, $"already {targetVersion}"));
                continue;
            }

            // A project pinned to something newer than the target (typically a
            // date-stamped local dev build) is only moved on an explicit --version;
            // a plain update must never silently downgrade it.
            string? newerPin = settings.Version == null
                ? edit.PreviousVersions.FirstOrDefault(v => NuGetVersions.IsNewer(v, targetVersion))
                : null;
            if (newerPin != null)
            {
                plan.Add(new PinPlanEntry(file, edit, $"pinned to newer {newerPin} — kept (use --version to override)"));
                continue;
            }

            plan.Add(new PinPlanEntry(file, edit, null));
        }

        return plan;
    }

    private static void PrintPinPlan(List<PinPlanEntry> plan, string? projectRoot, string? targetVersion)
    {
        if (projectRoot == null || plan.Count == 0)
        {
            return;
        }

        AnsiConsole.WriteLine();
        foreach (PinPlanEntry entry in plan)
        {
            string relative = Path.GetRelativePath(projectRoot, entry.FilePath);
            if (entry.SkipReason != null)
            {
                AnsiConsole.MarkupLine($"  {Markup.Escape(relative)}: [dim]{Markup.Escape(entry.SkipReason)}[/]");
                continue;
            }

            string previous = string.Join(", ", entry.Edit.PreviousVersions);
            AnsiConsole.MarkupLine(
                $"  {Markup.Escape(relative)}: [yellow]{Markup.Escape(previous)}[/] -> [green]{Markup.Escape(targetVersion ?? "?")}[/] ({entry.Edit.ChangedCount} pins)");
        }
    }

    private static bool ApplyPinPlan(List<PinPlanEntry> plan, string? projectRoot)
    {
        if (projectRoot == null)
        {
            return true;
        }

        List<PinPlanEntry> writable = plan.Where(entry => entry.SkipReason == null).ToList();
        if (writable.Count == 0)
        {
            return true;
        }

        AnsiConsole.WriteLine();
        bool ok = true;
        foreach (PinPlanEntry entry in writable)
        {
            string relative = Path.GetRelativePath(projectRoot, entry.FilePath);
            try
            {
                File.WriteAllText(entry.FilePath, entry.Edit.NewContent);
                string previous = string.Join(", ", entry.Edit.PreviousVersions);
                AnsiConsole.MarkupLine($"  {Markup.Escape(relative)}: [yellow]{Markup.Escape(previous)}[/] -> [green]updated[/] ({entry.Edit.ChangedCount} pins)");
            }
            catch (Exception ex)
            {
                ok = false;
                AnsiConsole.MarkupLine($"  [red]{Markup.Escape(relative)}: {Markup.Escape(ex.Message)}[/]");
            }
        }

        return ok;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CLI self-update — always the last step
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task<bool> SelfUpdateAsync(string current, string? latest)
    {
        AnsiConsole.WriteLine();
        if (latest == null || !NuGetVersions.IsNewer(latest, current))
        {
            AnsiConsole.MarkupLine($"  cosmos CLI [green]{Markup.Escape(current)}[/] is up to date.");
            return true;
        }

        // Pin the exact version the availability check just reported: a freshly
        // published release can be missing from NuGet's cached "latest"
        // resolution for a while, and an unpinned update then quietly reports
        // "already installed" while the old version stays in place. `latest`
        // passed IsNewer's numeric parse, so interpolating it is safe.
        string versionArgs = $" --version {latest}";

        if (OperatingSystem.IsWindows())
        {
            // A running global tool cannot replace itself on Windows: the .store
            // directory move fails on the locked DLLs mid-transaction and can leave
            // the shim broken. Hand the update to a detached shell that gives this
            // process ~2 seconds to exit first; the outcome shows on the next run.
            AnsiConsole.Markup($"  Updating cosmos CLI to {Markup.Escape(latest)} in the background... ");
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C timeout /T 2 /NOBREAK >nul & dotnet tool update --global Cosmos.Tools{versionArgs}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                AnsiConsole.MarkupLine("[green]OK[/]");
                AnsiConsole.MarkupLine($"  [dim]Version {Markup.Escape(latest)} becomes active on the next cosmos invocation.[/]");
                return true;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]FAILED[/]");
                AnsiConsole.MarkupLine($"  [red]{Markup.Escape(ex.Message)}[/]");
                AnsiConsole.MarkupLine("  Run [blue]dotnet tool update -g Cosmos.Tools[/] manually.");
                return false;
            }
        }

        // On Linux/macOS replacing the files of a running process is safe (rename
        // semantics) — the current process keeps its old inodes.
        AnsiConsole.Markup($"  Updating cosmos CLI {Markup.Escape(current)} -> {Markup.Escape(latest)}... ");
        (bool ok, string output) = await RunCaptureAsync("dotnet", $"tool update --global Cosmos.Tools{versionArgs}");

        // Exit code 0 also covers "already installed" — report the tool's own
        // outcome line instead of trusting the code.
        string? outcome = output
            .Split('\n')
            .Select(line => line.Trim())
            .LastOrDefault(line =>
                line.Contains("successfully", StringComparison.OrdinalIgnoreCase)
                || line.Contains("already installed", StringComparison.OrdinalIgnoreCase));

        if (ok)
        {
            AnsiConsole.MarkupLine("[green]OK[/]");
            if (outcome != null)
            {
                AnsiConsole.MarkupLine($"  [dim]{Markup.Escape(outcome)}[/]");
            }

            return true;
        }

        AnsiConsole.MarkupLine("[red]FAILED[/]");
        AnsiConsole.MarkupLine("  Run [blue]dotnet tool update -g Cosmos.Tools[/] manually.");
        return false;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  Process helpers
    // ═══════════════════════════════════════════════════════════════════════

    private static async Task RunQuietAsync(string fileName, string arguments)
    {
        try
        {
            using Process? process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
        catch
        {
            // Best-effort — the follow-up install reports the real outcome.
        }
    }

    private static async Task<(bool Ok, string Output)> RunCaptureAsync(string fileName, string arguments)
    {
        try
        {
            using Process? process = Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            if (process == null)
            {
                return (false, "");
            }

            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            return (process.ExitCode == 0, stdout + "\n" + stderr);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
