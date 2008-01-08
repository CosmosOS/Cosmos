using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Cosmos.Build.Windows {
    public class Builder {
        //TODO: Fix this - config file? Package format?
        protected const string mCosmosPath = @"s:\source\il2cpu\";
        protected string mBuildPath;

        public Builder() {
            mBuildPath = mCosmosPath + @"Build\Cosmos\";
        }

        protected void RemoveFile(string aPathname) {
            if (File.Exists(aPathname)) {
                File.Delete(aPathname);
            }
        }

        protected void Call(string aEXEPathname, string aArgLine, string aWorkDir, bool aWait) {
            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = aEXEPathname;
            xStartInfo.Arguments = aArgLine;
            xStartInfo.WorkingDirectory = aWorkDir;
            xStartInfo.UseShellExecute = false;
            xStartInfo.RedirectStandardError = false;
            xStartInfo.RedirectStandardOutput = false;
            var xProcess = Process.Start(xStartInfo);
            if (aWait) {
                if (!xProcess.WaitForExit(60 * 1000) || xProcess.ExitCode != 0) {
                    //TODO: Fix
                    throw new Exception("Call failed");
                    //Console.WriteLine("Error while running FASM!");
                    //Console.Write(xProcess.StandardOutput.ReadToEnd());
                    //Console.Write(xProcess.StandardError.ReadToEnd());
                }
            }
        }

        protected void MakeISO() {
            RemoveFile(Path.Combine(mBuildPath, @"ISO\files\output.obj"));
            File.Move(mBuildPath + @"ISO\output.obj", mBuildPath + @"ISO\files\output.obj");

            RemoveFile(Path.Combine(mBuildPath, @"ISO\cosmos.iso"));
            File.SetAttributes(mBuildPath + @"ISO\files\syslinux\isolinux.bin", FileAttributes.Normal);

            Call(mCosmosPath + @"Tools\mkisofs\mkisofs.exe", "-R -b syslinux/isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o Cosmos.iso files", mBuildPath + @"ISO\", true);
        }

        public void Compile() {
            IL2CPU.Program.Main(new string[] {@"-in:" + mCosmosPath + @"source\Cosmos\Cosmos.Shell.Console\bin\Debug\Cosmos.Shell.Console.exe"
            , @"-plug:" + mCosmosPath + @"source\Cosmos\Cosmos.Kernel.Plugs\bin\Debug\Cosmos.Kernel.Plugs.dll"
            , @"-out:" + mBuildPath + @"ISO\output.obj", "-platform:nativex86", @"-asm:" + mBuildPath + @"asm"});
            Call(mCosmosPath + @"Tools\nasm\nasm.exe", String.Format("-g -f elf -F stabs -o \"{0}\" \"{1}\"", mBuildPath + @"ISO\output.obj-tmp", mBuildPath + @"asm\main.asm"), mBuildPath, true);
            Call(mCosmosPath + @"Tools\BinUtils-NativeX86\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", mBuildPath + @"ISO\output.obj", mBuildPath + @"ISO\output.obj-tmp"), mBuildPath, true);
        }

        public void BuildKernel() {
        }

        public enum Target { ISO, PXE, QEMU, QEMU_GDB, VMWare, VMWarePXE, VirtualPC, VirtualPCPXE };
        public void Build(Target aType) {
            Compile();
            switch (aType) {
                case Target.ISO:
                    MakeISO();
                    break;

                case Target.PXE:
                    RemoveFile(Path.Combine(mBuildPath, @"PXE\Boot\output.obj"));
                    File.Move(mBuildPath + @"output.obj", mBuildPath + @"PXE\Boot\output.obj");
                    // *Must* set working dir so tftpd32 will set itself to proper dir
                    Call(mBuildPath + @"tftpd32.exe", "", mBuildPath + @"pxe\", false);
                    break;

                case Target.QEMU:
                case Target.QEMU_GDB:
                    MakeISO();
                    RemoveFile(@"ISO\serial-debug.txt");
                    Call(mCosmosPath + @"tools\qemu\qemu.exe", @"-L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial " + "\"" + @"file:..\..\build\Cosmos\ISO\serial-debug.txt" + "\"" + " -S -s", mCosmosPath + @"tools\qemu\", aType == Target.QEMU);

                    if (aType == Target.QEMU_GDB) {
                        Call(mCosmosPath + @"tools\gdb\bin\gdb.exe"
                            , mBuildPath + @"ISO\files\output.obj" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
                            , mCosmosPath + @"tools\qemu\", true);
                    }
                    break;

                case Target.VMWare:
                    break;

                case Target.VMWarePXE:
                    break;

                case Target.VirtualPC:
                    break;

                case Target.VirtualPCPXE:
                    break;

            }

        }
    }
}
