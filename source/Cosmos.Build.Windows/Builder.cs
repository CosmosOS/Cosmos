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
using IL2CPU;
using System.IO.Pipes;

namespace Cosmos.Compiler.Builder {

    public class Builder {
        public string BuildPath;
        public readonly string ToolsPath;
        public readonly string AsmPath;
        public readonly Engine Engine = new Engine();
        public bool UseInternalAssembler = false;
        public Builder() {
            BuildPath = GetBuildPath();
            ToolsPath = BuildPath + @"Tools\";
            AsmPath = ToolsPath + @"asm\";
            // MtW: leave this here, otherwise VS wont copy required dependencies!
            typeof(X86OpCodeMap).Equals(null);
            
        }
        
        /// <summary>
        /// Retrieves the path to the Build directory.
        /// First looks in the .xml file in %AppData%\Cosmos.
        /// Secondly searches Registry.
        /// If no match found there either it will use the Current Directory to calculate the path to the Build directory.
        /// </summary>
        /// <returns>Full path to the Build directory.</returns>
        protected static string GetBuildPath()
        {
            try
            {
                //string xResult = "";
                Options.Load();

                //Primary look in .XML
                if (!String.IsNullOrEmpty(Options.BuildPath))
                {
                    if (Directory.Exists(Options.BuildPath))
                        return Options.BuildPath;
                    else
                        Options.BuildPath = String.Empty; //Reset the path if it doesn't exist.
                }

                //Fallback to Registry
                if (String.IsNullOrEmpty(Options.BuildPath))
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Cosmos");
                    if (key != null)
                    {
                        var xRegPath = key.GetValue("Build Path", String.Empty).ToString();
                        if (!String.IsNullOrEmpty(xRegPath))
                        {
                            if (!Directory.Exists(xRegPath))
                                throw new DirectoryNotFoundException(
                                    "Found path to Build Path in registry, but directory does not exist.");
                            Options.BuildPath = xRegPath;
                        }
                    }
                }

                //Fallback to calculating from current dir
                if (string.IsNullOrEmpty(Options.BuildPath))
                {
                    var xCalculatedPath = Directory.GetCurrentDirectory().ToLowerInvariant();
                    int xPos = xCalculatedPath.LastIndexOf("source");
                    if (xPos == -1)
                    {
                        throw new Exception("Unable to find directory named 'source' when using CurrentDirectory.");
                    }
                    xCalculatedPath = xCalculatedPath.Substring(0, xPos) + @"Build\";
                    Options.BuildPath = xCalculatedPath;
                }

                //If path not in .xml, Registry and also unable to calculate
                if (string.IsNullOrEmpty(Options.BuildPath))
                    throw new DirectoryNotFoundException("Unable to find Cosmos Build Path!");

                //Make sure path has a trailing slash
                if (!Options.BuildPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    Options.BuildPath += Path.DirectorySeparatorChar;

                Options.Save();
                return Options.BuildPath;
            }
            catch (Exception E)
            {
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
        public event Action<int, int> CompilingMethods;
        public event Action<int, int> CompilingStaticFields;
        public event Action<LogSeverityEnum, string> LogMessage;
        private void OnCompilingMethods(int aCur, int aMax) {
            if (CompilingMethods != null) { CompilingMethods(aCur, aMax); }
        }

        private void OnCompilingStaticFields(int aCur, int aMax) {
            if (CompilingStaticFields != null) { CompilingStaticFields(aCur, aMax); }
        }

        private void OnLogMessage(LogSeverityEnum aSeverity, string aMessage) {
            if (aSeverity == LogSeverityEnum.Informational) {
                return; }
            if (LogMessage != null) { LogMessage(aSeverity, aMessage); }
        }
    
        protected void ThreadExecute(object aParam) {
            var xParam = (PassedEngineValue)aParam;
            Engine.TraceAssemblies = xParam.TraceAssemblies;
            Engine.CompilingMethods += OnCompilingMethods;
            Engine.CompilingStaticFields += OnCompilingStaticFields;
            try {
                Engine.DebugLog += OnLogMessage;
                Engine.Execute(xParam.aAssembly,
                               xParam.aTargetPlatform,
                               g => Path.Combine(AsmPath,
                                                 g + ".asm"),
                               xParam.aPlugs,
                               xParam.aDebugMode,
                               1,
                               xParam.aOutputDir,
                               UseInternalAssembler);
            }catch(Exception E) {
                OnLogMessage(LogSeverityEnum.Error, E.ToString());   
            }
            CompileCompleted.Invoke();
        }

        private string GetMonoExeFilename() {
            return Path.Combine(Path.Combine(Path.Combine(Path.Combine(GetBuildPath(),
                                                                       "Tools"),
                                                          "Mono"),
                                             "bin"),
                                "mono.exe");
        }

        private string GetMonoLibDir() {
            return Path.Combine(Path.Combine(Path.Combine(GetBuildPath(),
                                                          "Tools"),
                                             "Mono"),
                                "lib");
        }

        protected void MonoThreadExecute(object aParam) {
            var xParam = (PassedEngineValue)aParam;
            var xExeParamsBuilder = new StringBuilder();
            xExeParamsBuilder.AppendFormat("\"{0}\" ",
                                           typeof(IL2CPU.Program).Assembly.Location);
            xExeParamsBuilder.AppendFormat("/assembly \"{0}\" ",
                                           xParam.aAssembly);
            if (xParam.aOutputDir.EndsWith("\\")) {
                xExeParamsBuilder.AppendFormat("/output \"{0}\\\" ",
                                            xParam.aOutputDir);
            } else {
                xExeParamsBuilder.AppendFormat("/output \"{0}\" ",
                                               xParam.aOutputDir);
            }
            xExeParamsBuilder.AppendFormat("/trace \"{0}\" ",
                                           xParam.TraceAssemblies.ToString());
            xExeParamsBuilder.AppendFormat("/target X86 ");
            xExeParamsBuilder.AppendFormat("/comport 1 ");
            foreach (var xPlug in xParam.aPlugs) {
                xExeParamsBuilder.AppendFormat("/plug \"{0}\" ",
                                               xPlug);
            }
            xExeParamsBuilder.AppendFormat("/logpipe CosmosCompileLog");
            //var xProcess = Process.Start(@"C:\Program Files\Mono-2.0.1\bin\mono.exe",
            //                             xExeParamsBuilder.ToString());
            var xProcessStartInfo = new ProcessStartInfo(GetMonoExeFilename());
            //var xProcessStartInfo = new ProcessStartInfo(typeof(IL2CPU.Program).Assembly.Location);
            xProcessStartInfo.Arguments = xExeParamsBuilder.ToString();
            xProcessStartInfo.RedirectStandardError = true;
            xProcessStartInfo.RedirectStandardInput = true;
            xProcessStartInfo.RedirectStandardOutput = true;
            xProcessStartInfo.EnvironmentVariables.Add("MONO_PATH", GetMonoLibDir());
            xProcessStartInfo.UseShellExecute = false;
            using (var xLogStream= new System.IO.Pipes.NamedPipeServerStream("CosmosCompileLog",PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough)) {
                AutoResetEvent xWaitForConEvent = new AutoResetEvent(false);
                var xAsyncCookie = xLogStream.BeginWaitForConnection(delegate { xWaitForConEvent.Set(); }, null);
                var xProcess = Process.Start(xProcessStartInfo);
                try {
                    while (!xProcess.HasExited) {
                        if (xWaitForConEvent.WaitOne(100)) {
                            break;
                        }
                    }
                    xLogStream.EndWaitForConnection(xAsyncCookie);
                    bool xRunning = true;
                        do {
                            //while (!xLogStream.DataAvailable) { Thread.Sleep(15); }
                            var xBuff = new byte[1];
                            if (xLogStream.Read(xBuff, 0, 1) != 1) { throw new Exception("No Command received!"); }
                            switch (xBuff[0]) {
                                case (byte)LogChannelCommand.CompilingMethods: {
                                        xBuff = new byte[8];
                                        if (xLogStream.Read(xBuff, 0, 8) != 8) { throw new Exception("No complete CompilingMethods data received!"); }
                                        var xCurrent = BitConverter.ToInt32(xBuff, 0);
                                        var xMax = BitConverter.ToInt32(xBuff, 4);
                                        OnCompilingMethods(xCurrent, xMax);
                                        break;
                                    }
                                case (byte)LogChannelCommand.CompilingFields: {
                                        xBuff = new byte[8];
                                        if (xLogStream.Read(xBuff, 0, 8) != 8) { throw new Exception("No complete CompilingFields data received!"); }
                                        var xCurrent = BitConverter.ToInt32(xBuff, 0);
                                        var xMax = BitConverter.ToInt32(xBuff, 4);
                                        OnCompilingStaticFields(xCurrent, xMax);
                                        break;
                                    }
                                case (byte)LogChannelCommand.LogMessage: {
                                        xBuff = new byte[5];
                                        if (xLogStream.Read(xBuff, 0, 5) != 5) { throw new Exception("No complete LogMessage data received!"); }
                                        var xSeverity = (LogSeverityEnum)xBuff[0];
                                        var xStringLength = BitConverter.ToInt32(xBuff, 1);
                                        xBuff = new byte[xStringLength];
                                        if (xLogStream.Read(xBuff, 0, xStringLength) != xStringLength) { throw new Exception("No complete LogMessage data received! (2)"); }
                                        var xMessage = Encoding.Unicode.GetString(xBuff);
                                        OnLogMessage(xSeverity, xMessage);
                                        break;
                                    }
                                case (byte)LogChannelCommand.EndOfProcessing: {
                                    xRunning = false;
                                    break;
                                }
                            }
                        } while (!xProcess.HasExited && xRunning);
                } catch (Exception E) {
                    if (xLogStream.IsConnected && !xProcess.HasExited) {
                        throw new Exception("Error while processing log command", E);
                    }
                }
                if (!xProcess.HasExited) { xProcess.Kill(); }
            }
            // at the end:
            CompileCompleted.Invoke();
        }

        // MtW: added as field, so that it can be reused by the test runner
        public Assembly TargetAssembly = Assembly.GetEntryAssembly();

        public void BeginCompile(DebugMode aDebugMode, byte aDebugComport) {
            if (!Directory.Exists(AsmPath)) {
                Directory.CreateDirectory(AsmPath);
            }
            var xEngineParams = new PassedEngineValue(TargetAssembly.Location, TargetPlatformEnum.X86
                , new string[] {
                    Path.Combine(Path.Combine(ToolsPath, "Cosmos.Kernel.Plugs"), "Cosmos.Kernel.Plugs.dll"), 
                    Path.Combine(Path.Combine(ToolsPath, "Cosmos.Hardware.Plugs"), "Cosmos.Hardware.Plugs.dll"), 
                    Path.Combine(Path.Combine(ToolsPath, "Cosmos.Sys.Plugs"), "Cosmos.Sys.Plugs.dll")
                }
                 , aDebugMode, aDebugComport, AsmPath, Options.TraceAssemblies);

            if (Options.dotNETFrameworkImplementation == dotNETFrameworkImplementationEnum.Microsoft) {
                var xThread = new Thread(new ParameterizedThreadStart(ThreadExecute));
                xThread.Start(xEngineParams);
            }else {
                var xThread = new Thread(MonoThreadExecute);
                xThread.Start(xEngineParams);
            }
        }
        
        public void Assemble() {
            RemoveFile(BuildPath + "output.obj");
            Global.Call(ToolsPath + @"nasm\nasm.exe", String.Format("-g -f bin -o \"{0}\" \"{1}\"", BuildPath + "output.obj", AsmPath + "main.asm"), BuildPath);
        }
        
        public void Link() {
            RemoveFile(BuildPath + "output.bin");
            //Global.Call(ToolsPath + @"cygwin\ld.exe", String.Format("-Ttext 0x500000 -Tdata 0x200000 -e Kernel_Start -o \"{0}\" \"{1}\"", "output.bin", "output.obj"), BuildPath);
            File.Move(Path.Combine(BuildPath, "output.obj"), Path.Combine(BuildPath, "output.bin"));
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

        public Process MakeQEMU(bool aUseHDImage, bool aGDB, bool aDebugger, string aDebugComMode, bool aUseNetworkTap, object aNetworkCard, object aAudioCard) {
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

            var xProcess = Global.Call(ToolsPath + @"qemu\qemu.exe"
                // HD image
                , xHDString
                // Path for BIOS, VGA BIOS, and keymaps
                + " -L ."
                // CD ROM image
                + " -cdrom \"" + BuildPath + "Cosmos.iso\""
                // Boot CD ROM
                + " -boot d
                // Audio hardware
                + (String.IsNullOrEmpty(aAudioCard as String) ? "" : " -soundhw " + aAudioCard)
                // Setup serial port
                // Might allow serial file later for post debugging of CPU
                // etc since serial to TCP on a byte level is likely highly innefficient
                // with the packet overhead
                //
                // COM1
                + (aDebugger ? " "+aDebugComMode : " -serial null")
                // COM2
                + " -serial file:\"" + BuildPath + "COM2-output.dbg\""
                // Enable acceleration if we are not using GDB
                + (aGDB ? " -S -s" : " -kernel-kqemu")
                // Ethernet card
                + (String.IsNullOrEmpty(aNetworkCard as String) ? "" : string.Format(" -net nic,model={0},macaddr=52:54:00:12:34:57", aNetworkCard))
                //+ " -redir tcp:5555::23" //use f.instance 'telnet localhost 5555' or 'http://localhost:5555/' to access machine
                + (String.IsNullOrEmpty(aNetworkCard as String) ? "" :(aUseNetworkTap ? " -net tap,ifname=CosmosTAP" : "") //requires TAP installed on development computer
                + " -net user\"")
                , ToolsPath + @"qemu", false, true);

            if (aGDB) {
                //TODO: If the host is really busy, sometimes GDB can run before QEMU finishes loading.
                //in this case, GDB says "program not running". Not sure how to fix this properly.
                // Add a sleep? :)
                Global.Call(ToolsPath + "gdb.exe"
                    , BuildPath + @"output.bin" + " --eval-command=\"target remote:1234\" --eval-command=\"b _CODE_REQUESTED_BREAK_\" --eval-command=\"c\""
                    , ToolsPath + @"qemu\", false, false);
            }
            return xProcess;
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
            //TODO: Hangs on 2008 - maybe needs admin permissions? Or maybe its not compat?
            // Runs from command line ok in 2008.....
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
