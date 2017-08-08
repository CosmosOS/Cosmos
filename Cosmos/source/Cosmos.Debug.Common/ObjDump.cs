using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;

using Cosmos.Debug.Symbols;

namespace Cosmos.Debug.Common
{
    public class ObjDump
    {
        [Obsolete("We're not using ELF Format anymore")]
        public static SortedList<uint, string> GetLabelByAddressMapping(string aKernel, string aObjDumpExe)
        {
            string[] xSymbolsContents;
            #region Run ObjDump
            string xTempFile = Path.GetTempFileName();
            var xRandom = new Random(78367);
            string xBatFile = String.Empty;
            do
            {
                xBatFile = Path.GetTempPath();
                xBatFile = Path.Combine(xBatFile, BitConverter.GetBytes(xRandom.NextDouble()).Aggregate<byte, string>("", (r, b) => r += b.ToString("X2").ToUpper()) + ".bat");
            } while (File.Exists(xBatFile));
            string xObjDumpFile = aObjDumpExe;
            File.WriteAllText(xBatFile, String.Format("@ECHO OFF\r\n\"{0}\" --syms --wide \"{1}\" > \"{2}\"", xObjDumpFile, aKernel, xTempFile));
            using (var xProcess = Process.Start(xBatFile))
            {
                xProcess.WaitForExit();
            }
            xSymbolsContents = File.ReadAllLines(xTempFile);
            File.Delete(xTempFile);
            File.Delete(xBatFile);
            #endregion

            var xResult = new SortedList<uint, string>();
            foreach (var xLabel in ExtractMapSymbolsForElfFile(xSymbolsContents))
            {
                xResult.Add((uint)xLabel.Address, xLabel.Name);
            }
            return xResult;
        }

        /// <summary>
        /// Sequentially parse symbols from the lines sequence.
        /// </summary>
        /// <param name="lines">Sequence of lines which contain map files.</param>
        /// <returns>Sequence of parsed label.</returns>
        public static IEnumerable<Label> ExtractMapSymbolsForElfFile(IEnumerable<string> lines)
        {
            bool xListStarted = false;
            foreach (var xLine in lines)
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
                    xAddress = UInt32.Parse(xLine.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Error processing line '" + xLine + "' " + ex.Message);
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

                long xGuid;
                // See if label has an embedded GUID. If so, use it.
                if (xLabel.StartsWith("GUID_"))
                {
                    xGuid = long.Parse(xLabel.Substring(5));
                }
                else
                {
                    xGuid = DebugInfo.CreateId();
                }

                yield return new Label()
                {
                    ID = xGuid,
                    Name = xLabel,
                    Address = xAddress
                };
            }
        }

        /// <summary>
        /// Extract symbols from the specified map file and populate debug database from that file.
        /// </summary>
        /// <param name="debugFile">Debug file to popuplate with symbols.</param>
        /// <param name="mapFile">Map file from where sybols should be extracted.</param>
        public static void ExtractMapSymbolsForElfFile(string debugFile, string mapFile)
        {
            DebugInfo.SetRange(DebugInfo.ElfFileMapExtractionRange);
            using (var xDebugInfo = new DebugInfo(debugFile))
            {
                // In future instead of loading all labels, save indexes to major labels but not IL.ASM labels.
                // Then code can find major lables, and use position markers into the map file to parse in between
                // as needed.
                var xLabels = new List<Label>();
                var xFileLines = File.ReadLines(mapFile);
                foreach (var xLabel in ExtractMapSymbolsForElfFile(xFileLines))
                {
                    xLabels.Add(xLabel);
                    xDebugInfo.AddLabels(xLabels);
                }

                xDebugInfo.AddLabels(xLabels, true);
                xDebugInfo.CreateIndexes();
            }
        }
    }
}
