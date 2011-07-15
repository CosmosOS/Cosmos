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

namespace Cosmos.Build.MSBuild
{
    public class ExtractMapFromElfFile: BaseToolTask
    {
        [Required]
        public string InputFile
        {
            get;
            set;
        }

        [Required]
        public string DebugInfoFile
        {
            get;
            set;
        }

        [Required]
        public string WorkingDir
        {
            get;
            set;
        }

        [Required]
        public string CosmosBuildDir
        {
            get;
            set;
        }

        public override bool Execute()
        {
            string xSymbolString;
            if (!RunObjDump(out xSymbolString))
            {
                return false;
            }

            var xResult = new SortedList<uint, string>();
            var xLines = xSymbolString.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            bool xListStarted = false;
            #region Parse file
            foreach (string xLine in xLines)
            {
                if (!xListStarted)
                {
                    if (xLine != "SYMBOL TABLE:")
                    {
                        continue;
                    }
                    else
                    {
                        xListStarted = true;
                        continue;
                    }
                }
                if (String.IsNullOrEmpty(xLine))
                {
                    continue;
                }
                uint xAddress;
                try
                {
                    xAddress = UInt32.Parse(xLine.Substring(0, 8), Globalization.NumberStyles.HexNumber);
                }
                catch (Exception)
                {
                    Log.LogError("Error processing line '" + xLine + "'");
                    throw;
                }
                if (xResult.ContainsKey(xAddress))
                {
                    continue;
                }
                string xSection = xLine.Substring(17, 5);
                if (xSection != ".text" && xSection != ".data")
                {
                    continue;
                }
                string xLabel = xLine.Substring(32);
                if (xLabel == xSection)
                {
                    continue;
                }
                xResult.Add(xAddress, xLabel);
            }
            #endregion
            using (var xDebugInfo = new DebugInfo())
            {
              xDebugInfo.OpenCPDB(DebugInfoFile);
                xDebugInfo.WriteAddressLabelMappings(xResult);
            }
            return true;
        }

        private bool RunObjDump(out string result)
        {
            result = "";
            var xTempBatFile = Path.Combine(WorkingDir, "ExtractMapFromElfFileTemp.bat");
            var xTempOutFile = Path.Combine(WorkingDir, "ExtractMapFromElfFileTemp.out");
            if (File.Exists(xTempBatFile))
            {
                Log.LogError("ExtractMapFromElfFileTemp.bat already exists!");
                return false;
            }
            if (File.Exists(xTempOutFile))
            {
                Log.LogError("ExtractMapFromElfFileTemp.out already exists!");
                return false;
            }
            File.WriteAllText(xTempBatFile, "@ECHO OFF\r\n\"" + Path.Combine(CosmosBuildDir, @"tools\cygwin\objdump.exe") + "\" --wide --syms \"" + InputFile + "\" > ExtractMapFromElfFileTemp.out");
            try
            {
                try
                {
                    if (!ExecuteTool(WorkingDir, xTempBatFile, "", "objdump"))
                    {
                        return false;
                    }
                    result = File.ReadAllText(xTempOutFile);
                }
                finally
                {
                    if (File.Exists(xTempOutFile))
                    {
                        File.Delete(xTempOutFile);
                    }
                }
            }
            finally
            {
                if (File.Exists(xTempBatFile))
                {
                    File.Delete(xTempBatFile);
                }
            }

            return true;
        }
    }
}