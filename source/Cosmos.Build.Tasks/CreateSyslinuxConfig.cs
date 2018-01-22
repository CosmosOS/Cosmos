using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cosmos.Build.Tasks
{
    public class CreateSyslinuxConfig : Task
    {
        [Required]
        public string IsoDirectory { get; set; }

        [Required]
        public string BinName { get; set; }

        private string Indentation = "    ";
        
        public override bool Execute()
        {
            if (String.IsNullOrWhiteSpace(IsoDirectory) || !Directory.Exists(IsoDirectory))
            {
                Log.LogError($"Invalid ISO directory! ISO directory: '{IsoDirectory}'");
                return false;
            }

            var xBinName = BinName;
            var xLabelName = Path.GetFileNameWithoutExtension(xBinName);

            using (var xWriter = File.CreateText(Path.Combine(IsoDirectory, "syslinux.cfg")))
            {
                xWriter.WriteLine("default " + xLabelName);
                xWriter.WriteLine("label " + xLabelName);
                WriteIndentedLine(xWriter, "kernel mboot.c32");
                WriteIndentedLine(xWriter, "append " + xBinName);
            }

            return true;
        }

        private void WriteIndentedLine(TextWriter aWriter, string aText)
        {
            aWriter.WriteLine(Indentation + aText);
        }
    }
}
