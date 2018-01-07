using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Build.Framework;

using IL2CPU.Debug.Symbols;

namespace Cosmos.Build.MSBuild
{
    public class ReadNAsmMapToDebugInfo : BaseToolTask
    {
        [Required]
        public string InputBaseDir { get; set; }

        [Required]
        public string DebugInfoFile { get; set; }

        public override bool Execute()
        {
            var xSW = new Stopwatch();
            xSW.Start();
            try
            {
                var xSourceInfos = ParseMapFile(InputBaseDir);
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
            finally
            {
                xSW.Stop();
                Log.LogMessage(MessageImportance.High, "ReadNAsmMapToDebugInfo took {0}", xSW.Elapsed);
            }
        }

        private static List<Label> ParseMapFile(string inputBaseDir)
        {
            var xSourceStrings = File.ReadAllLines(Path.Combine(inputBaseDir, "main.map"));
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
