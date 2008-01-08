using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cosmos.Build.Windows {
    //TODO: How can we ignore whole assembly? Option to IL2CPU? Passed by self?
    //[IL2CPU.Ignore]
    public class Builder {
        //TODO: Fix this - config file? Package format?
        protected const string mCosmosPath = @"S:\Source\IL2CPU\";
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

        protected void Call(string aEXEPathname, string aArgLine) {
            var xStartInfo = new ProcessStartInfo();
            xStartInfo.FileName = aEXEPathname;
            xStartInfo.Arguments = aArgLine;
            
            xStartInfo.UseShellExecute = false;
            xStartInfo.RedirectStandardError = false;
            xStartInfo.RedirectStandardOutput = false;
            var xProcess = Process.Start(xStartInfo);
            if (!xProcess.WaitForExit(60 * 1000) || xFasm.ExitCode != 0) {
                //TODO: Fix
                throw new Exception("Call failed");
                //Console.WriteLine("Error while running FASM!");
                //Console.Write(xProcess.StandardOutput.ReadToEnd());
                //Console.Write(xProcess.StandardError.ReadToEnd());
            }
        }

        protected void MakeISO() {
            RemoveFile(@"ISO\files\output.obj");
            File.Move(mBuildPath + @"output.obj", mBuildPath + @"ISO\files\");

            RemoveFile(@"ISO\cosmos.iso");
            File.SetAttributes(mBuildPath + @"ISO\files\syslinux\isolinux.bin", FileAttributes.Normal);

            
            //cd iso
            //..\..\..\Tools\mkisofs\mkisofs -R -b syslinux/isolinux.bin -no-emul-boot -boot-load-size 4 -boot-info-table -o Cosmos.iso files
            //cd ..
        }

        public void CompileIL() {
            RemoveFile("output.asm");
            //TODO - why call EXE? Lets call the asm directly!
            //..\..\source\il2cpu\bin\Debug\il2cpu "-in:..\..\source\Cosmos\Cosmos.Shell.Console\bin\Debug\Cosmos.Shell.Console.exe" "-plug:..\..\source\Cosmos\Cosmos.Kernel.Plugs\bin\Debug\Cosmos.Kernel.Plugs.dll" "-out:output.obj" "-platform:nativex86" "-asm:.\asm"
        }

        public static void Build() {
            var xBuilder = new Builder();
            xBuilder.CompileIL();
            xBuilder.MakeISO();

            //cd iso
            //# ----------- Start QEMU
            //remove-item serial-debug.txt -ea SilentlyContinue
            //cd ..\..\..\tools\qemu\
            //#.\qemu -L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial "file:..\..\build\Cosmos\ISO\serial-debug.txt" -S -s
            //$qemu = resolve-path qemu.exe
            //$qemuparms = '-L . -cdrom ..\..\build\Cosmos\ISO\Cosmos.iso -boot d -hda ..\..\build\Cosmos\ISO\C-drive.img -serial "file:..\..\build\Cosmos\ISO\serial-debug.txt" -no-kqemu -S -s'

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

            //if(!$process2.HasExited) {
            //    $process2.Kill()
            //}
            //if(!$process.HasExited) {
            //    $process.Kill()
            //}
        }
    }
}
