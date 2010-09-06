using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Cosmos.Debug.Common;

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
            var xSourceInfos = SourceInfo.ParseMapFile(InputBaseDir);
            if (xSourceInfos.Count == 0)
            {
                Log.LogError("No SourceInfos found!");
                return false;
            }
            SourceInfo.WriteToFile(xSourceInfos, OutputFile);
            
            return true;
        }
    }
}