using System;
using System.Collections.Generic;
using Globalization = System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using Microsoft.Build.Utilities;
using System.Diagnostics;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Data.SQLite;
using Cosmos.Debug.Common;

namespace Cosmos.Build.MSBuild {
    public class ExtractMapFromElfFile : BaseToolTask {
        [Required]
        public string InputFile { get; set; }

        [Required]
        public string DebugInfoFile { get; set; }

        [Required]
        public string WorkingDir { get; set; }

        [Required]
        public string CosmosBuildDir { get; set; }

        #region Old NASM Map parser
        //public static List<Label> ParseMapFile(String buildPath) {
        //  var xSourceStrings = File.ReadAllLines(Path.Combine(buildPath, "main.map"));
        //  var xSource = new List<Label>();
        //  uint xIndex = 0;
        //  for (xIndex = 0; xIndex < xSourceStrings.Length; xIndex++) {
        //    if (xSourceStrings[xIndex].StartsWith("Real ")) {
        //      // further check it:
        //      //Virtual   Name"))
        //      if (!xSourceStrings[xIndex].Substring(4).TrimStart().StartsWith("Virtual ")
        //          || !xSourceStrings[xIndex].EndsWith(" Name")) {
        //        continue;
        //      }
        //      xIndex++;
        //      break;
        //    }
        //  }
        //  for (; xIndex < xSourceStrings.Length; xIndex++) {
        //    string xLine = xSourceStrings[xIndex];
        //    var xLineParts = xLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        //    if (xLineParts.Length == 3) {
        //      uint xAddress = UInt32.Parse(xLineParts[0], System.Globalization.NumberStyles.HexNumber);
        //      xSource.Add(new Label() {
        //        LABELNAME = xLineParts[2],
        //        ADDRESS = xAddress
        //      });
        //    }
        //  }
        //  return xSource;
        //}
        #endregion

        public override bool Execute()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                // Important! A given address can have more than one label.
                // Do NOT filter by duplicate addresses as this causes serious lookup problems.
                string xFile = RunObjDump();

                using (var xDebugInfo = new DebugInfo(DebugInfoFile))
                {
                    // In future instead of loading all labels, save indexes to major labels but not IL.ASM labels.
                    // Then code can find major lables, and use position markers into the map file to parse in between 
                    // as needed.
                    using (var xMapReader = new StreamReader(xFile))
                    {
                        var xLabels = new List<Label>();
                        bool xListStarted = false;
                        string xLine;
                        while ((xLine = xMapReader.ReadLine()) != null)
                        {
                            if (String.IsNullOrEmpty(xLine))
                            {
                                continue;
                            }
                            else if (!xListStarted)
                            {
                                // Find start of the data
                                if (xLine == "SYMBOL TABLE:")
                                {
                                    xListStarted = true;
                                }
                                continue;
                            }

                            uint xAddress;
                            try
                            {
                                xAddress = UInt32.Parse(xLine.Substring(0, 8), Globalization.NumberStyles.HexNumber);
                            }
                            catch (Exception ex)
                            {
                                LogError("Error processing line '" + xLine + "' " + ex.Message);
                                throw;
                            }

                            string xSection = xLine.Substring(17, 5);
                            if (xSection != ".text" && xSection != ".data")
                            {
                                continue;
                            }
                            string xLabel = xLine.Substring(32);
                            if (xLabel == xSection)
                            {
                                // Non label, skip
                                continue;
                            }

                            Guid xGuid;
                            // See if label has an embedded GUID. If so, use it.
                            if (xLabel.StartsWith("GUID_"))
                            {
                                xGuid = new Guid(xLabel.Substring(5));
                            }
                            else
                            {
                                xGuid = Guid_NewGuid();
                            }

                            xLabels.Add(new Label()
                            {
                                ID = xGuid,
                                Name = xLabel,
                                Address = xAddress
                            });
                            xDebugInfo.AddLabels(xLabels);
                        }
                        xDebugInfo.AddLabels(xLabels, true);
                    }
                }

                return true;
            }
            catch (Exception E)
            {
                LogError("An error occurred: {0}", E.ToString());
                return false;
            }
            finally
            {
                sw.Stop();
                Log.LogMessage(MessageImportance.High, "Extracting Map file took {0}", sw.Elapsed);
            }
        }

        private string RunObjDump() {
            var xMapFile = Path.ChangeExtension(InputFile, "map");
            File.Delete(xMapFile);
            if (File.Exists(xMapFile)) {
                throw new Exception("Could not delete " + xMapFile);
            }

            var xTempBatFile = Path.Combine(WorkingDir, "ExtractElfMap.bat");
            File.WriteAllText(xTempBatFile, "@ECHO OFF\r\n\"" + Path.Combine(CosmosBuildDir, @"tools\cygwin\objdump.exe") + "\" --wide --syms \"" + InputFile + "\" > \"" + Path.GetFileName(xMapFile) + "\"");

            if (!ExecuteTool(WorkingDir, xTempBatFile, "", "objdump")) {
                throw new Exception("Error extracting map from " + InputFile);
            }
            File.Delete(xTempBatFile);

            return xMapFile;
        }
        private static UInt64 mLastGuid = 0;
        private static Guid Guid_NewGuid() {
            // Old code: 
            //return Guid.NewGuid();
            // Do NOT use Guid.NewGuid(). During compilation we're generating
            // about 60 milion guids. Guid.NewGuid slows down compilation significantly (about half the time!)

            mLastGuid++;
            var bytes = new byte[16];
            bytes[0] = (byte)(mLastGuid >> 56);
            bytes[1] = (byte)(mLastGuid >> 48);
            bytes[2] = (byte)(mLastGuid >> 40);
            bytes[3] = (byte)(mLastGuid >> 32);
            bytes[4] = (byte)(mLastGuid >> 24);
            bytes[5] = (byte)(mLastGuid >> 16);
            bytes[6] = (byte)(mLastGuid >> 8);
            bytes[7] = (byte)(mLastGuid);
            //Adds an additional element to help prevent collisions between this function and it's equivelant in
            //Cosmos.Debug.Common.DebugInfo
            bytes[15] = (byte)(1);

            var r = new Guid(bytes);
            return r;
        }
    }
}