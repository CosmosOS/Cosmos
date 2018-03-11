using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cosmos.Build.Tasks
{
    public class MakeIso : ToolTask
    {
        [Required]
        public string IsoDirectory { get; set; }

        [Required]
        public string OutputFile { get; set; }

        protected override string ToolName => "mkisofs.exe";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        protected override bool ValidateParameters()
        {
            if (String.IsNullOrEmpty(OutputFile))
            {
                Log.LogError(nameof(OutputFile) + " is null or empty!");
            }

            try
            {
                Path.GetFullPath(OutputFile);
            }
            catch
            {
                Log.LogError($"{nameof(OutputFile)} is an invalid path! {nameof(OutputFile)}: '{OutputFile}'");
            }

            if (String.IsNullOrEmpty(IsoDirectory))
            {
                Log.LogError(nameof(IsoDirectory) + " is null or empty!");
            }

            try
            {
                Path.GetFullPath(IsoDirectory);
            }
            catch
            {
                Log.LogError($"{nameof(IsoDirectory)} is an invalid path! {nameof(IsoDirectory)}: '{IsoDirectory}'");
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

            xBuilder.AppendSwitch("-relaxed-filenames");
            xBuilder.AppendSwitch("-J");
            xBuilder.AppendSwitch("-R");
            xBuilder.AppendSwitchIfNotNull("-o ", OutputFile);
            xBuilder.AppendSwitch("-b isolinux.bin");
            xBuilder.AppendSwitch("-no-emul-boot");
            xBuilder.AppendSwitch("-boot-load-size 4");
            xBuilder.AppendSwitch("-boot-info-table");
            xBuilder.AppendFileNameIfNotNull(IsoDirectory.TrimEnd('\\', '/'));

            Log.LogMessage(MessageImportance.High, xBuilder.ToString());

            return xBuilder.ToString();
        }
    }
}
