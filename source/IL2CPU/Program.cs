using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Indy.IL2CPU;
using Indy.IL2CPU.IL.X86;

namespace IL2CPU {
	public class Program {
		public static void Main(string[] args) {
			//System.Diagnostics.Debugger.Break();
			try {
				string exeName = "HelloWorld.exe";
				if (args.Length > 0) {
					exeName = args[0];
				}
				string outputFileName = @"output.asm";
				if (args.Length > 1) {
					outputFileName = args[1];
				}
				Engine e = new Engine();
				e.DebugLog += delegate(LogSeverityEnum aSeverity, string aMessage) {
					if (aSeverity == LogSeverityEnum.Warning) {
						Console.ForegroundColor = ConsoleColor.Yellow;
					}
					Console.WriteLine(aMessage);
					Console.ResetColor();
				};
				using (FileStream fs = new FileStream(outputFileName, FileMode.Create)) {
					using (StreamWriter br = new StreamWriter(fs)) {
						e.Execute(exeName, TargetPlatformEnum.NativeX86, br);
					}
				}
			} catch (ReflectionTypeLoadException E) {
				Console.WriteLine(E.ToString());
				for (int i = 0; i < E.LoaderExceptions.Length; i++) {
					Console.WriteLine("[{0}] {1}", i + 1, E.LoaderExceptions[i]);
					Console.WriteLine();
				}
			} catch (Exception E) {
				Console.WriteLine(E.ToString());
			}
			Console.WriteLine("");
			Console.WriteLine("Completed");
		}
	}
}