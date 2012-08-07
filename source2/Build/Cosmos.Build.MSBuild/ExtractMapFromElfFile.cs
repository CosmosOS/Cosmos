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
      string xFile = RunObjDump();

      using (var xDebugInfo = new DebugInfo(DebugInfoFile)) {
        // In future instead of loading all labels, save indexes to major labels but not IL.ASM labels.
        // Then code can find major lables, and use position markers into the map file to parse in between 
        // as needed.
        using (var xMapReader = new StreamReader(xFile)) {
          var xLabels = new List<Label>();
          bool xListStarted = false;
          string xLine;
          while ((xLine = xMapReader.ReadLine()) != null) {
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
            } catch (Exception ex) {
              Log.LogError("Error processing line '" + xLine + "' " + ex.Message);
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

            Guid xGuid;
            // See if label has an embedded GUID. If so, use it.
            if (xLabel.Contains(".GUID_")) {
              var xParts = xLabel.Split('.');
              string xPart = xParts.Single(q => q.StartsWith("GUID_"));
              xGuid = new Guid(xPart.Substring(5));
            } else {
              xGuid = Guid.NewGuid();
            }

            xLabels.Add(new Label() {
              ID = xGuid,
              Name = xLabel,
              Address = xAddress
            });
            xDebugInfo.AddLabels(xLabels);
          }
          xDebugInfo.AddLabels(xLabels, true);
        }

        // All labels are in DB, now go back and match up Method.LabelStart
        using (var xDB = xDebugInfo.DB()) {
          //TODO: EF keeps all items in RAM, even after AcceptChanges.
          // Right now we only have a few hundred methods, but when this grows we 
          // need to find a way around this. Future EF versions might addresst this better.
          // Current EF versions require us to destroy the context and create a new one.
          foreach (var xMethod in xDB.Methods) {
            //xMethod
          }
        }
      }

      return true;
    }

    private string RunObjDump() {
      var xMapFile = Path.ChangeExtension(InputFile, "map");
      File.Delete(xMapFile);
      if (File.Exists(xMapFile)) {
        throw new Exception("Could not delete " + xMapFile);
      }

      var xTempBatFile = Path.Combine(WorkingDir, "ExtractElfMap.bat");
      File.WriteAllText(xTempBatFile, "@ECHO OFF\r\n\"" + Path.Combine(CosmosBuildDir, @"tools\cygwin\objdump.exe") + "\" --wide --syms \"" + InputFile + "\" >" + Path.GetFileName(xMapFile));

      if (!ExecuteTool(WorkingDir, xTempBatFile, "", "objdump")) {
        throw new Exception("Error extracting map from " + InputFile);
      }
      File.Delete(xTempBatFile);

      return xMapFile;
    }
  }
}