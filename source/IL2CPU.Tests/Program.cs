using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;

namespace IL2CPU.Tests {
	class Program {
		public const int TestTimeout_Seconds = 10;
		public const int TestTimeout_Milliseconds = TestTimeout_Seconds * 1000;
		static void Main(string[] args) {
			SortedList<string, bool?> xTests = new SortedList<string, bool?>();
			Console.WriteLine("IL2CPU Tester. Please be patient while all tests are executed");
			Console.WriteLine();
			string xBaseDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
			string xBaseTestsDir = Path.Combine(xBaseDir, "Tests");
			foreach (string s in Directory.GetFiles(xBaseTestsDir, "*.expected.asm", SearchOption.AllDirectories)) {
				if (File.Exists(s.Replace(".expected.asm", ".exe"))) {
					xTests.Add(s.Replace(".expected.asm", ".exe"), null);
				}
			}
			Console.WriteLine("Found {0} tests. Executing now:", xTests.Count);
			for (int i = 0; i < xTests.Count; i++) {
				string xTestExe = xTests.Keys[i];
				string xExpectedResultFile = xTestExe.Substring(0, xTestExe.Length - ".exe".Length) + ".expected.asm";
				string xActualOutputFile = Path.GetTempFileName();
				try {
					ProcessStartInfo xStartInfo = new ProcessStartInfo();
					xStartInfo.CreateNoWindow = true;
					xStartInfo.UseShellExecute = false;
					xStartInfo.FileName = Path.Combine(xBaseDir, "IL2CPU.exe");
					xStartInfo.Arguments = "\"" + xTestExe + "\" \"" + xActualOutputFile + "\"";
					xStartInfo.RedirectStandardError = true;
					xStartInfo.RedirectStandardOutput = true;
					Process xProc = Process.Start(xStartInfo);
					if (!xProc.WaitForExit(TestTimeout_Milliseconds)) {
						xTests[xTestExe] = null;
						Console.Write("E");
					}
					if (String.Equals(File.ReadAllText(xExpectedResultFile), File.ReadAllText(xActualOutputFile))) {
						xTests[xTestExe] = true;
						Console.Write(".");
					} else {
						xTests[xTestExe] = false;
						Console.Write("F");
					}
				} finally {
					File.Delete(xActualOutputFile);
				}
			}
			Console.WriteLine();
			Console.WriteLine("Test execution done.");
			if ((from item in xTests
				 where item.Value == false
				 select item.Key).Count() > 0) {
				Console.WriteLine("Tests Failed:");
				foreach (string s in (from item in xTests
									  where item.Value == false
									  select item.Key)) {
					Console.WriteLine("\t" + s);
				}
			}
			if ((from item in xTests
				 where item.Value == null
				 select item.Key).Count() > 0) {
				Console.WriteLine("Tests Timed out:");
				foreach (string s in (from item in xTests
									  where item.Value == null
									  select item.Key)) {
					Console.WriteLine("\t" + s);
				}
			}
			int xPassCount = (from item in xTests
							  where item.Value == true
							  select item).Count();
			int xFailCount = (from item in xTests
							  where item.Value == false
							  select item).Count();
			int xTimeoutCount = (from item in xTests
								 where item.Value == null
								 select item).Count();
			Console.WriteLine("\tTests passed:    {0}/{1}", xPassCount, xTests.Count);
			Console.WriteLine("\tTests failed:    {0}/{1}", xFailCount, xTests.Count);
			Console.WriteLine("\tTests timed out: {0}/{1}", xTimeoutCount, xTests.Count);
		}
	}
}
