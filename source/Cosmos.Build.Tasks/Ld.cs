using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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

        protected override string ToolName => "ld.exe";
        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;

        private static bool IsValidAddress(string aAddress)
        {
            if (UInt64.TryParse(aAddress, out var xAddress))
            {
                return true;
            }

            if (aAddress.StartsWith("0x")
                && UInt64.TryParse(aAddress.Remove(0, 2), NumberStyles.AllowHexSpecifier, null, out xAddress))
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

                if (String.IsNullOrWhiteSpace(xFullPath))
                {
                    Log.LogError($"Input file is an empty string! Input files: '{String.Join(";", InputFiles.Select(f => f.GetMetadata("Identity")))}'");
                }
                else if (!File.Exists(xFullPath))
                {
                    Log.LogError($"Input file '{xFullPath}' doesn't exist!");
                }
            }

            if (String.IsNullOrEmpty(OutputFile))
            {
                Log.LogError("No output file specified!");
            }

            if (String.IsNullOrWhiteSpace(Entry))
            {
                Entry = null;
            }

            if (String.IsNullOrWhiteSpace(TextAddress))
            {
                TextAddress = null;
            }
            else if (!IsValidAddress(TextAddress))
            {
                Log.LogError(nameof(TextAddress) + " isn't a valid 64-bit number!");
            }

            if (String.IsNullOrWhiteSpace(DataAddress))
            {
                DataAddress = null;
            }
            else if (!IsValidAddress(DataAddress))
            {
                Log.LogError(nameof(DataAddress) + " isn't a valid 64-bit number!");
            }

            if (String.IsNullOrWhiteSpace(BssAddress))
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

            xBuilder.AppendSwitchIfNotNull("-Ttext ", TextAddress);
            xBuilder.AppendSwitchIfNotNull("-Tdata ", DataAddress);
            xBuilder.AppendSwitchIfNotNull("-Tbss ", BssAddress);
            xBuilder.AppendSwitchIfNotNull("-e ", Entry);
            xBuilder.AppendSwitchIfNotNull("-o ", OutputFile);

            xBuilder.AppendFileNamesIfNotNull(InputFiles, " ");

            Log.LogMessage(MessageImportance.High, xBuilder.ToString());

            return xBuilder.ToString();
        }
    }
}
