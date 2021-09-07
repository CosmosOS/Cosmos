using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Cosmos.Build.Tasks
{
    public class CreateMbr : ToolTask
    {
        [Required]
        public string TargetDrive { get; set; }

        [Required]
        public bool FormatDrive { get; set; }

        protected override string ToolName => "syslinux.exe";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        protected override bool ValidateParameters()
        {
            if (!DriveInfo.GetDrives().Any(
                d => String.Equals(
                    Path.GetFullPath(d.Name),
                    Path.GetFullPath(TargetDrive),
                    StringComparison.OrdinalIgnoreCase)))
            {
                Log.LogError($"Invalid target drive '{TargetDrive}'!");
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
            var builder = new CommandLineBuilder();

            var driveLetter = TargetDrive.TrimEnd(':', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            builder.AppendSwitch("-fma");
            builder.AppendSwitch($"{driveLetter}:");

            return builder.ToString();
        }
    }
}
