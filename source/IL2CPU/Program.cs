using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Indy.IL2CPU;
using Indy.IL2CPU.IL.X86.Win32;
using Indy.IL2CPU.IL.X86.Native;

namespace IL2CPU {
	public class Program {
		public static string InputFile;
		public static string OutputFile;
		public static string AsmFile;
		public static bool MetalMode;
		public static TargetPlatformEnum TargetPlatform = TargetPlatformEnum.Win32;

		private Type win32Type = typeof(Win32OpCodeMap);
		private Type nativeType = typeof(NativeOpCodeMap);

		private static bool ParseArguments(IEnumerable<string> aArgs) {
			Console.WriteLine("Indy IL2CPU");
			Console.WriteLine();
			foreach (string x in aArgs) {
				if (x[0] != '-') {
					Console.WriteLine("Error parsing arguments. Arguments should start with a dash ('{0}')", x);
					return false;
				}
				string xArg = x.Substring(1);
				string[] xArgParts = new string[] { xArg, "" };
				if (xArg.IndexOfAny(new char[] { '=', ':' }) > -1) {
					xArgParts[0] = xArg.Substring(0, xArg.IndexOfAny(new char[] { '=', ':' }));
					xArgParts[1] = xArg.Substring(xArg.IndexOfAny(new char[] { '=', ':' }) + 1);
				}
				switch (xArgParts[0].ToLower()) {
					case "in": {
							InputFile = xArgParts[1];
							if (InputFile.StartsWith("\"") && InputFile.EndsWith("\"")) {
								InputFile = InputFile.Substring(1, InputFile.Length - 2);
							}
							break;
						}
					case "out": {
							OutputFile = xArgParts[1];
							if (OutputFile.StartsWith("\"") && OutputFile.EndsWith("\"")) {
								OutputFile = OutputFile.Substring(1, OutputFile.Length - 2);
							}
							break;
						}
					case "asm": {
							AsmFile = xArgParts[1];
							break;
						}
					case "metal": {
							if (String.IsNullOrEmpty(xArgParts[1])) {
								MetalMode = true;
							} else {
								if (!Boolean.TryParse(xArgParts[1], out MetalMode)) {
									Console.WriteLine("Error parsing MetalMode argument. Invalid value. Valid values are '" + Boolean.TrueString + "' and '" + Boolean.FalseString + "', or use -metal/-metalmode, which is equal to -metal:true");
									return false;
								}
							}
							break;
						}
					case "platform": {
							try {
								TargetPlatform = (TargetPlatformEnum)Enum.Parse(typeof(TargetPlatformEnum), xArgParts[1], true);
							} catch {
								Console.WriteLine("Error parsing TargetPlatform argument. Invalid value '" + xArgParts[1] + "'");
								return false;
							}
							break;
						}
					default: {
							Console.WriteLine("Error parsing arguments. Arguments not recognized ('{0}')", xArgParts[0]);
							return false;
						}
				}
			}
			if (String.IsNullOrEmpty(InputFile)) {
				Console.WriteLine("Error: No InputFile specified!");
				return false;
			}
			if (!File.Exists(InputFile)) {
				Console.WriteLine("Error: InputFile '" + InputFile + "' not found!");
				return false;
			}
			if (String.IsNullOrEmpty(OutputFile)) {
				Console.WriteLine("Error: No OutputFile specified!");
				return false;
			}
			return true;
		}

		private static string FasmFileName {
			get {
				return Path.Combine(Path.Combine(Path.Combine(Directory.GetParent(typeof(Program).Assembly.Location).Parent.Parent.Parent.Parent.FullName, "Tools"), "fasm"), "fasm.exe");
			}
		}

		public static int Main(string[] args) {
			//System.Diagnostics.Debugger.Break();
			try {
				if (ParseArguments(args)) {
					Engine e = new Engine();
					e.DebugLog += delegate(LogSeverityEnum aSeverity, string aMessage) {
						if (aSeverity == LogSeverityEnum.Warning) {
							Console.ForegroundColor = ConsoleColor.Yellow;
						}
						Console.WriteLine(aMessage);
						Console.ResetColor();
					};
					bool xCleanupAsm = String.IsNullOrEmpty(AsmFile);
					if (xCleanupAsm) {
						AsmFile = Path.GetTempFileName();
					}
					using (FileStream fs = new FileStream(AsmFile, FileMode.Create)) {
						using (StreamWriter br = new StreamWriter(fs)) {
							e.Execute(InputFile, TargetPlatform, br, MetalMode);
						}
					}
					ProcessStartInfo xFasmStartInfo = new ProcessStartInfo();
					xFasmStartInfo.FileName = FasmFileName;
					xFasmStartInfo.Arguments = String.Format("\"{0}\" \"{1}\"", AsmFile, OutputFile);
					xFasmStartInfo.UseShellExecute = false;
					xFasmStartInfo.RedirectStandardError = true;
					xFasmStartInfo.RedirectStandardOutput = true;
					Console.WriteLine("fasm = '{0}'", FasmFileName);
					Process xFasm = Process.Start(xFasmStartInfo);
					xFasm.Start();
					if (!xFasm.WaitForExit(60 * 1000) || xFasm.ExitCode != 0) {
						Console.WriteLine("Error while running FASM!");
						Console.Write(xFasm.StandardOutput.ReadToEnd());
						Console.Write(xFasm.StandardError.ReadToEnd());
						return 3;
					}
				} else {
					return 1;
				}
			} catch (ReflectionTypeLoadException E) {
				Console.WriteLine(E.ToString());
				for (int i = 0; i < E.LoaderExceptions.Length; i++) {
					Console.WriteLine("[{0}] {1}", i + 1, E.LoaderExceptions[i]);
					Console.WriteLine();
				}
				return 2;
			} catch (Exception E) {
				Console.WriteLine(E.ToString());
				return 2;
			}
			Console.WriteLine("");
			Console.WriteLine("Completed");
			return 0;
		}
	}
}