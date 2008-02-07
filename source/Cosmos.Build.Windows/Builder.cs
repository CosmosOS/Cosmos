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
        public readonly string BuildPath;
        public readonly string ToolsPath;
        public readonly string ISOPath;
        public readonly string PXEPath;
        public readonly string AsmPath;
        protected IBuildConfiguration mConfig;

        public Builder() {
            BuildPath = GetBuildPath();
            ToolsPath = BuildPath + @"Tools\";
            ISOPath = BuildPath + @"ISO\";
            PXEPath = BuildPath + @"PXE\";
            AsmPath = ToolsPath + @"asm\";
        }

        public Builder(IBuildConfiguration aConfig) : this() {
            mConfig = aConfig;
        }

        protected static string GetBuildPath() {
            try {
                RegistryKey xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");
                string xResult;
                if (xKey == null) {
                    xResult = Directory.GetCurrentDirectory();
                    xResult = xResult.Substring(0, xResult.IndexOf("source"));
                    xResult += @"Build\";
                } else { xResult = (string)xKey.GetValue("Build Path"); }
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

        protected void MakeISO() {
            RemoveFile(BuildPath + "cosmos.iso");
            RemoveFile(ISOPath + "output.bin");
            File.Copy(BuildPath + "output.bin", ISOPath + "output.bin");
            // From TFS its read only, mkisofs doesnt like that
            File.SetAttributes(ISOPath + "isolinux.bin", FileAttributes.Normal);
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
                , "-platform:nativex86", "-asm:" + AsmPath}
                );
            xSW.Stop();
            Console.WriteLine("IL2CPU Run took " + xSW.Elapsed.ToString());

            RemoveFile(BuildPath + "output.obj");
            Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f elf -F stabs -o \"{0}\" \"{1}\"", BuildPath + "output.obj", AsmPath + "main.asm"), BuildPath);

            RemoveFile(BuildPath + "output.bin");
            Global.Call(ToolsPath + @"cygwin\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", "output.bin", "output.obj"), BuildPath);
            RemoveFile(BuildPath + "output.obj");
        }

        public void BuildKernel() { }

        public enum Target { ISO, PXE, QEMU, QEMU_HardDisk, QEMU_GDB, QEMU_GDB_HardDisk, VMWare, VPC };

        public void Build() {
            if (mConfig == null) {
                BuildOptionsWindow xOptions = new BuildOptionsWindow(this);
                xOptions.ShowDialog();
                mConfig = xOptions;
            }

            if (mConfig.Compile) {
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
                    Global.Call(PXEPath + "tftpd32.exe", "", PXEPath, false, false);
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
                        , ToolsPath + @"qemu\", false, false);
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
                        , ToolsPath + @"qemu\", false, false);
                    break;

                case Target.VMWare:
                    MakeISO();
                // VMware [-x] [-X] [-q] [-s <variablename>=<value>]
                //[-m] [-v] [/<path_to_config>/<config>.virtual machinex ]

            }
            Console.WriteLine("Press enter to continue.");
            Console.ReadLine();
        }
    }
}
