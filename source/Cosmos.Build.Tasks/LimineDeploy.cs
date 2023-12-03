using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static Cosmos.Build.Tasks.OperatingSystem;

namespace Cosmos.Build.Tasks;

public class LimineDeploy : ToolTask
{
    [Required]
    public string IsoFile { get; set; }

    [Required]
    public string LimineIsoFile { get; set; }

    protected override string ToolName => IsWindows() ? "limine.exe" : "limine";

    protected override string GenerateFullPathToTool()
    {
        return string.IsNullOrWhiteSpace(ToolExe) ? null : Path.Combine(string.IsNullOrWhiteSpace(ToolPath) ? Directory.GetCurrentDirectory() : Path.GetFullPath(ToolPath), ToolExe);
    }

    protected override string GenerateCommandLineCommands()
    {
        CommandLineBuilder xBuilder = new();
        xBuilder.AppendSwitch($"bios-install");
        xBuilder.AppendFileNameIfNotNull($"{IsoFile}");

        return xBuilder.ToString();
    }
}