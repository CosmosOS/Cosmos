using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static Cosmos.Build.Tasks.OperatingSystem;

namespace Cosmos.Build.Tasks
{
    public class TheRingMaster : ToolTask
    {
        [Required]
        public string KernelAssemblyPath { get; set; }

        protected override string ToolName => IsWindows() ? "TheRingMaster.exe" : "TheRingMaster";

        protected override MessageImportance StandardErrorLoggingImportance => MessageImportance.High;
        protected override MessageImportance StandardOutputLoggingImportance => MessageImportance.High;

        protected override bool ValidateParameters()
        {
            if (!File.Exists(KernelAssemblyPath))
            {
                Log.LogError(nameof(KernelAssemblyPath) + " doesn't exist!");
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
            return KernelAssemblyPath;
        }
    }
}
