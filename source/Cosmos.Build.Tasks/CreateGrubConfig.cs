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

        public string[] Modules { get; set; }

        private string Indentation = "    ";

        public string timeout { get; set; }
        
        public override bool Execute()
        {
            if (!Directory.Exists(TargetDirectory))
            {
                Log.LogError($"Invalid target directory! Target directory: '{TargetDirectory}'");
                return false;
            }

            var xBinName = BinName;
            var xLabelName = Path.GetFileNameWithoutExtension(xBinName).remove;

            using (var xWriter = File.CreateText(Path.Combine(TargetDirectory + "/boot/grub/", "grub.cfg")))
            {
                if (!String.IsNullOrWhiteSpace(timeout) && !String.IsNullOrEmpty(timeout))
                {
                    xWriter.WriteLine("set timeout=" + timeout);
                }
                else
                {
                    xWriter.WriteLine("set timeout=0");
                }
                if (Modules != null)
                {
                    foreach (var module in Modules)
                    {
                        xWriter.WriteLine($"insmod {module}");
                    }
                }

                xWriter.WriteLine();
                xWriter.WriteLine("menuentry '" + xLabelName + "' {");
                WriteIndentedLine(xWriter, "multiboot2 /boot/" + xBinName);
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
