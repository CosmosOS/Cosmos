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

namespace Cosmos.Build.Windows {
	public class Builder {
		public readonly string BuildPath;
		public readonly string ToolsPath;

		public Builder() {
			BuildPath = GetBuildPath();
			ToolsPath = BuildPath + @"Tools\";
		}

		protected static string GetBuildPath() {
			try {
				RegistryKey xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");
				string xResult;
                // If no key, see if we are in dev mode
                // Problem  - noone checked this for user kit mode and no key...
                xResult = (string)xKey.GetValue("Build Path");

                if (xResult == null) {
					xResult = Directory.GetCurrentDirectory();
					xResult = xResult.Substring(0, xResult.IndexOf("source"));
					xResult += @"Build\";
				}
                
                if (String.IsNullOrEmpty(xResult)) {
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
                RemoveReadOnly(aPathname);
				File.Delete(aPathname);
			}
		}
		
		protected void CopyFile(string aFrom, string aTo) {
			string xDir = Path.GetDirectoryName(aTo);
			if(!Directory.Exists(xDir)) {
				Directory.CreateDirectory(xDir);
			}
			File.Copy(aFrom, aTo);
		}

		protected void RemoveReadOnly(string aPathname) {
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
			RemoveReadOnly(xPath + "isolinux.bin");
			Global.Call(ToolsPath + @"mkisofs.exe", @"-R -b isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o ..\Cosmos.iso .", xPath);
		}

		public void Compile() {
            string xAsmPath = ToolsPath + @"asm\";
            if (!Directory.Exists(xAsmPath)) {
				Directory.CreateDirectory(xAsmPath);
			}
			Assembly xTarget = System.Reflection.Assembly.GetEntryAssembly();
			Stopwatch xSW = new Stopwatch();
			xSW.Start();
			IL2CPU.Program.Main(new string[] {@"-in:" + xTarget.Location
                , "-plug:" + ToolsPath + @"Cosmos.Kernel.Plugs\Cosmos.Kernel.Plugs.dll"
                , "-platform:nativex86", "-asm:" + xAsmPath,
				"-debug:d:\\debug.xml"}
				);
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
            RemoveReadOnly(xPath + "Cosmos.vmc");
            RemoveReadOnly(xPath + "hda.vhd");
            Process.Start(xPath + "Cosmos.vmc");
        }

        public void MakeVMWare() {
            MakeISO();
            string xPath = BuildPath + @"VMWare\";
            RemoveReadOnly(xPath + "Cosmos.nvram");
            RemoveReadOnly(xPath + "Cosmos.vmsd");
            RemoveReadOnly(xPath + "Cosmos.vmx");
            RemoveReadOnly(xPath + "Cosmos.vmxf");
            RemoveReadOnly(xPath + "hda.vmdk");
            Process.Start(xPath + "Cosmos.vmx");
        }

        public void MakeQEMU(bool aUseHDImage, bool aGDB) {
            MakeISO();
            RemoveFile(BuildPath + "serial-debug.txt");
            Global.Call(ToolsPath + @"qemu\qemu.exe"
                , (aUseHDImage ? "-hda \"" + BuildPath + "hda.img\" " : "")
                + "-L . -cdrom \"" + BuildPath + "Cosmos.iso\" -boot d -serial"
                + " \"file:" + BuildPath + "serial-debug.txt\""
                + (aGDB ? "-S -s" : "-kernel-kqemu")
                + " -net nic,model=rtl8139"
                , ToolsPath + @"qemu\"
                , false, true);

            if (aGDB) {
					//TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
					//in this case, GDB says "program not running". Not sure how to fix this properly.
					Global.Call(ToolsPath + "gdb.exe"
						, BuildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
                        , ToolsPath + @"qemu\", false, true);
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
