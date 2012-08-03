using System;
using System.Collections.Generic;
using Globalization = System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using Microsoft.Build.Utilities;
using System.Diagnostics;
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

    public override bool Execute() {
      // Important! A given address can have more than one label.
      // Do NOT filter by duplicate addresses as this causes serious lookup problems.

      string xSymbolString;
      //TODO: This reads the file (13MB currently) into RAM...
      // Thats not needed.. read it line by line instead. Wait till we move to direct
      // DB only use though as we can do this at one time.
      if (!RunObjDump(out xSymbolString)) {
        return false;
      }

      var xLabels = new List<Label>();
      var xLines = xSymbolString.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

      using (var xDebugInfo = new DebugInfo(DebugInfoFile)) {
        bool xListStarted = false;
        foreach (string xLine in xLines) {
          if (String.IsNullOrEmpty(xLine)) {
            continue;
          } else if (!xListStarted) {
            // Find start of the data
            if (xLine == "SYMBOL TABLE:") {
              xListStarted = true;
            }
            continue;
          }

          uint xAddress;
          try {
            xAddress = UInt32.Parse(xLine.Substring(0, 8), Globalization.NumberStyles.HexNumber);
          } catch (Exception) {
            Log.LogError("Error processing line '" + xLine + "'");
            throw;
          }

          string xSection = xLine.Substring(17, 5);
          if (xSection != ".text" && xSection != ".data") {
            continue;
          }
          string xLabel = xLine.Substring(32);
          if (xLabel == xSection) {
            // Non label, skip
            continue;
          }

          xLabels.Add(new Label() {
            LABELNAME = xLabel,
            ADDRESS = xAddress
          });

          xDebugInfo.WriteLabels(xLabels);
        }
        xDebugInfo.WriteLabels(xLabels, true);
      }
      return true;
    }

    private bool RunObjDump(out string result) {
      result = "";
      var xTempBatFile = Path.Combine(WorkingDir, "ExtractMapFromElfFileTemp.bat");
      if (File.Exists(xTempBatFile)) {
        File.Delete(xTempBatFile);
        if (File.Exists(xTempBatFile)) {
          Log.LogError("ExtractMapFromElfFileTemp.bat already exists!");
          return false;
        }
      }

      var xTempOutFile = Path.Combine(WorkingDir, "ExtractMapFromElfFileTemp.out");
      if (File.Exists(xTempOutFile)) {
        File.Delete(xTempOutFile);
        if (File.Exists(xTempOutFile)) {
          Log.LogError("ExtractMapFromElfFileTemp.out already exists!");
          return false;
        }
      }

      File.WriteAllText(xTempBatFile, "@ECHO OFF\r\n\"" + Path.Combine(CosmosBuildDir, @"tools\cygwin\objdump.exe") + "\" --wide --syms \"" + InputFile + "\" > ExtractMapFromElfFileTemp.out");
      if (!ExecuteTool(WorkingDir, xTempBatFile, "", "objdump")) {
        return false;
      }
      result = File.ReadAllText(xTempOutFile);

      return true;
    }
  }
}