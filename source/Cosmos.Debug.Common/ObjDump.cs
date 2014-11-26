using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Cosmos.Debug.Common {
    [Obsolete("We're not using ELF Format anymore")]
	public class ObjDump {
		public static SortedList<uint, string> GetLabelByAddressMapping(string aKernel, string aObjDumpExe) {
			string[] xSymbolsContents;
			#region Run ObjDump
			string xTempFile = Path.GetTempFileName();
			var xRandom = new Random(78367);
			string xBatFile = String.Empty;
			do {
				xBatFile = Path.GetTempPath();
				xBatFile = Path.Combine(xBatFile, BitConverter.GetBytes(xRandom.NextDouble()).Aggregate<byte, string>("", (r, b) => r += b.ToString("X2").ToUpper()) + ".bat");
			} while (File.Exists(xBatFile));
			string xObjDumpFile = aObjDumpExe;
			File.WriteAllText(xBatFile, String.Format("@ECHO OFF\r\n\"{0}\" --syms --wide \"{1}\" > \"{2}\"", xObjDumpFile, aKernel, xTempFile));
			using (var xProcess = Process.Start(xBatFile)) {
				xProcess.WaitForExit();
			}
			xSymbolsContents = File.ReadAllLines(xTempFile);
			File.Delete(xTempFile);
			File.Delete(xBatFile);
			#endregion
			bool xListStarted = false;
			var xResult = new SortedList<uint, string>();
			foreach (string xLine in xSymbolsContents) {
				if (!xListStarted) {
					if (xLine != "SYMBOL TABLE:") {
						continue;
					} else {
						xListStarted = true;
						continue;
					}
				}
				if (String.IsNullOrEmpty(xLine)) {
					continue;
				}
				uint xAddress = UInt32.Parse(xLine.Substring(0, 8), System.Globalization.NumberStyles.HexNumber);
				if (xResult.ContainsKey(xAddress)) {
					continue;
				}
				string xSection = xLine.Substring(17, 5);
				if (xSection != ".text" && xSection != ".data") {
					continue;
				}
				string xLabel = xLine.Substring(32);
				if (xLabel == xSection) {
					continue;
				}
				xResult.Add(xAddress, xLabel);
			}
			return xResult;
		}
	}
}