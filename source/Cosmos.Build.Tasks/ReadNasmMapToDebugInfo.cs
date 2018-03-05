using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using IL2CPU.Debug.Symbols;

namespace Cosmos.Build.Tasks
{
    public class ReadNasmMapToDebugInfo : Task
    {
        [Required]
        public string MapFile { get; set; }

        [Required]
        public string DebugInfoFile { get; set; }

        public override bool Execute()
        {
            try
            {
                var xSourceInfos = ParseMapFile(MapFile);

                if (xSourceInfos.Count == 0)
                {
                    Log.LogError("No SourceInfos found!");
                    return false;
                }

                using (var xDebugInfo = new DebugInfo(DebugInfoFile))
                {
                    xDebugInfo.AddLabels(xSourceInfos);
                    xDebugInfo.CreateIndexes();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex, true, true, null);
                return false;
            }
        }

        private static List<Label> ParseMapFile(string aMapFile)
        {
            var xSourceStrings = File.ReadAllLines(aMapFile);
            var xSource = new List<Label>();
            uint xIndex = 0;
            DebugInfo.SetRange(DebugInfo.NAsmMapExtractionRange);
            for (xIndex = 0; xIndex < xSourceStrings.Length; xIndex++)
            {
                if (xSourceStrings[xIndex].StartsWith("Real "))
                {
                    // further check it:
                    //Virtual   Name"))
                    if (!xSourceStrings[xIndex].Substring(4).TrimStart().StartsWith("Virtual ")
                        || !xSourceStrings[xIndex].EndsWith(" Name"))
                    {
                        continue;
                    }
                    xIndex++;
                    break;
                }
            }
            for (; xIndex < xSourceStrings.Length; xIndex++)
            {
                string xLine = xSourceStrings[xIndex];
                var xLineParts = xLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (xLineParts.Length == 3)
                {
                    uint xAddress = UInt32.Parse(xLineParts[0], NumberStyles.HexNumber);

                    long xId;
                    if (xLineParts[2].StartsWith("GUID_"))
                    {
                        xId = long.Parse(xLineParts[2].Substring(5));
                    }
                    else
                    {
                        xId = DebugInfo.CreateId();
                    }
                    xSource.Add(new Label()
                    {
                        ID = xId,
                        Name = xLineParts[2],
                        Address = xAddress
                    });
                }
            }
            return xSource;
        }
    }
}
