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
using Indy.IL2CPU.IL.X86;

namespace Cosmos.Build.Windows {

    public class Builder {
        public string BuildPath;
        public readonly string ToolsPath;
        public readonly string AsmPath;
        public readonly Engine Engine = new Engine();
        
        public Builder() {
            BuildPath = GetBuildPath();
            ToolsPath = BuildPath + @"Tools\";
            AsmPath = ToolsPath + @"asm\";
            // MtW: leave this here, otherwise VS wont copy required dependencies!
            typeof(X86OpCodeMap).Equals(null);
        }
        
        /// <summary>
        /// Retrieves the path to the Build directory.
        /// Looks first in the Registry. If no match found there it will use the Current Directory to calculate the
        /// path to the Build directory.
        /// </summary>
        /// <returns>Full path to the Build directory.</returns>
        protected static string GetBuildPath() {
            try {
                string xResult = "";
                RegistryKey xKey = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");

                if (xKey != null)
                { // Use Registry
                    xResult = (string)xKey.GetValue("Build Path");
                }
                if (string.IsNullOrEmpty(xResult))
                {
                    xResult = Directory.GetCurrentDirectory().ToLowerInvariant();
                    int xPos = xResult.LastIndexOf("source");
                    if (xPos == -1)
                    {
                        throw new Exception("Unable to find directory named 'source' when using CurrentDirectory.");
                    }
                    xResult = xResult.Substring(0, xPos) + @"Build\";
                }

                if (string.IsNullOrEmpty(xResult)) {
                    throw new Exception("Cannot find Cosmos build path either in the Registry or using current directory search.");
                }
                if (!xResult.EndsWith(@"\"))
                {
                    xResult = xResult + @"\";
                }

                return xResult;
            } catch (Exception E) {
                throw new Exception("Error while getting Cosmos Build Path!", E);
            }
        }

        protected void RemoveFile(string aPathname)
        {
            if (File.Exists(aPathname))
            {
                RemoveReadOnlyAttribute(aPathname);
                File.Delete(aPathname);
            }
        }

        protected void CopyFile(string aFrom, string aTo)
        {
            string xDir = Path.GetDirectoryName(aTo);
            if (!Directory.Exists(xDir))
            {
                Directory.CreateDirectory(xDir);
            }
            File.Copy(aFrom, aTo);
        }

        protected void RemoveReadOnlyAttribute(string aPathname)
        {
            var xAttribs = File.GetAttributes(aPathname);
            if ((xAttribs & FileAttributes.ReadOnly) > 0)
            {
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

        public event Action CompileCompleted;
    
        protected void ThreadExecute(object aParam) {
            var xParam = (PassedEngineValue)aParam;
            Engine.TraceAssemblies = xParam.TraceAssemblies;
            Engine.Execute(xParam.aAssembly, xParam.aTargetPlatform, xParam.aGetFileNameForGroup
             , xParam.aInMetalMode, xParam.aPlugs, xParam.aDebugMode, xParam.aDebugComNumber
             , xParam.aOutputDir);
            CompileCompleted.Invoke();
        }

        public void BeginCompile(DebugModeEnum aDebugMode, Options aOptions, byte aDebugComport) {
            if (!Directory.Exists(AsmPath)) {
                Directory.CreateDirectory(AsmPath);
            }
            Assembly xTarget = System.Reflection.Assembly.GetEntryAssembly();
            var xEngineParams = new PassedEngineValue(xTarget.Location, TargetPlatformEnum.X86
             , g => Path.Combine(AsmPath, g + ".asm"), false
             , new string[] {
                Path.Combine(Path.Combine(ToolsPath, "Cosmos.Kernel.Plugs"), "Cosmos.Kernel.Plugs.dll"), 
                Path.Combine(Path.Combine(ToolsPath, "Cosmos.Hardware.Plugs"), "Cosmos.Hardware.Plugs.dll"), 
                Path.Combine(Path.Combine(ToolsPath, "Cosmos.Sys.Plugs"), "Cosmos.Sys.Plugs.dll")
             }
             , aDebugMode, aDebugComport, AsmPath, aOptions.TraceAssemblies);
            
            var xThread = new Thread(new ParameterizedThreadStart(ThreadExecute));
            xThread.Start(xEngineParams);
        }
        
        public void Assemble() {
            RemoveFile(BuildPath + "output.obj");
            Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f elf -F stabs -o \"{0}\" \"{1}\"", BuildPath + "output.obj", AsmPath + "main.asm"), BuildPath);
        }
        
        public void Link() {
            RemoveFile(BuildPath + "output.bin");
            Global.Call(ToolsPath + @"cygwin\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", "output.bin", "output.obj"), BuildPath);
            RemoveFile(BuildPath + "output.obj");
        }

        public void MakeVPC() {
            MakeISO();
            string xPath = BuildPath + @"VPC\";
            RemoveReadOnlyAttribute(xPath + "Cosmos.vmc");
            RemoveReadOnlyAttribute(xPath + "hda.vhd");
            Process.Start(xPath + "Cosmos.vmc");
        }

        public void MakeVMWare(bool useVMWareServer) {
            MakeISO();
            string xPath = BuildPath + @"VMWare\";

            if (useVMWareServer) {
                xPath += @"Server\";
            } else {
                xPath += @"Workstation\";
            }

            RemoveReadOnlyAttribute(xPath + "Cosmos.nvram");
            RemoveReadOnlyAttribute(xPath + "Cosmos.vmsd");
            RemoveReadOnlyAttribute(xPath + "Cosmos.vmx");
            RemoveReadOnlyAttribute(xPath + "Cosmos.vmxf");
            RemoveReadOnlyAttribute(xPath + "hda.vmdk");

            Process.Start(xPath + @"Cosmos.vmx");
        }

        public void MakeQEMU(bool aUseHDImage, bool aGDB, bool aDebugger, bool aUseNetworkTap, object aNetworkCard, object aAudioCard) {
            MakeISO();

            //From v0.9.1 Qemu requires forward slashes in path
            BuildPath = BuildPath.Replace('\\', '/');

            RemoveFile(BuildPath + "serial-debug.txt");
            // QEMU Docs - http://fabrice.bellard.free.fr/qemu/qemu-doc.html
            if (File.Exists(BuildPath + "COM1-output.dbg")) {
                File.Delete(BuildPath + "COM1-output.dbg");
            }
            if (File.Exists(BuildPath + "COM2-output.dbg")) {
                File.Delete(BuildPath + "COM2-output.dbg");
            }
            string xHDString = "";
            if (aUseHDImage) {
                if (File.Exists(BuildPath + "hda.img")) {
                    xHDString += "-hda \"" + BuildPath + "hda.img\" ";
                }
                if (File.Exists(BuildPath + "hdb.img")) {
                    xHDString += "-hdb \"" + BuildPath + "hdb.img\" ";
                }
                if (File.Exists(BuildPath + "hdd.img")) {
                    xHDString += "-hdb \"" + BuildPath + "hdd.img\" ";
                }
            }

            Global.Call(ToolsPath + @"qemu\qemu.exe"
                // HD image
                , xHDString
                // Path for BIOS, VGA BIOS, and keymaps
                + " -L ."
                // CD ROM image
                + " -cdrom \"" + BuildPath + "Cosmos.iso\""
                // Boot CD ROM
                + " -boot d"
                // Audio hardware
                + " -soundhw " + Enum.Parse(typeof(QemuAudioCard), aAudioCard.ToString())
                // Setup serial port
                // Might allow serial file later for post debugging of CPU
                // etc since serial to TCP on a byte level is likely highly innefficient
                // with the packet overhead
                //
                // COM1
                //" -serial tcp::4444,server"
                //TODO: QEMU supports pipes like VMWare - Change this to a pipe and use same 
                // handler as VMWare?
                + (aDebugger ? " -serial tcp:127.0.0.1:4444"
                // We have to output to a dummy file, so the other port will always be COM2
                 : " -serial null")
                // COM2
                + " -serial \"file:" + BuildPath + "COM2-output.dbg\" "
                // Enable acceleration if we are not using GDB
                + (aGDB ? " -S -s" : " -kernel-kqemu")
                // Ethernet card
                + string.Format(" -net nic,model={0},macaddr=52:54:00:12:34:57", Enum.Parse(typeof(QemuNetworkCard), aNetworkCard.ToString()))
                //+ " -redir tcp:5555::23" //use f.instance 'telnet localhost 5555' or 'http://localhost:5555/' to access machine
                + (aUseNetworkTap ? " -net tap,ifname=CosmosTAP" : "") //requires TAP installed on development computer
                + " -net user"
                , ToolsPath + @"qemu\", false, true);

            if (aGDB) {
                //TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
                //in this case, GDB says "program not running". Not sure how to fix this properly.
                // Add a sleep? :)
                Global.Call(ToolsPath + "gdb.exe"
                    , BuildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
                    , ToolsPath + @"qemu\", false, false);
            }
        }

        // Dont change these, these are passed directly to QEMU.
        // Need to fix this so we display better things, and dont tie to QEMU constants
        public enum QemuAudioCard { pcspk, sb16, es1370, adlib }
        public enum QemuNetworkCard { ne2k_pci, rtl8139, pcnet }

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
