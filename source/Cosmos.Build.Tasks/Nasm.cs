using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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

        #endregion

        private OutputFormatEnum mOutputFormat;

        protected override string ToolName => "nasm.exe";

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

            xBuilder.AppendSwitch("-g");

            xBuilder.AppendSwitchIfNotNull("-f ", OutputFormat);
            xBuilder.AppendSwitchIfNotNull("-o ", OutputFile);

            if (mOutputFormat == OutputFormatEnum.ELF)
            {
                xBuilder.AppendSwitch("-dELF_COMPILATION");
            }
            else
            {
                xBuilder.AppendSwitch("-dBIN_COMPILATION");
            }

            xBuilder.AppendSwitch("-O0");

            xBuilder.AppendFileNameIfNotNull(InputFile);

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
                Log.LogMessage(MessageImportance.High, "Nasm task took {0}", xSW.Elapsed);
            }
        }
    }
}
