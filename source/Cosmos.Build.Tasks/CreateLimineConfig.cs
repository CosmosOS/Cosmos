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

    public string LimineBackDrop { get; set; }

    public string LimineWallpaperPath { get; set; }

    public override bool Execute()
    {
        if (!Directory.Exists(TargetDirectory))
        {
            Log.LogError($"Invalid target directory! Target directory: '{TargetDirectory}'");
            return false;
        }

        var xBinName = BinName;
        var xLabelName = Path.GetFileNameWithoutExtension(xBinName);
        using var xWriter = File.CreateText(Path.Combine($"{TargetDirectory}boot/limine/", "limine.cfg"));

        xWriter.WriteLine(!IsNull(Timeout) ? $"TIMEOUT={Timeout}" : "TIMEOUT=0");
        xWriter.WriteLine("DEFAULT_ENTRY=3");
        xWriter.WriteLine("VERBOSE=yes");
        xWriter.WriteLine();
        xWriter.WriteLine(IsNull(LimineWallpaperPath) ? $"TERM_WALLPAPER={LimineWallpaperPath}" : "");
        xWriter.WriteLine(IsNull(LimineBackDrop) ? $"TERM_BACKDROP={LimineBackDrop}" : "TERM_BACKDROP=008080");

        xWriter.WriteLine($":Multiboot2 {xLabelName}");
        WriteIndentedLine(xWriter, $"COMMENT={xLabelName} Cosmos kernel");
        xWriter.WriteLine();
        WriteIndentedLine(xWriter, "PROTOCOL=multiboot2");
        WriteIndentedLine(xWriter, $"KERNEL_PATH=boot:///{xBinName}");
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
        aWriter.WriteLine(Indentation + aText);
    }

    private bool IsNull(string s)
    {
        return String.IsNullOrWhiteSpace(s) && String.IsNullOrEmpty(s);
    }
}
