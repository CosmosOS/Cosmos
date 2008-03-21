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
		public readonly string ISOPath;
		public readonly string PXEPath;
		public readonly string AsmPath;
		public readonly string VMWarePath;
		public readonly string VPCPath;
		protected IBuildConfiguration mConfig;

		public Builder() {
			BuildPath = GetBuildPath();
			ToolsPath = BuildPath + @"Tools\";
			ISOPath = BuildPath + @"ISO\";
			PXEPath = BuildPath + @"PXE\";
			AsmPath = ToolsPath + @"asm\";
			VMWarePath = BuildPath + @"VMWare\";
			VPCPath = BuildPath + @"VPC\";
		}

		public Builder(IBuildConfiguration aConfig)
			: this() {
			mConfig = aConfig;
		}

		protected static string GetBuildPath() {
			try {
				RegistryKey xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");
				string xResult;
                // If no key, see if we are in dev mode
                // Problem  - noone checked this for user kit mode and no key...
                xResult = (string)xKey.GetValue("Build Path");

                if (xResult == null)
                {
					xResult = Directory.GetCurrentDirectory();
					xResult = xResult.Substring(0, xResult.IndexOf("source"));
					xResult += @"Build\";
				}
                
                if (String.IsNullOrEmpty(xResult)) {
					throw new Exception("Cannot find Cosmos build path in registry");
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

		protected void MakeISO() {
			RemoveFile(BuildPath + "cosmos.iso");
			RemoveFile(ISOPath + "output.bin");
			CopyFile(BuildPath + "output.bin", ISOPath + "output.bin");
			// From TFS its read only, mkisofs doesnt like that
			RemoveReadOnly(ISOPath + "isolinux.bin");
			Global.Call(ToolsPath + @"mkisofs.exe", @"-R -b isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o ..\Cosmos.iso .", ISOPath);
		}

		public void Compile() {
			if (!Directory.Exists(AsmPath)) {
				Directory.CreateDirectory(AsmPath);
			}
			Assembly xTarget = System.Reflection.Assembly.GetEntryAssembly();
			Stopwatch xSW = new Stopwatch();
			xSW.Start();
			IL2CPU.Program.Main(new string[] {@"-in:" + xTarget.Location
                , "-plug:" + ToolsPath + @"Cosmos.Kernel.Plugs\Cosmos.Kernel.Plugs.dll"
                , "-platform:nativex86", "-asm:" + AsmPath,
				"-debug:d:\\debug.xml"}
				);
			xSW.Stop();
			Console.WriteLine("IL2CPU Run took " + xSW.Elapsed.ToString());

			RemoveFile(BuildPath + "output.obj");
			Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f elf -F stabs -o \"{0}\" \"{1}\"", BuildPath + "output.obj", AsmPath + "main.asm"), BuildPath);

			RemoveFile(BuildPath + "output.bin");
			Global.Call(ToolsPath + @"cygwin\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", "output.bin", "output.obj"), BuildPath);
			RemoveFile(BuildPath + "output.obj");
		}

		public void BuildKernel() {
		}

		public enum Target {
			ISO,
			PXE,
			QEMU,
			QEMU_HardDisk,
			QEMU_GDB,
			QEMU_GDB_HardDisk,
			VMWare,
			VPC
		};

		public void Build() {
			if (mConfig == null) {
				BuildOptionsWindow xOptions = new BuildOptionsWindow(this);
                
                if ((bool)!xOptions.ShowDialog())
                    return; //Cancel
				
                mConfig = xOptions;
			}

			if (mConfig.Compile) {
				Console.WriteLine("Now compiling");
				Compile();
			}

			switch (mConfig.Target) {
				case Target.ISO:
					MakeISO();
					break;

				case Target.PXE:
					RemoveFile(PXEPath + @"Boot\output.bin");
					File.Move(BuildPath + "output.bin", PXEPath + @"Boot\output.bin");
					// *Must* set working dir so tftpd32 will set itself to proper dir
                    Global.Call(PXEPath + "tftpd32.exe", "", PXEPath, false, true);
					break;

				case Target.QEMU:
					MakeISO();
					RemoveFile(BuildPath + "serial-debug.txt");
					Global.Call(ToolsPath + @"qemu\qemu.exe"
						, "-L . -cdrom \"" + BuildPath + "Cosmos.iso\" -boot d -serial"
						+ " \"file:" + BuildPath + "serial-debug.txt" + "\" -kernel-kqemu"
						+ " -net nic,model=rtl8139"
						, ToolsPath + @"qemu\"
						, false, true);
					break;

				case Target.QEMU_HardDisk:
					MakeISO();
					RemoveFile(BuildPath + "serial-debug.txt");
					Global.Call(ToolsPath + @"qemu\qemu.exe"
											, "-hda \"" + BuildPath + "hda.img\" -L . -cdrom \"" + BuildPath + "Cosmos.iso\" -boot d -serial \"file:" + BuildPath + "serial-debug.txt" + "\" -kernel-kqemu"
											+ " -net nic,model=rtl8139"
											, ToolsPath + @"qemu\"
											, false, true);
					break;

				case Target.QEMU_GDB:
					MakeISO();
					RemoveFile(BuildPath + "serial-debug.txt");
					Global.Call(ToolsPath + @"qemu\qemu.exe"
						, "-L . -cdrom \"" + BuildPath + "Cosmos.iso\" -boot d -serial \"file:" + BuildPath + "serial-debug.txt" + "\" -S -s"
						+ " -net nic,model=rtl8139"
						, ToolsPath + @"qemu\"
						, false, true);
					//TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
					//in this case, GDB says "program not running". Not sure how to fix this properly.
					Global.Call(ToolsPath + "gdb.exe"
						, BuildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
						, ToolsPath + @"qemu\", false, true);
					break;

				case Target.QEMU_GDB_HardDisk:
					MakeISO();
					RemoveFile(BuildPath + "serial-debug.txt");
					Global.Call(ToolsPath + @"qemu\qemu.exe"
						 , "-hda \"" + BuildPath + "hda.img\" -L . -cdrom \"" + BuildPath + "Cosmos.iso\" -boot d -serial \"file:" + BuildPath + "serial-debug.txt" + "\" -S -s"
						 + " -net nic,model=rtl8139"
						 , ToolsPath + @"qemu\"
						 , false, true);
					//TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
					//in this case, GDB says "program not running". Not sure how to fix this properly.
					Global.Call(ToolsPath + "gdb.exe"
						, BuildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
                        , ToolsPath + @"qemu\", false, true);
					break;

				case Target.VMWare:
					MakeISO();
					RemoveReadOnly(VMWarePath + "Cosmos.nvram");
					RemoveReadOnly(VMWarePath + "Cosmos.vmsd");
					RemoveReadOnly(VMWarePath + "Cosmos.vmx");
					RemoveReadOnly(VMWarePath + "Cosmos.vmxf");
					RemoveReadOnly(VMWarePath + "hda.vmdk");
					Process.Start(VMWarePath + "Cosmos.vmx");
					break;

				case Target.VPC:
					MakeISO();
					RemoveReadOnly(VPCPath + "Cosmos.vmc");
					RemoveReadOnly(VPCPath + "hda.vhd");
					Process.Start(VPCPath + "Cosmos.vmc");
					break;

			}
			Console.WriteLine("Press enter to continue.");
			Console.ReadLine();
		}
	}
}
