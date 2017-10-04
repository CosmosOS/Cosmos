using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;

using Cosmos.Build.Common;

namespace Cosmos.Build.MSBuild
{
    public class TheRingMaster : BaseToolTask
    {
        [Required]
        public string KernelAssemblyPath { get; set; }

        [Required]
        public string WorkingDir { get; set; }

        public override bool Execute()
        {
            var xSW = Stopwatch.StartNew();

            try
            {
                Log.LogMessage(MessageImportance.High, $"Invoking TheRingMaster.exe {KernelAssemblyPath}");
                return ExecuteTool(WorkingDir, Path.Combine(CosmosPaths.Build, "TheRingMaster", "TheRingMaster.exe"), KernelAssemblyPath, "The Ring Master");
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, $"TheRingMaster invoked with KernelAssemblyPath = '{KernelAssemblyPath}'");
                Log.LogMessage(MessageImportance.High, "TheRingMaster task took {0}", xSW.Elapsed);
            }
        }
    }
}
