using System.Diagnostics;
using System.IO;
using Microsoft.Build.Framework;

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
            var xSW = Stopwatch.StartNew();
            try
            {
                return base.ExecuteTool(WorkingDir,
                   Path.Combine(CosmosBuildDir, @"tools\cygwin\ld.exe"),
                   Arguments.Replace('\\', '/'),
                   "ld");
            }
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, "Ld task took {0}", xSW.Elapsed);
            }
        }
    }
}
