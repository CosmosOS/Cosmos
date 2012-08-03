using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Cosmos.Debug.Common;

namespace Cosmos.Build.MSBuild {
  
  public class ReadNAsmMapToDebugInfo : AppDomainIsolatedTask {
    [Required]
    public string InputBaseDir { get; set; }
    
    [Required]
    public string DebugInfoFile { get; set; }

    public override bool Execute() {
      // We dont use this action currently. We use elf instead.
      throw new Exception("Not used currently");

      //var xSourceInfos = SourceInfo.ParseMapFile(InputBaseDir);
      //if (xSourceInfos.Count == 0) {
      //  Log.LogError("No SourceInfos found!");
      //  return false;
      //}
      //using (var xDebugInfo = new DebugInfo(DebugInfoFile)) {
      //  xDebugInfo.WriteLabels(xSourceInfos);
      //}
      //return true;
    }
  
  }

}