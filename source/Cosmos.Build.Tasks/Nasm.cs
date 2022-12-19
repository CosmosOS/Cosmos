using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static Cosmos.Build.Tasks.OperatingSystem;

namespace Cosmos.Build.Tasks
{
    public class Nasm : ToolTask
    {
        enum OutputFormatEnum
        {
            Bin,
            ELF
        }

        #region Task Parameters
        
        [Required]
        public string InputFile { get; set; }

        [Required]
        public string OutputFile { get; set; }

        [Required]
        public string OutputFormat
        {
            get => mOutputFormat.ToString();
            set => mOutputFormat = (OutputFormatEnum)Enum.Parse(typeof(OutputFormatEnum), value, true);
        }

        public string OptimizationLevel { get; set; }

        #endregion

        private OutputFormatEnum mOutputFormat;

        protected override string ToolName => IsWindows() ? "yasm.exe" : "yasm";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        protected override bool ValidateParameters()
        {
            if (String.IsNullOrWhiteSpace(InputFile))
            {
                Log.LogError("No input file specified!");
            }
            else if (!File.Exists(InputFile))
            {
                Log.LogError($"Input file '{InputFile}' doesn't exist!");
            }

            if (String.IsNullOrWhiteSpace(OutputFile))
            {
                Log.LogError($"No output file specified!");
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

            if (mOutputFormat == OutputFormatEnum.ELF)
            {
                xBuilder.AppendSwitch("-g dwarf2");
            }
            else
            {
                xBuilder.AppendSwitch("-g null");
            }

            xBuilder.AppendSwitchIfNotNull("-f ", OutputFormat.ToLower());
            xBuilder.AppendSwitchIfNotNull("-o ", OutputFile);

            if (mOutputFormat == OutputFormatEnum.ELF)
            {
                xBuilder.AppendSwitch("-dELF_COMPILATION");
            }
            else
            {
                xBuilder.AppendSwitch("-dBIN_COMPILATION");
            }

            /* Apply the optimization level that the user chose */
	    if(!String.IsNullOrWhiteSpace(OptimizationLevel) && !String.IsNullOrWhiteSpace(OptimizationLevel))
            	xBuilder.AppendSwitch($"-O{OptimizationLevel}");

            xBuilder.AppendFileNameIfNotNull(InputFile);

            Log.LogMessage(MessageImportance.High, xBuilder.ToString());

            return xBuilder.ToString();
        }

        public override bool Execute()
        {
            var xSW = Stopwatch.StartNew();

            try
            {
                return base.Execute();
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, "Yasm task took {0}", xSW.Elapsed);
            }
        }
    }
}
