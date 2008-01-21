using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Win32;
using System.Threading;

namespace Cosmos.Build.Windows {
    public class Builder {
        protected string mBuildPath;
        protected string mToolsPath;
        protected string mISOPath;
        protected string mPXEPath;
        protected string mAsmPath;

        public static string GetBuildPath() {
			try {
				RegistryKey xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");
			    string xResult;
                if (xKey == null) {
                    xResult = Directory.GetCurrentDirectory();
                    xResult = xResult.Substring(0, xResult.IndexOf("source"));
                    xResult += @"Build\";
                }
                else { xResult = (string)xKey.GetValue("Build Path"); }
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

        public Builder() {
            mBuildPath = GetBuildPath();
            mToolsPath = mBuildPath + @"Tools\";
            mISOPath = mBuildPath + @"ISO\";
            mPXEPath = mBuildPath + @"PXE\";
            mAsmPath = mToolsPath + @"asm\";
        }

        protected void RemoveFile(string aPathname) {
            if (File.Exists(aPathname)) {
                File.Delete(aPathname);
            }
        }

        protected void MakeISO() {
            RemoveFile(mBuildPath + "cosmos.iso");
            RemoveFile(mISOPath + "output.bin");
            File.Copy(mBuildPath + "output.bin", mISOPath + "output.bin");
            // From TFS its read only, mkisofs doesnt like that
            File.SetAttributes(mISOPath + "isolinux.bin", FileAttributes.Normal);
            Global.Call(mToolsPath + @"mkisofs.exe", @"-R -b isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o ..\Cosmos.iso .", mISOPath);
        }

        public void Compile() {
            if (!Directory.Exists(mAsmPath)) {
                Directory.CreateDirectory(mAsmPath);
            }
            Assembly xTarget = System.Reflection.Assembly.GetEntryAssembly();
            IL2CPU.Program.Main(new string[] {@"-in:" + xTarget.Location
                , "-plug:" + mToolsPath + @"Cosmos.Kernel.Plugs\Cosmos.Kernel.Plugs.dll"
                , "-platform:nativex86", "-asm:" + mAsmPath}
                );

            RemoveFile(mBuildPath + "output.obj");
            Global.Call(mToolsPath + @"nasm\nasm.exe", String.Format("-g -f elf -F stabs -o \"{0}\" \"{1}\"", mBuildPath + "output.obj", mAsmPath + "main.asm"), mBuildPath);

            RemoveFile(mBuildPath + "output.bin");
            Global.Call(mToolsPath + @"cygwin\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", "output.bin", "output.obj"), mBuildPath);
            RemoveFile(mBuildPath + "output.obj");
        }

        public void BuildKernel() {
        }

        public enum Target { ISO, PXE, QEMU, QEMU_GDB };

        public void Build() {
			BuildOptionsWindow xOptions = new BuildOptionsWindow();
			xOptions.ShowDialog();
        }

        public void Build(Target aType) {
            Compile();

            switch (aType) {
                case Target.ISO:
                    MakeISO();
                    break;

                case Target.PXE:
                    RemoveFile(mPXEPath + @"Boot\output.bin");
                    File.Move(mBuildPath + "output.bin", mPXEPath + @"Boot\output.bin");
                    // *Must* set working dir so tftpd32 will set itself to proper dir
                    Global.Call(mPXEPath + "tftpd32.exe", "", mPXEPath, false, false);
                    break;

                case Target.QEMU:
                    MakeISO();
                    RemoveFile(mBuildPath + "serial-debug.txt");
                    Global.Call(mToolsPath + @"qemu\qemu.exe"
                        , "-L . -cdrom \"" + mBuildPath + "Cosmos.iso\" -boot d -serial \"file:" + mBuildPath + "serial-debug.txt" + "\" -kernel-kqemu", mToolsPath + @"qemu\"
                        , false, true);
                    break;

                case Target.QEMU_GDB:
                    MakeISO();
                    RemoveFile(mBuildPath + "serial-debug.txt");
                    Global.Call(mToolsPath + @"qemu\qemu.exe"
                        , "-L . -cdrom \"" + mBuildPath + "Cosmos.iso\" -boot d -serial \"file:" + mBuildPath + "serial-debug.txt" + "\" -S -s", mToolsPath + @"qemu\"
                        , false, true);
                    //TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
                    //in this case, GDB says "program not running". Not sure how to fix this properly.
                    Global.Call(mToolsPath + "gdb.exe"
                        , mBuildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
                        , mToolsPath + @"qemu\", false, false);
                    break;

            }
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
        }
    }
}
