using System;
using System.IO;
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

    // public string LimineBackDrop { get; set; }
    // public string LimineWallpaperPath { get; set; }

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

        xWriter.WriteLineAsync(!IsNull(Timeout) ? $"TIMEOUT={Timeout}" : "TIMEOUT=0");
        xWriter.WriteLineAsync("VERBOSE=yes");
        xWriter.WriteLineAsync();

        // TODO: Add custom wallpaper system
        // xWriter.WriteLineAsync(IsNull(LimineWallpaperPath) ? $"TERM_WALLPAPER={LimineWallpaperPath}" : "TERM_WALLPAPER=boot:///boot/liminewp.bmp");
        // xWriter.WriteLineAsync(IsNull(LimineBackDrop) ? $"TERM_BACKDROP={LimineBackDrop}" : "TERM_BACKDROP=008080");
        xWriter.WriteLineAsync("TERM_WALLPAPER=boot:///boot/liminewp.bmp");
        xWriter.WriteLineAsync("INTERFACE_RESOLUTION=800x600x32");
        xWriter.WriteLineAsync();

        xWriter.WriteLineAsync($":Cosmos {xLabelName}");
        WriteIndentedLine(xWriter, $"COMMENT=Boot {xLabelName} Cosmos kernel using multiboot2");
        xWriter.WriteLineAsync();
        WriteIndentedLine(xWriter, "PROTOCOL=multiboot2");
        WriteIndentedLine(xWriter, $"KERNEL_PATH=boot:///boot/{xBinName}");
        WriteIndentedLine(xWriter, "KERNEL_CMDLINE= Cosmos Kernel");

        if (Modules == null) return true;

        foreach (var module in Modules)
        {
            WriteIndentedLine(xWriter, $"MODULE_PATH=boot:///{module}");
            // WriteIndentedLine(xWriter, $"MODULE_STRING=This is {module} module");
        }

        xWriter.Flush();

        return true;
    }

    private void WriteIndentedLine(TextWriter aWriter, string aText)
    {
        aWriter.WriteLineAsync(Indentation + aText);
    }

    private bool IsNull(string s)
    {
        return String.IsNullOrWhiteSpace(s) && String.IsNullOrEmpty(s);
    }
}
