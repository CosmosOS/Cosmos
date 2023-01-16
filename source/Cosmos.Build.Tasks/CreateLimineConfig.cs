using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cosmos.Build.Tasks;

public class CreateLimineConfig : Task
{
    [Required]
    public string TargetDirectory { get; set; }

    [Required]
    public string BinName { get; set; }

    public string[] Modules { get; set; }

    private string Indentation = "    ";

    public string Timeout { get; set; }

    public override bool Execute()
    {
        if (!Directory.Exists(TargetDirectory))
        {
            Log.LogError($"Invalid target directory! Target directory: '{TargetDirectory}'");
            return false;
        }

        var xBinName = BinName;
        var xLabelName = Path.GetFileNameWithoutExtension(xBinName);
        using var xWriter = File.CreateText(Path.Combine($"{TargetDirectory}boot/", "limine.cfg"));

        xWriter.WriteLineAsync(!String.IsNullOrEmpty(Timeout) ? $"TIMEOUT={Timeout}" : "TIMEOUT=0");
        xWriter.WriteLineAsync("VERBOSE=yes");
        xWriter.WriteLineAsync();

        // TODO: Add custom wallpaper system
        xWriter.WriteLineAsync("TERM_WALLPAPER=boot:///boot/liminewp.bmp");
        xWriter.WriteLineAsync("INTERFACE_RESOLUTION=800x600x32");
        xWriter.WriteLineAsync();

        xWriter.WriteLineAsync($":Cosmos {xLabelName}");
        WriteIndentedLine(xWriter, $"COMMENT=Boot {xLabelName} Cosmos kernel using multiboot2");
        xWriter.WriteLineAsync();
        WriteIndentedLine(xWriter, "PROTOCOL=multiboot2");
        WriteIndentedLine(xWriter,
            xBinName.EndsWith(".gz")
                ? $"KERNEL_PATH=$boot:///boot/{xBinName}"
                : $"KERNEL_PATH=boot:///boot/{xBinName}");

        if (Modules == null) return true;

        foreach (var module in Modules)
        {
            WriteIndentedLine(xWriter, $"MODULE_PATH=boot:///{module}");
        }

        xWriter.Flush();

        return true;
    }

    private void WriteIndentedLine(TextWriter aWriter, string aText)
    {
        aWriter.WriteLineAsync(Indentation + aText);
    }
}
