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

namespace Cosmos.Debug.Common {

  public class SourceInfos : SortedList<uint, SourceInfo> {
  }

  public class SourceInfo {
    public string MethodName { get; set; }
    public string SourceFile { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public int LineEnd { get; set; }
    public int ColumnEnd { get; set; }

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

  }
}