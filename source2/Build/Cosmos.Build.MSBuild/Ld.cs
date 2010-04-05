using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;

namespace Cosmos.Build.MSBuild
{
    public class Ld: BaseToolTask
    {
        [Required]
        public string CosmosBuildDir
        {
            get;
            set;
        }

        [Required]
        public string WorkingDir
        {
            get;
            set;
        }

        [Required]
        public string Arguments
        {
            get;
            set;
        }

        public override bool Execute()
        {
            return base.ExecuteTool(WorkingDir, 
                Path.Combine(CosmosBuildDir, @"tools\cygwin\ld.exe"),
                Arguments.Replace('\\', '/'),
                "ld");
        }
    }
}
