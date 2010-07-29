using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Diagnostics;

namespace Cosmos.Debug.Common.CDebugger
{
	public class SourceInfos: SortedList<uint, SourceInfo> {
		public SourceInfo GetMapping(uint aValue) {
			for (int i = Count - 1; i >=0; i--) {
				if (Keys[i] <= aValue) {
					return Values[i];
				}
			}
			return null;
		}
	}
	public class SourceInfo {
        public string MethodName
        {
            get;
            set;
        }

		public string SourceFile {
			get;
			set;
		}
		public int Line {
			get;
			set;
		}
		public int Column {
			get;
			set;
		}
		public int LineEnd {
			get;
			set;
		}
		public int ColumnEnd {
			get;
			set;
		}

        public static void WriteToFile(SortedList<uint, String> aMap, string outFile)
        {
            using (var xOut = new StreamWriter(outFile, false))
            {
                foreach (var xItem in aMap)
                {
                    xOut.WriteLine("{0}\t{1}", xItem.Key.ToString("X8"), xItem.Value);
                }
            }
        }

        public static void ReadFromFile(string aFile, out IDictionary<uint, string> oAddressLabelMappings, out IDictionary<string, uint> oLabelAddressMappings)
        {
            oAddressLabelMappings = new Dictionary<uint, string>();
            oLabelAddressMappings = new Dictionary<string, uint>();
            foreach (var xLine in File.ReadAllLines(aFile))
            {
                if (!xLine.Contains('\t'))
                {
                    continue;
                }
                var xPart1 = xLine.Substring(0, xLine.IndexOf('\t'));
                var xPart2 = xLine.Substring(xLine.IndexOf('\t') + 1);
                var xAddr = UInt32.Parse(xPart1, NumberStyles.HexNumber);
                oAddressLabelMappings.Add(xAddr, xPart2);
                oLabelAddressMappings.Add(xPart2, xAddr);
            }
        }

        public static SortedList<uint,String> ParseMapFile(String buildPath)
        {
            var xSourceStrings = File.ReadAllLines(Path.Combine(buildPath, "main.map"));
            SortedList<uint,String> xSource = new SortedList<uint,String>();
            uint xIndex = 0;
            for (xIndex = 0; xIndex < xSourceStrings.Length; xIndex++)
            {
                if (xSourceStrings[xIndex].StartsWith("Real "))
                {
                    // further check it:
                    //Virtual   Name"))
                    if (!xSourceStrings[xIndex].Substring(4).TrimStart().StartsWith("Virtual ")
                        ||!xSourceStrings[xIndex].EndsWith(" Name"))
                    {
                        continue;
                    }
                    xIndex++;
                    break;
                }
            }
            for (; xIndex < xSourceStrings.Length; xIndex++)
            {
                var xLine = xSourceStrings[xIndex];//.Trim();
                var xLineParts = xLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (xLineParts.Length == 3)
                {
                    uint xAddress = UInt32.Parse(xLineParts[0], System.Globalization.NumberStyles.HexNumber);
                    if (!xSource.ContainsKey(xAddress))
                        xSource.Add(xAddress, xLineParts[2]);
                    else continue;
                }
            }
            return xSource;
        }
		public static int GetIndexClosestSmallerMatch(IList<int> aList, int aValue) {
			int xIdx = -1;
			for (int i = 0; i < aList.Count; i++) {
				if (aList[i] <= aValue) {
					xIdx = i;
				} else {
					break;
				}
			}
			return xIdx;
		}

		public static SourceInfos GetSourceInfo(IDictionary<uint, string> aAddressLabelMappings, IDictionary<string, uint> aLabelAddressMappings, string aDebugFile) {
			var xSymbolsList = new List<MLDebugSymbol>();
            var xSW = new Stopwatch();
            xSW.Start();
			MLDebugSymbol.ReadSymbolsListFromFile(xSymbolsList, aDebugFile);
            xSW.Stop();
            System.Diagnostics.Trace.WriteLine("Loading SymbolsList took: " + xSW.Elapsed);
            xSW.Reset();
            xSW.Start();
            #region sort
            xSymbolsList.Sort(delegate(MLDebugSymbol a, MLDebugSymbol b) {
				if (a == null) {
					throw new ArgumentNullException("a");
				}
				if (b == null) {
					throw new ArgumentNullException("b");
				}
				int xCompareResult = StringComparer.InvariantCultureIgnoreCase.Compare(a.AssemblyFile, b.AssemblyFile);
				if (xCompareResult == 0) {
					xCompareResult = a.TypeToken.CompareTo(b.TypeToken);
					if (xCompareResult == 0) {
						xCompareResult = a.MethodToken.CompareTo(b.MethodToken);
						if (xCompareResult == 0) {
							return a.ILOffset.CompareTo(b.ILOffset);
						}

					}
				}
				return xCompareResult;
			});
            #endregion
            xSW.Stop();
            System.Diagnostics.Trace.WriteLine("Sorting SymbolsList took: " + xSW.Elapsed);
            //MLDebugSymbol.WriteSymbolsListToFile(xSymbolsList, aDebugFile + ".sorted");
            xSW.Reset();
            xSW.Start();
            var xResult = new SourceInfos();
			string xOldAssembly = null;
			ISymbolReader xSymbolReader = null;
			int[] xCodeOffsets = null;
			ISymbolDocument[] xCodeDocuments = null;
			int[] xCodeLines = null;
			int[] xCodeColumns = null;
			int[] xCodeEndLines = null;
			int[] xCodeEndColumns = null;
			int? xOldMethodToken = null;
			ISymbolMethod xMethodSymbol = null;
			foreach (var xSymbol in xSymbolsList) {
                if (!xSymbol.AssemblyFile.Equals(xOldAssembly, StringComparison.InvariantCultureIgnoreCase)) {
					try {
                        xMethodSymbol = null;
						xSymbolReader = SymbolAccess.GetReaderForFile(xSymbol.AssemblyFile);
					} catch {
						xSymbolReader = null;
						xMethodSymbol = null;
					}
					xOldAssembly = xSymbol.AssemblyFile;
				}
				if (xOldMethodToken != xSymbol.MethodToken) {
					if (xSymbolReader != null) {
						try {
							xMethodSymbol = xSymbolReader.GetMethod(new SymbolToken(xSymbol.MethodToken));
							if (xMethodSymbol != null) {
								xCodeOffsets = new int[xMethodSymbol.SequencePointCount];
								xCodeDocuments = new ISymbolDocument[xMethodSymbol.SequencePointCount];
								xCodeLines = new int[xMethodSymbol.SequencePointCount];
								xCodeColumns = new int[xMethodSymbol.SequencePointCount];
								xCodeEndLines = new int[xMethodSymbol.SequencePointCount];
								xCodeEndColumns = new int[xMethodSymbol.SequencePointCount];
								xMethodSymbol.GetSequencePoints(xCodeOffsets, xCodeDocuments, xCodeLines, xCodeColumns, xCodeEndLines, xCodeEndColumns);
							}
						} catch {
							xMethodSymbol = null;
						}
					}
					xOldMethodToken = xSymbol.MethodToken;
				}
				if (xMethodSymbol != null) {
                    if(aLabelAddressMappings.ContainsKey(xSymbol.LabelName)){
						uint xAddress = aLabelAddressMappings[xSymbol.LabelName];
						//try {
						int xIdx = GetIndexClosestSmallerMatch(xCodeOffsets, xSymbol.ILOffset);
                        var xSourceInfo = new SourceInfo()
                        {
                            SourceFile = xCodeDocuments[xIdx].URL,
                            Line = xCodeLines[xIdx],
                            LineEnd = xCodeEndLines[xIdx],
                            Column = xCodeColumns[xIdx],
                            ColumnEnd = xCodeEndColumns[xIdx],
                            MethodName=xSymbol.MethodName
                        };
						xResult.Add(xAddress, xSourceInfo);
					}
				}
			}
            xSW.Stop();
            System.Diagnostics.Trace.WriteLine("Loading PDB info took: " + xSW.Elapsed);
			return xResult;
		}
	}
}