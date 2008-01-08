using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Build.Windows {
    //TODO: How can we ignore whole assembly? Option to IL2CPU? Passed by self?
    //[IL2CPU.Ignore]
    public class Builder {
        //TODO: Fix this - config file? Package format?
		protected const string mCosmosPath = @"D:\dotnet\IL2ASM\repos\";
        protected string mBuildPath;

        public Builder() {
            mBuildPath = mCosmosPath + @"Build\Cosmos\";
        }

        protected void RemoveFile(string aPathname) {
            aPathname = Path.Combine(mBuildPath, aPathname);
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
            RemoveFile(@"ISO\files\output.obj");
            File.Move(mBuildPath + @"ISO\output.obj", mBuildPath + @"ISO\files\output.obj");

            RemoveFile(@"ISO\cosmos.iso");
            File.SetAttributes(mBuildPath + @"ISO\files\syslinux\isolinux.bin", FileAttributes.Normal);

            Call(mCosmosPath + @"Tools\mkisofs\mkisofs.exe", "-R -b syslinux/isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o Cosmos.iso files", mBuildPath + @"ISO\", true);
        }

        public void CompileIL() {
            IL2CPU.Program.Main(new string[] {@"-in:" + mCosmosPath + @"source\Cosmos\Cosmos.Shell.Console\bin\Debug\Cosmos.Shell.Console.exe"
                , @"-plug:" + mCosmosPath + @"source\Cosmos\Cosmos.Kernel.Plugs\bin\Debug\Cosmos.Kernel.Plugs.dll"
                , @"-out:" + mBuildPath + @"ISO\output.obj", "-platform:nativex86", @"-asm:" + mBuildPath + @"asm"});
        }

		public void BuildKernel() {
			Call(mCosmosPath + @"Tools\nasm\nasm.exe", String.Format("-g -f elf -F stabs -o \"{0}\" \"{1}\"", mBuildPath + @"ISO\output.obj-tmp", mBuildPath + @"asm\main.asm"), mBuildPath, true);
			Call(mCosmosPath + @"Tools\BinUtils-NativeX86\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", mBuildPath + @"ISO\output.obj", mBuildPath + @"ISO\output.obj-tmp"), mBuildPath, true);
		}

        public void Build() {
            CompileIL();
			BuildKernel();
            MakeISO();

            RemoveFile(@"ISO\serial-debug.txt");
            //cd ..\..\tools\qemu\
			Call(mCosmosPath + @"tools\qemu\qemu.exe", @"-L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial " + "\"" + @"file:..\..\build\Cosmos\ISO\serial-debug.txt" + "\"" + " -S -s", Path.Combine(Directory.GetParent(mCosmosPath).FullName, @"tools\qemu\"), false);

            //# Still failing - because its a command line exe? run under cmd.exe?
            //$processInfo = new-object System.Diagnostics.ProcessStartInfo
            //$processInfo.FileName = $qemu
            //$processInfo.WorkingDirectory = [System.IO.Path]::GetDirectoryName($qemu);
            //$processInfo.Arguments = $qemuparms;
            //$processInfo.UseShellExecute = $False
            //$processInfo.RedirectStandardOutput = $False
            //$processInfo.RedirectStandardError = $False
            //$process = [System.Diagnostics.Process]::Start($processInfo)
            //[System.Threading.Thread]::Sleep(1000)
            //cd ..\gdb\bin\
            //$blaat = resolve-path ..\..\..\Build\Cosmos\ISO\files\output.obj
            //$gdb = resolve-path gdb.exe
            //$gdbparms =  [System.String]::Concat($blaat, ' --eval-command="target remote:1234" --eval-command="b _CODE_REQUESTED_BREAK_" --eval-command="c"');
            //$process2 = [System.Diagnostics.Process]::Start($gdb, $gdbparms);
            //$process2.WaitForExit()
        }
    }
}
