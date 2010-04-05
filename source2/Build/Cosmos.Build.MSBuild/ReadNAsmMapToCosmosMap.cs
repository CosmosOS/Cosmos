using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Cosmos.Debug.Common.CDebugger;

namespace Cosmos.Build.MSBuild
{
    public class ReadNAsmMapToCosmosMap : Task
    {
        [Required]
        public string InputBaseDir
        {
            get;
            set;
        }

        [Required]
        public string OutputFile
        {
            get;
            set;
        }

        public override bool Execute()
        {
            SourceInfo.WriteToFile(SourceInfo.ParseMapFile(InputBaseDir), OutputFile);
            return true;
        }
    }
}