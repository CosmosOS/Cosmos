using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.Samples.Debugging.CorSymbolStore;
using System.Diagnostics.SymbolStore;
using System.IO;

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
        public static SortedList<uint,String> ParseFile(String buildPath)
        {
            var xSourceStrings = File.ReadAllLines(Path.Combine(buildPath, "main.map"));
            SortedList<uint,String> xSource = new SortedList<uint,String>();
            uint xIndex = 0;
            for (xIndex = 0; xIndex < xSourceStrings.Length; xIndex++)
            {
                if (xSourceStrings[xIndex].StartsWith("Real      Virtual   Name"))
                {
                    xIndex++;
                    break;
                }
            }
            for (; xIndex < xSourceStrings.Length; xIndex++)
            {
                    var xLine = xSourceStrings[xIndex];
                    if (xLine.Length > 21)
                    {
                        uint xAddress = UInt32.Parse(xLine.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
                        if (!xSource.ContainsKey(xAddress))
                            xSource.Add(xAddress, xLine.Substring(20));
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

		public static SourceInfos GetSourceInfo(SortedList<uint, string> aAddressLabelMappings, string aDebugFile) {
			var xSymbolsList = new List<MLDebugSymbol>();
			MLDebugSymbol.ReadSymbolsListFromFile(xSymbolsList, aDebugFile);
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
					int xIndex = aAddressLabelMappings.IndexOfValue(xSymbol.LabelName);
					if (xIndex != -1) {
						uint xAddress = aAddressLabelMappings.Keys[xIndex];
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
			return xResult;
		}
	}
}