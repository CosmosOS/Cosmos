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
                xWriter.WriteLine("menuentry '" + xLabelName + "' {");
                WriteIndentedLine(xWriter, "multiboot /boot/" + xBinName);
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
