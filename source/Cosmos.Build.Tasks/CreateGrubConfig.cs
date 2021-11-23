using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cosmos.Build.Tasks
{
    public class CreateGrubConfig: Task
    {
        [Required]
        public string TargetDirectory { get; set; }

        [Required]
        public string BinName { get; set; }

        private string Indentation = "    ";
        
        public override bool Execute()
        {
            if (!Directory.Exists(TargetDirectory))
            {
                Log.LogError($"Invalid target directory! Target directory: '{TargetDirectory}'");
                return false;
            }

            var xBinName = BinName;
            var xLabelName = Path.GetFileNameWithoutExtension(xBinName);

            using (var xWriter = File.CreateText(Path.Combine(TargetDirectory + "/boot/grub/", "grub.cfg")))
            {
                xWriter.WriteLine("insmod vbe");
                xWriter.WriteLine("insmod vga");
                xWriter.WriteLine("insmod video_bochs");
                xWriter.WriteLine("insmod video_cirrus");
                xWriter.WriteLine("set root='(hd0,msdos1)'");
                xWriter.WriteLine();
                xWriter.WriteLine("menuentry '" + xLabelName + "' {");
                WriteIndentedLine(xWriter, "multiboot /boot/" + xBinName + " vid=preset,1024,768 hdd=0");
                WriteIndentedLine(xWriter, "set gfxpayload=800x600x32");
                WriteIndentedLine(xWriter, "boot");
                xWriter.WriteLine("}");
            }

            return true;
        }

        private void WriteIndentedLine(TextWriter aWriter, string aText)
        {
            aWriter.WriteLine(Indentation + aText);
        }
    }
}
