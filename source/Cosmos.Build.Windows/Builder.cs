using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Indy.IL2CPU;
using Indy.IL2CPU.IL.X86.Native;

namespace Cosmos.Build.Windows {
	public class Builder {
		public readonly string BuildPath;
		public readonly string ToolsPath;

		public Builder() {
			BuildPath = GetBuildPath();
			ToolsPath = BuildPath + @"Tools\";
			// MtW: leave this here, otherwise VS wont copy required dependencies!
			typeof(NativeOpCodeMap).Equals(null);
		}

		protected static string GetBuildPath() {
			try {
				RegistryKey xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");
				string xResult;
				// If no key, see if we are in dev mode
				// Problem  - noone checked this for user kit mode and no key...
				xResult = (string)xKey.GetValue("Build Path");

				// Dev kit
				if (xResult == null) {
					xResult = Directory.GetCurrentDirectory().ToLowerInvariant();
					int xPos = xResult.IndexOf("source");
					if (xPos > -1) {
						// Hack around users that have source in the path 2x.. but wont 
						// accomodate if they have it >2 times
						int xPos2 = xResult.IndexOf("source", xPos + 1);
						if (xPos2 > -1) {
							xPos = xPos2;
						}
						xResult = xResult.Substring(0, xPos) + @"Build\";
					}
				}

				if (xResult == "") {
					throw new Exception("Cannot find Cosmos build path in registry.");
				}
				if (!xResult.EndsWith(@"\")) {
					xResult = xResult + @"\";
				}
				return xResult;
			} catch (Exception E) {
				throw new Exception("Error while getting Cosmos Build Path!", E);
			}
		}

		protected void RemoveFile(string aPathname) {
			if (File.Exists(aPathname)) {
				RemoveReadOnlyAttribute(aPathname);
				File.Delete(aPathname);
			}
		}

		protected void CopyFile(string aFrom, string aTo) {
			string xDir = Path.GetDirectoryName(aTo);
			if (!Directory.Exists(xDir)) {
				Directory.CreateDirectory(xDir);
			}
			File.Copy(aFrom, aTo);
		}

		protected void RemoveReadOnlyAttribute(string aPathname) {
			var xAttribs = File.GetAttributes(aPathname);
			if ((xAttribs & FileAttributes.ReadOnly) > 0) {
				// This works because we only do this if Read only is already set
				File.SetAttributes(aPathname, xAttribs ^ FileAttributes.ReadOnly);
			}
		}

		public void MakeISO() {
			string xPath = BuildPath + @"ISO\";
			RemoveFile(BuildPath + "cosmos.iso");
			RemoveFile(xPath + "output.bin");
			CopyFile(BuildPath + "output.bin", xPath + "output.bin");
			// From TFS its read only, mkisofs doesnt like that
			RemoveReadOnlyAttribute(xPath + "isolinux.bin");
			Global.Call(ToolsPath + @"mkisofs.exe", @"-R -b isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o ..\Cosmos.iso .", xPath);
		}

		private void DoDebugLog(LogSeverityEnum aSeverity, string aMessage) {
			if (DebugLog != null) {
				DebugLog(aSeverity, aMessage);
			}
		}

		public event DebugLogHandler DebugLog;

		public void Compile(DebugModeEnum aDebugMode, byte aDebugComport) {
			string xAsmPath = ToolsPath + @"asm\";
			if (!Directory.Exists(xAsmPath)) {
				Directory.CreateDirectory(xAsmPath);
			}
			Assembly xTarget = System.Reflection.Assembly.GetEntryAssembly();
			Stopwatch xSW = new Stopwatch();
			xSW.Start();
			var xEngine = new Engine();
			xEngine.DebugLog += DoDebugLog;
			xEngine.Execute(xTarget.Location, TargetPlatformEnum.NativeX86, g => Path.Combine(xAsmPath, g + ".asm"), false,
				new string[] { Path.Combine(Path.Combine(ToolsPath, "Cosmos.Kernel.Plugs"), "Cosmos.Kernel.Plugs.dll") }, aDebugMode, aDebugComport, BuildPath);
			xSW.Stop();
			Console.WriteLine("IL2CPU Run took " + xSW.Elapsed.ToString());

			RemoveFile(BuildPath + "output.obj");
			Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f elf -F stabs -o \"{0}\" \"{1}\"", BuildPath + "output.obj", xAsmPath + "main.asm"), BuildPath);

			RemoveFile(BuildPath + "output.bin");
			Global.Call(ToolsPath + @"cygwin\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", "output.bin", "output.obj"), BuildPath);
			RemoveFile(BuildPath + "output.obj");
		}

		public void BuildKernel() {
		}

		public void MakeVPC() {
			MakeISO();
			string xPath = BuildPath + @"VPC\";
			RemoveReadOnlyAttribute(xPath + "Cosmos.vmc");
			RemoveReadOnlyAttribute(xPath + "hda.vhd");
			Process.Start(xPath + "Cosmos.vmc");
		}

		public void MakeVMWare(string aSelectedVersion) {
			MakeISO();
			string xPath = BuildPath + @"VMWare\";

			if (aSelectedVersion == "VMWare Server")
				xPath += @"Server\";
			else if (aSelectedVersion == "VMWare Workstation")
				xPath += @"Workstation\";

			RemoveReadOnlyAttribute(xPath + "Cosmos.nvram");
			RemoveReadOnlyAttribute(xPath + "Cosmos.vmsd");
			RemoveReadOnlyAttribute(xPath + "Cosmos.vmx");
			RemoveReadOnlyAttribute(xPath + "Cosmos.vmxf");
			RemoveReadOnlyAttribute(xPath + "hda.vmdk");

			Process.Start(xPath + @"Cosmos.vmx");
		}

		public void MakeQEMU(bool aUseHDImage, bool aGDB, bool aWaitSerialTCP, bool aDebugger) {
			MakeISO();
			RemoveFile(BuildPath + "serial-debug.txt");
			// QEMU Docs - http://fabrice.bellard.free.fr/qemu/qemu-doc.html
			Global.Call(ToolsPath + @"qemu\qemu.exe"
				// HD image
				, (aUseHDImage ? "-hda \"" + BuildPath + "hda.img\"" : "")
				// Path for BIOS, VGA BIOS, and keymaps
				+ " -L ."
				// CD ROM image
				+ " -cdrom \"" + BuildPath + "Cosmos.iso\""
				// Boot CD ROM
				+ " -boot d"
				// Setup serial port
				// Might allow serial file later for post debugging of CPU
				// etc since serial to TCP on a byte level is likely highly innefficient
				// with the packet overhead
				// COM1
				+ " -serial \"file:" + BuildPath + "COM1-output.dbg\" "
				// COM2
				+ (aDebugger ?
				 (aWaitSerialTCP ? " -serial tcp::4444,server" + (aWaitSerialTCP ? "" : ",nowait") : "")
				 : " -serial \"file:" + BuildPath + "COM2-output.dbg\" ")
				// Enable acceleration if we are not using GDB
				+ (aGDB ? " -S -s" : " -kernel-kqemu")
				// Ethernet card - Later the model should be a QEMU option on 
				// options screen
				+ " -net nic,model=rtl8139,macaddr=52:54:00:12:34:57"
				+ " -net tap,ifname=CosmosTAP" //for network testing
				+ " -net user"
				, ToolsPath + @"qemu\", false, true);

			if (aGDB) {
				//TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
				//in this case, GDB says "program not running". Not sure how to fix this properly.
				Global.Call(ToolsPath + "gdb.exe"
					, BuildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
					, ToolsPath + @"qemu\", false, false);
			}
		}

		public void MakeUSB(char aDrive) {
			string xPath = BuildPath + @"USB\";
			RemoveFile(xPath + @"output.bin");
			File.Move(BuildPath + @"output.bin", xPath + @"output.bin");
			// Copy to USB device
			RemoveFile(aDrive + @":\output.bin");
			File.Copy(xPath + @"output.bin", aDrive + @":\output.bin");
			RemoveFile(aDrive + @":\mboot.c32");
			File.Copy(xPath + @"mboot.c32", aDrive + @":\mboot.c32");
			RemoveFile(aDrive + @":\syslinux.cfg");
			File.Copy(xPath + @"syslinux.cfg", aDrive + @":\syslinux.cfg");
			// Set MBR
			Global.Call(ToolsPath + "syslinux.exe", "-fma " + aDrive + ":", ToolsPath, true, true);
		}

		public void MakePXE() {
			string xPath = BuildPath + @"PXE\";
			RemoveFile(xPath + @"Boot\output.bin");
			File.Move(BuildPath + "output.bin", xPath + @"Boot\output.bin");
			// *Must* set working dir so tftpd32 will set itself to proper dir
			Global.Call(xPath + "tftpd32.exe", "", xPath, false, true);
		}
	}
}
