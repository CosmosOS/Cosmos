using System;
using System.Diagnostics;
using System.IO;
using Cosmos.Debug.Common;
using Microsoft.Build.Framework;

namespace Cosmos.Build.MSBuild {
    public class ExtractMapFromElfFile : BaseToolTask {
        [Required]
        public string InputFile { get; set; }

        [Required]
        public string DebugInfoFile { get; set; }

        [Required]
        public string WorkingDir { get; set; }

        [Required]
        public string CosmosBuildDir { get; set; }


        public override bool Execute()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Important! A given address can have more than one label.
                // Do NOT filter by duplicate addresses as this causes serious lookup problems.
                string xFile = RunObjDump();
                
                ObjDump.ExtractMapSymbolsForElfFile(DebugInfoFile, xFile);

                return true;
            }
            catch (Exception E)
            {
                LogError("An error occurred: {0}", E.ToString());
                return false;
            }
            finally
            {
                sw.Stop();
                Log.LogMessage(MessageImportance.High, "Extracting Map file took {0}", sw.Elapsed);
            }
        }

        private string RunObjDump() {
            var xMapFile = Path.ChangeExtension(InputFile, "map");
            File.Delete(xMapFile);
            if (File.Exists(xMapFile)) {
                throw new Exception("Could not delete " + xMapFile);
            }

            var xTempBatFile = Path.Combine(WorkingDir, "ExtractElfMap.bat");
            File.WriteAllText(xTempBatFile, "@ECHO OFF\r\n\"" + Path.Combine(CosmosBuildDir, @"tools\cygwin\objdump.exe") + "\" --wide --syms \"" + InputFile + "\" > \"" + Path.GetFileName(xMapFile) + "\"");

            if (!ExecuteTool(WorkingDir, xTempBatFile, "", "objdump")) {
                throw new Exception("Error extracting map from " + InputFile);
            }
            File.Delete(xTempBatFile);

            return xMapFile;
        }
    }
}