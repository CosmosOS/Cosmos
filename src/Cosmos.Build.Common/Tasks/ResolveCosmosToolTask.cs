// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;

namespace Cosmos.Build.Common.Tasks;

/// <summary>
/// Resolves a Cosmos toolchain binary (clang, ld.lld, xorriso, qemu-*) using
/// the same policy as Cosmos.Tools.Platform.ToolResolver:
///   1. explicit override path (e.g. user-set MSBuild $(CCPath))
///   2. system tool on PATH, accepted only if its version matches the bundle's
///      VERSION stamp under ~/.cosmos/tools/&lt;asset&gt;/VERSION
///   3. cosmos-bundled binary under ~/.cosmos/tools/ (or %LOCALAPPDATA%\Cosmos\Tools).
///
/// Keep this in sync with src/Cosmos.Tools/Platform/ToolResolver.cs — the algorithm
/// and bundle layout are duplicated only because Cosmos.Tools is net10.0 and MSBuild
/// tasks must be netstandard2.0.
/// </summary>
public sealed class ResolveCosmosToolTask : Microsoft.Build.Utilities.Task
{
    private const int CommandTimeoutMs = 5000;

    /// <summary>Logical tool name: clang | lld | xorriso | qemu-x64 | qemu-arm64 | qemu-riscv64.</summary>
    [Required]
    public string ToolName { get; set; } = string.Empty;

    /// <summary>Optional explicit override path (wins unconditionally if it points to a real file).</summary>
    public string OverridePath { get; set; } = string.Empty;

    [Output]
    public string ResolvedPath { get; set; } = string.Empty;

    /// <summary>One of: Override, System, Bundle, NotFound.</summary>
    [Output]
    public string Source { get; set; } = "NotFound";

    public override bool Execute()
    {
        ToolSpec? spec = LookupTool(ToolName);
        if (spec is null)
        {
            Log.LogError("ResolveCosmosTool: unknown tool name '{0}'. Valid: clang, lld, xorriso, qemu-x64, qemu-arm64, qemu-riscv64.", ToolName);
            return false;
        }

        // 1. Override.
        if (!string.IsNullOrEmpty(OverridePath) && File.Exists(OverridePath))
        {
            ResolvedPath = OverridePath;
            Source = "Override";
            Log.LogMessage(MessageImportance.Low, "ResolveCosmosTool({0}): override -> {1}", ToolName, OverridePath);
            return true;
        }

        // 2. System tool, gated by bundle VERSION match.
        string? pinnedVersion = ReadBundleVersion(spec);
        foreach (string command in spec.Commands)
        {
            string? systemPath = FindOnPath(command);
            if (systemPath is null)
            {
                continue;
            }
            string? systemVersion = TryGetVersion(systemPath);
            if (pinnedVersion is null || VersionsMatch(pinnedVersion, systemVersion))
            {
                ResolvedPath = systemPath;
                Source = "System";
                Log.LogMessage(MessageImportance.Low, "ResolveCosmosTool({0}): system {1} (v{2}) -> {3}", ToolName, command, systemVersion ?? "?", systemPath);
                return true;
            }
            Log.LogMessage(MessageImportance.Low, "ResolveCosmosTool({0}): system {1} v{2} != bundle v{3}, falling back",
                ToolName, command, systemVersion ?? "?", pinnedVersion);
        }

        // 3. Bundle.
        string? bundlePath = FindInBundle(spec);
        if (bundlePath != null)
        {
            ResolvedPath = bundlePath;
            Source = "Bundle";
            Log.LogMessage(MessageImportance.Low, "ResolveCosmosTool({0}): bundle -> {1}", ToolName, bundlePath);
            return true;
        }

        // Fall back to bare command name; the build's own <Exec> will surface a
        // useful error if the shell can't find it.
        ResolvedPath = spec.Commands[0];
        Source = "NotFound";
        Log.LogMessage(MessageImportance.Normal, "ResolveCosmosTool({0}): not found in override/system/bundle; will rely on PATH for '{1}'.", ToolName, spec.Commands[0]);
        return true;
    }

    private sealed class ToolSpec
    {
        public string ReleaseAsset { get; set; } = string.Empty;
        public string[] Commands { get; set; } = Array.Empty<string>();
        public bool InBinSubdir { get; set; }
    }

    private static ToolSpec? LookupTool(string name)
    {
        switch (name.ToLowerInvariant())
        {
            case "clang": return new ToolSpec { ReleaseAsset = "llvm-tools", Commands = new[] { "clang" }, InBinSubdir = true };
            case "lld": return new ToolSpec { ReleaseAsset = "llvm-tools", Commands = new[] { "ld.lld", "lld" }, InBinSubdir = true };
            case "xorriso": return new ToolSpec { ReleaseAsset = "xorriso", Commands = new[] { "xorriso" } };
            case "qemu-x64": return new ToolSpec { ReleaseAsset = "qemu", Commands = new[] { "qemu-system-x86_64" } };
            case "qemu-arm64": return new ToolSpec { ReleaseAsset = "qemu", Commands = new[] { "qemu-system-aarch64" } };
            case "qemu-riscv64": return new ToolSpec { ReleaseAsset = "qemu", Commands = new[] { "qemu-system-riscv64" } };
            default: return null;
        }
    }

    private static string CosmosToolsBase()
    {
        bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        if (isWindows)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Cosmos", "Tools");
        }
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".cosmos", "tools");
    }

    private static string Ext()
    {
        return Environment.OSVersion.Platform == PlatformID.Win32NT ? ".exe" : "";
    }

    private static string? ReadBundleVersion(ToolSpec spec)
    {
        string versionFile = Path.Combine(CosmosToolsBase(), spec.ReleaseAsset, "VERSION");
        if (!File.Exists(versionFile))
        {
            return null;
        }
        try
        {
            return File.ReadAllText(versionFile).Trim();
        }
        catch
        {
            return null;
        }
    }

    private static string? FindInBundle(ToolSpec spec)
    {
        string root = Path.Combine(CosmosToolsBase(), spec.ReleaseAsset);
        string ext = Ext();
        string[] subDirs = spec.InBinSubdir ? new[] { "bin", "" } : new[] { "", "bin" };
        foreach (string command in spec.Commands)
        {
            foreach (string sub in subDirs)
            {
                string dir = string.IsNullOrEmpty(sub) ? root : Path.Combine(root, sub);
                if (!string.IsNullOrEmpty(ext))
                {
                    string withExt = Path.Combine(dir, command + ext);
                    if (File.Exists(withExt))
                    {
                        return withExt;
                    }
                }
                string plain = Path.Combine(dir, command);
                if (File.Exists(plain))
                {
                    return plain;
                }
            }
        }
        return null;
    }

    private static string? FindOnPath(string command)
    {
        string finder = Environment.OSVersion.Platform == PlatformID.Win32NT ? "where" : "which";
        var (success, output) = RunCommand(finder, command);
        if (!success)
        {
            return null;
        }
        string candidate = output.Split('\n', '\r').FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Trim() ?? "";
        return File.Exists(candidate) ? candidate : null;
    }

    private static string? TryGetVersion(string command)
    {
        var (success, output) = RunCommand(command, "--version");
        if (!success || string.IsNullOrWhiteSpace(output))
        {
            return null;
        }
        foreach (string line in output.Split('\n', '\r'))
        {
            Match match = Regex.Match(line, @"(\d+\.\d+(?:\.\d+)?)");
            if (match.Success)
            {
                return match.Value;
            }
        }
        return null;
    }

    /// <summary>major.minor must agree; patch differences accepted.</summary>
    private static bool VersionsMatch(string pinned, string? system)
    {
        if (string.IsNullOrEmpty(system))
        {
            return false;
        }
        string[] p = pinned.Split('.');
        string[] s = system!.Split('.');
        if (p.Length < 2 || s.Length < 2)
        {
            return false;
        }
        return p[0] == s[0] && p[1] == s[1];
    }

    private static (bool success, string output) RunCommand(string command, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using Process? process = Process.Start(psi);
            if (process == null)
            {
                return (false, string.Empty);
            }
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(CommandTimeoutMs))
            {
                try { process.Kill(); } catch { }
                return (false, string.Empty);
            }
            string combined = (output + "\n" + error).Trim();
            return (process.ExitCode == 0 || combined.Length > 0, combined);
        }
        catch
        {
            return (false, string.Empty);
        }
    }
}
