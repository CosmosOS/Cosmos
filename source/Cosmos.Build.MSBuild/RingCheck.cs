using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;

using Cosmos.Build.Common;

namespace Cosmos.Build.MSBuild
{
    public class RingCheck : BaseToolTask
    {
        [Required]
        public string KernelAssemblyPath { get; set; }

        [Required]
        public string WorkingDir { get; set; }

        private List<string> mAssemblySearchPaths;

        public override bool Execute()
        {
            return ExecuteTool(WorkingDir, Path.Combine(CosmosPaths.Build, "RingCheck", "RingCheck.exe"), KernelAssemblyPath, "Ring Check");
        }
    }
}
