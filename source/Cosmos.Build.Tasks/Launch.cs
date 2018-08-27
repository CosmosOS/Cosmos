using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Cosmos.Build.Common;
using Cosmos.Debug.Hosts;

namespace Cosmos.Build.Tasks
{
    public class Launch : Task
    {
        [Required]
        public string ConfigurationFile { get; set; }

        [Required]
        public string IsoFile { get; set; }

        public override bool Execute()
        {
            var xParams = new Dictionary<string, string>()
            {
                ["ISOFile"] = IsoFile,
                [BuildPropertyNames.EnableBochsDebugString] = "False",
                [BuildPropertyNames.StartBochsDebugGui] = "False",
                ["VisualStudioDebugPort"] = @"Pipe: Cosmos\Serial"
            };

            var xBochs = new Bochs(xParams, false, new FileInfo(ConfigurationFile));
            xBochs.Start();

            return true;
        }
    }
}
