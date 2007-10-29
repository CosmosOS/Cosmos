using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;

namespace IL2CPU.Tests {
	public enum TestRunStateEnum {
		NotRan,
		TimeoutWhileRunningIL2CPU,
		TimeoutWhileRunningTest,
		WrongReturnCodeFromTest,
		Passed
	}

	class Program {
		public const int TestTimeout_Seconds = 60;
		public const int TestTimeout_Milliseconds = TestTimeout_Seconds * 1000;
		private static string IL2CPUFileName {
			get {
				return Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "IL2CPU.exe");
			}
		}
		static void Main(string[] args) {
			SortedList<string, TestRunStateEnum> xTests = new SortedList<string, TestRunStateEnum>();
			Console.WriteLine("IL2CPU Tester. Please be patient while all tests are executed");
			Console.WriteLine();
			string xBaseDir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
			string xBaseTestsDir = Path.Combine(xBaseDir, "Tests");
			foreach (string s in Directory.GetFiles(xBaseTestsDir, "*.exe", SearchOption.AllDirectories)) {
				xTests.Add(s, TestRunStateEnum.NotRan);
			}
			Console.WriteLine("Found {0} tests. Executing now:", xTests.Count);
			for (int i = 0; i < xTests.Count; i++) {
				string xTestExe = xTests.Keys[i];
				try {
					ProcessStartInfo xStartInfo = new ProcessStartInfo();
					xStartInfo.CreateNoWindow = true;
					xStartInfo.UseShellExecute = false;
					xStartInfo.FileName = IL2CPUFileName;
					xStartInfo.Arguments = "-in:\"" + xTestExe + "\" -out:\"" + xTestExe + ".exe\"";
					xStartInfo.RedirectStandardError = true;
					xStartInfo.RedirectStandardOutput = true;
					using (Process xProc = Process.Start(xStartInfo)) {
						if (!xProc.WaitForExit(TestTimeout_Milliseconds)) {
							xTests[xTestExe] = TestRunStateEnum.TimeoutWhileRunningIL2CPU;
							Console.Write("E");
							continue;
						}
						if (xProc.ExitCode != 0) {
							Console.Write("E");
							Console.WriteLine("Result of running IL2CPU on '" + xTestExe.Substring(xBaseDir.Length + 1) + ":");
							string[] lines = xProc.StandardOutput.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
							foreach (string s in lines) {
								Console.WriteLine("\t" + s);
							}
							lines = xProc.StandardError.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
							foreach (string s in lines) {
								Console.WriteLine("\t" + s);
							}
							continue;
						}
					}
					xStartInfo = new ProcessStartInfo();
					xStartInfo.CreateNoWindow = true;
					xStartInfo.UseShellExecute = false;
					xStartInfo.FileName = Path.Combine(xBaseDir, xTestExe + ".exe");
					xStartInfo.RedirectStandardError = true;
					xStartInfo.RedirectStandardOutput = true;
					using (Process xProc = Process.Start(xStartInfo)) {
						if (!xProc.WaitForExit(TestTimeout_Milliseconds)) {
							xTests[xTestExe] = TestRunStateEnum.TimeoutWhileRunningTest;
							Console.Write("T");
							continue;
						}
						if (xProc.ExitCode != 0) {
							xTests[xTestExe] = TestRunStateEnum.WrongReturnCodeFromTest;
							Console.Write("R");
							continue;
						}
					}
					xTests[xTestExe] = TestRunStateEnum.Passed;
					Console.Write(".");
				} catch (Exception E) {
					Console.WriteLine("Error while running test '" + xTestExe + "':");
					Console.WriteLine(E.ToString());

				} finally {
					if (File.Exists(xTestExe + ".exe")) {
						try {
							File.Delete(xTestExe + ".exe");
						} catch {
						}
					}
				}
			}
			Console.WriteLine();
			Console.WriteLine("Test execution done.");
			foreach (string s in Enum.GetNames(typeof(TestRunStateEnum))) {
				if (s == TestRunStateEnum.Passed.ToString()) {
					continue;
				}
				if ((from item in xTests
					 where item.Value.ToString() == s
					 select item.Key).Count() > 0) {
					Console.WriteLine("Tests with teststate '{0}':", s);
					foreach (string x in (from item in xTests
										  where item.Value.ToString() == s
										  select item.Key)) {
						Console.WriteLine("\t" + x.Substring(xBaseTestsDir.Length + 1));
					}

				}
			}
			Console.WriteLine("Tests passed:    {0}/{1}", (from item in xTests
														   where item.Value == TestRunStateEnum.Passed
														   select 1).Count(), xTests.Count);
		}
	}
}
