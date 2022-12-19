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

    protected override string ToolName => IsWindows() ? "limine-deploy.exe" : "limine-deploy";

    protected override string GenerateFullPathToTool()
    {
        return String.IsNullOrWhiteSpace(ToolExe) ? null : Path.Combine(String.IsNullOrWhiteSpace(ToolPath) ? Directory.GetCurrentDirectory() : Path.GetFullPath(ToolPath), ToolExe);
    }

    protected override string GenerateCommandLineCommands()
    {
        var xBuilder = new CommandLineBuilder();
        xBuilder.AppendSwitch($"{IsoFile}");

        return xBuilder.ToString();
    }
}
