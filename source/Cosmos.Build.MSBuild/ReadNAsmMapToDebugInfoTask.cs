using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Cosmos.Debug.Common;

namespace Cosmos.Build.MSBuild
{
    public class ReadNAsmMapToDebugInfoTask
    {
        public string InputBaseDir;
        public string DebugInfoFile;
        public Action<string> LogError;
        public Action<string> LogMessage;

        public bool Execute()
        {
            var xSourceInfos = ReadNAsmMapToDebugInfo.ParseMapFile(InputBaseDir);
            if (xSourceInfos.Count == 0)
            {
                LogError("No SourceInfos found!");
                return false;
            }
            using (var xDebugInfo = new DebugInfo(DebugInfoFile))
            {
                xDebugInfo.AddLabels(xSourceInfos);
                xDebugInfo.CreateIndexes();
            }
            return true;
        }
    }
}
