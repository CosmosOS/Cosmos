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
		public static string BCLDir;
		public static List<string> Plugs = new List<string>();
		public static bool MetalMode;
		public static bool DebugMode = true;
		public static TargetPlatformEnum TargetPlatform = TargetPlatformEnum.Win32;
		public const string LDParamsTemplate_NativeX86 = "-Ttext 0x2000000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"";
		public const string NAsmParamsTemplate_NativeX86 = "-g -f elf -F stabs -o \"{0}\" \"{1}\"";
		public const string FAsmParamsTemplate_Win32 = "\"{1}\" \"{0}\"";
		

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
					case "plug": {
							if (!File.Exists(xArgParts[1])) {
								Console.WriteLine("Plug assembly '{0}' not found!", xArgParts[1]);
								return false;
							}
							Plugs.Add(xArgParts[1]);
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
					case "debug": {
							if (String.IsNullOrEmpty(xArgParts[1])) {
								DebugMode = true;
							} else {
								if (!Boolean.TryParse(xArgParts[1], out DebugMode)) {
									Console.WriteLine("Error parsing Debug argument. Invalid value. Valid values are '" + Boolean.TrueString + "' and '" + Boolean.FalseString + "', or use -metal/-metalmode, which is equal to -metal:true");
									return false;
								}
							}
							break;
						}
					case "bcldir": {
							if (String.IsNullOrEmpty(xArgParts[1])) {
								throw new Exception("When using the bcldir switch, you need to specify a valid path!");
							}
							BCLDir = xArgParts[1];
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

		private static string NasmFileName {
			get {
				return Path.Combine(Path.Combine(ToolsDir, "nasm"), "nasm.exe");
			}
		}

		private static string ElfLDFileName {
			get {
				return Path.Combine(Path.Combine(ToolsDir, "Binutils-NativeX86"), "ld.exe");
			}
		}

		private static string FAsmFileName {
			get {
				return Path.Combine(Path.Combine(ToolsDir, "fasm"), "fasm.exe");
			}
		}

		private static string ToolsDir {
			get {
				return Path.Combine(Directory.GetParent(typeof(Program).Assembly.Location).Parent.Parent.Parent.Parent.FullName, "Tools");
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
					string xTestOutput = Path.GetTempFileName();
					if (xCleanupAsm) {
						AsmFile = Path.GetTempFileName();
					}
					if (TargetPlatform == TargetPlatformEnum.Win32) {
						xTestOutput = OutputFile;
					}
					using (FileStream fs = new FileStream(AsmFile, FileMode.Create)) {
						using (StreamWriter br = new StreamWriter(fs)) {
							e.Execute(InputFile, TargetPlatform, br, MetalMode, DebugMode, BCLDir, Plugs);
						}
					}
					ProcessStartInfo xFasmStartInfo = new ProcessStartInfo();
					if (TargetPlatform == TargetPlatformEnum.NativeX86) {
						xFasmStartInfo.FileName = NasmFileName;
						xFasmStartInfo.Arguments = String.Format(NAsmParamsTemplate_NativeX86, xTestOutput, AsmFile);
					} else {
						xFasmStartInfo.FileName = FAsmFileName;
						xFasmStartInfo.Arguments = String.Format(FAsmParamsTemplate_Win32, xTestOutput, AsmFile);
					}
					xFasmStartInfo.UseShellExecute = false;
					xFasmStartInfo.RedirectStandardError = false;
					xFasmStartInfo.RedirectStandardOutput = false;
					Process xFasm = Process.Start(xFasmStartInfo);
					if (!xFasm.WaitForExit(60 * 1000) || xFasm.ExitCode != 0) {
						Console.WriteLine("Error while running FASM!");
						Console.Write(xFasm.StandardOutput.ReadToEnd());
						Console.Write(xFasm.StandardError.ReadToEnd());
						return 3;
					}
					if (TargetPlatform != TargetPlatformEnum.Win32) {
						ProcessStartInfo xLDStartInfo = new ProcessStartInfo();
						if (TargetPlatform == TargetPlatformEnum.NativeX86) {
							xLDStartInfo.FileName = ElfLDFileName;
							xLDStartInfo.Arguments = String.Format(LDParamsTemplate_NativeX86, OutputFile, xTestOutput);
						}
						xLDStartInfo.UseShellExecute = false;
						xLDStartInfo.RedirectStandardError = false;
						xLDStartInfo.RedirectStandardOutput = false;
						Process xLD = Process.Start(xLDStartInfo);
						if (!xLD.WaitForExit(60 * 1000) || xLD.ExitCode != 0) {
							Console.WriteLine("Error while running LD!");
							Console.Write(xLD.StandardOutput.ReadToEnd());
							Console.Write(xLD.StandardError.ReadToEnd());
							return 4;
						}
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