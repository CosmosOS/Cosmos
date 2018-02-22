using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using IL2CPU.Debug.Symbols;

namespace Cosmos.Build.Tasks
{
    public class ExtractMapFromElfFile : ToolTask
    {
        [Required]
        public string InputFile { get; set; }

        [Required]
        public string MapFile { get; set; }

        [Required]
        public string DebugInfoFile { get; set; }

        protected override string ToolName => "objdump.exe";

        protected override bool ValidateParameters()
        {
            if (!File.Exists(InputFile))
            {
                Log.LogError(nameof(InputFile) + " doesn't exist!");
            }

            return !Log.HasLoggedErrors;
        }

        protected override string GenerateFullPathToTool()
        {
            if (String.IsNullOrWhiteSpace(ToolExe))
            {
                return null;
            }

            if (String.IsNullOrWhiteSpace(ToolPath))
            {
                return Path.Combine(Directory.GetCurrentDirectory(), ToolExe);
            }

            return Path.Combine(Path.GetFullPath(ToolPath), ToolExe);
        }

        protected override string GenerateCommandLineCommands()
        {
            var xBuilder = new CommandLineBuilder();

            xBuilder.AppendSwitch("--wide");
            xBuilder.AppendSwitch("--syms");

            xBuilder.AppendSwitch(">");
            xBuilder.AppendFileNameIfNotNull(MapFile);

            return xBuilder.ToString();
        }

        public override bool Execute()
        {
            if (!base.Execute())
            {
                return false;
            }

            ObjDump.ExtractMapSymbolsForElfFile(DebugInfoFile, MapFile);

            return true;
        }
    }
}
