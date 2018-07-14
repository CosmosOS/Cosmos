using System;
using System.Diagnostics;
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

        protected override string ToolName => "objdump.bat";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

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

            string xPathToTool = Path.GetDirectoryName(GenerateFullPathToTool());

            xBuilder.AppendFileNameIfNotNull(xPathToTool);

            xBuilder.AppendFileNameIfNotNull(InputFile);

            xBuilder.AppendFileNameIfNotNull(MapFile);
            
            return xBuilder.ToString();
        }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "Extracting Map file...");

            var xSW = Stopwatch.StartNew();
            try
            {
                if (!base.Execute())
                {
                    return false;
                }

                ObjDump.ExtractMapSymbolsForElfFile(DebugInfoFile, MapFile);

                return true;
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, "Extracting Map file took {0}", xSW.Elapsed);
            }
        }
    }
}
