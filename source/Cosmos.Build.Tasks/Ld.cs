using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static Cosmos.Build.Tasks.OperatingSystem;

namespace Cosmos.Build.Tasks
{
    public class Ld : ToolTask
    {
        #region Task Parameters

        [Required]
        public ITaskItem[] InputFiles { get; set; }

        [Required]
        public string OutputFile { get; set; }

        public string Entry { get; set; }

        public string TextAddress { get; set; }

        public string DataAddress { get; set; }

        public string BssAddress { get; set; }

        #endregion

        protected override string ToolName => IsWindows() ? "ld.exe" : "ld";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        private static bool IsValidAddress(string aAddress)
        {
            if (ulong.TryParse(aAddress, out var xAddress))
            {
                return true;
            }

            if (aAddress.StartsWith("0x")
                && ulong.TryParse(aAddress.Remove(0, 2), NumberStyles.AllowHexSpecifier, null, out xAddress))
            {
                return true;
            }

            return false;
        }

        protected override bool ValidateParameters()
        {
            if (InputFiles.Length == 0)
            {
                Log.LogError("No input files specified!");
            }


            foreach (var xFile in InputFiles)
            {
                var xFullPath = xFile.GetMetadata("FullPath");

                if (string.IsNullOrWhiteSpace(xFullPath))
                {
                    Log.LogError($"Input file is an empty string! Input files: '{string.Join(";", InputFiles.Select(f => f.GetMetadata("Identity")))}'");
                }
                else if (!File.Exists(xFullPath))
                {
                    Log.LogError($"Input file '{xFullPath}' doesn't exist!");
                }
            }

            if (string.IsNullOrEmpty(OutputFile))
            {
                Log.LogError("No output file specified!");
            }

            if (string.IsNullOrWhiteSpace(Entry))
            {
                Entry = null;
            }

            if (string.IsNullOrWhiteSpace(TextAddress))
            {
                TextAddress = null;
            }
            else if (!IsValidAddress(TextAddress))
            {
                Log.LogError(nameof(TextAddress) + " isn't a valid 64-bit number!");
            }

            if (string.IsNullOrWhiteSpace(DataAddress))
            {
                DataAddress = null;
            }
            else if (!IsValidAddress(DataAddress))
            {
                Log.LogError(nameof(DataAddress) + " isn't a valid 64-bit number!");
            }

            if (string.IsNullOrWhiteSpace(BssAddress))
            {
                BssAddress = null;
            }
            else if (!IsValidAddress(BssAddress))
            {
                Log.LogError(nameof(BssAddress) + " isn't a valid 64-bit number!");
            }

            return !Log.HasLoggedErrors;
        }

        protected override string GenerateFullPathToTool()
        {
            if (string.IsNullOrWhiteSpace(ToolExe))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(ToolPath))
            {
                return Path.Combine(Directory.GetCurrentDirectory(), ToolExe);
            }

            return Path.Combine(Path.GetFullPath(ToolPath), ToolExe);
        }

        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder xBuilder = new();

            xBuilder.AppendSwitchIfNotNull("-Ttext ", TextAddress);
            xBuilder.AppendSwitchIfNotNull("-Tdata ", DataAddress);
            xBuilder.AppendSwitchIfNotNull("-Tbss ", BssAddress);
            xBuilder.AppendSwitchIfNotNull("-e ", Entry);
            xBuilder.AppendSwitchIfNotNull("-o ", OutputFile);

            xBuilder.AppendFileNamesIfNotNull(InputFiles, " ");
            xBuilder.AppendSwitch("-m elf_x86_64");

            Log.LogMessage(MessageImportance.High, xBuilder.ToString());
            
            return xBuilder.ToString();
        }

        public override bool Execute()
        {
            Stopwatch xSW = Stopwatch.StartNew();
            try
            {
                return base.Execute();
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, "LD task took {0}", xSW.Elapsed);
            }
        }

    }
}
