using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;

using IL2CPU.Debug.Symbols;

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
                string xFile = RunObjDump(CosmosBuildDir, WorkingDir, InputFile, s => LogError(s), s => Log.LogMessage(s));

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

        public static string RunObjDump(string cosmosBuildDir, string workingDir, string inputFile, Action<string> errorReceived, Action<string> outputReceived) {
            var xMapFile = Path.ChangeExtension(inputFile, "map");
            File.Delete(xMapFile);
            if (File.Exists(xMapFile)) {
                throw new Exception("Could not delete " + xMapFile);
            }

            var xTempBatFile = Path.Combine(workingDir, "ExtractElfMap.bat");
            File.WriteAllText(xTempBatFile, "@ECHO OFF\r\n\"" + Path.Combine(cosmosBuildDir, @"tools\cygwin\objdump.exe") + "\" --wide --syms \"" + inputFile + "\" > \"" + Path.GetFileName(xMapFile) + "\"");

            var xResult = ExecuteTool(workingDir, xTempBatFile, "", "objdump", errorReceived, outputReceived);
            if (!xResult) {
                throw new Exception("Error extracting map from " + inputFile);
            }
            File.Delete(xTempBatFile);

            return xMapFile;
        }
    }
}
