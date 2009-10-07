// leave the use of the following directive in: it eases a LOT during development, as it lets you use gdb for debugging
#define OUTPUT_ELF
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Win32;
//using Indy.IL2CPU.IL.X86;
using IL2CPU;
using System.IO.Pipes;
using Cosmos.IL2CPU;
using Cosmos.IL2CPU.X86;

namespace Cosmos.Compiler.Builder
{
    /// <summary>
    /// ROLE:Controls the entire build
    /// 
    /// communicates via events to show progress
    /// </summary>
    public class Builder : IBuilder
    {
                
        public readonly string ToolsPath;
        public readonly string AsmPath;

        public event Action CompileCompleted;
        public event Action BuildCompleted;
        public event Action<BuildProgress> BuildProgress;
        public event Action<LogSeverityEnum, string> LogMessage;

        private DebugWindowController xDebugWindow = null; //HACK pass in event
        public DebugWindowController DebugWindow { get { return xDebugWindow; } }


        private readonly BuildProgress currentProgress = new BuildProgress();
        private string buildPath;

            //    public bool UseInternalAssembler = false; //via options
        public Builder()
        {
            buildPath = GetBuildPath();
            ToolsPath = BuildPath + @"Tools\";
            AsmPath = ToolsPath + @"asm\";
            // MtW: leave this here, otherwise VS wont copy required dependencies!
            //typeof(X86OpCodeMap).Equals(null);

            //TODO static hack
            BuilderStep.Completed += new Action<string, object>(BuilderStep_Completed);
            BuilderStep.Started += new Action<string>(BuilderStep_Started);

            // enforce assembly linking:
            var xTheType = typeof(Indy.IL2CPU.X86.Plugs.CustomImplementations.System.Runtime.CompilerServices.RuntimeHelpersImpl);
            xTheType = typeof(Cosmos.Kernel.Plugs.ArrayListImpl);
            xTheType = typeof(Cosmos.Hardware.Plugs.FCL.System.Console);
            xTheType = typeof(Cosmos.Sys.Plugs.Deboot);

            // end enforce assembly linking


        }

        void BuilderStep_Started(string stepName)
        {

            currentProgress.Step = stepName;

            OnLogMessage(LogSeverityEnum.Informational, stepName + "Started");
            LogTime(stepName + "Started");
        }

        void BuilderStep_Completed(string stepName, object optionResult)
        {
            OnLogMessage(LogSeverityEnum.Informational, stepName + "Finished");
            LogTime(stepName + "Finished");

        }

        /// <summary>
        /// Retrieves the path to the Build directory.
        /// First looks in the .xml file in %AppData%\Cosmos.
        /// Secondly searches Registry.
        /// If no match found there either it will use the Current Directory to calculate the path to the Build directory.
        /// </summary>
        /// <returns>Full path to the Build directory.</returns>
        /// 
        ///TODO factor out
        string GetBuildPath()
        {
            try
            {
                //string xResult = "";
                var buildOptions = BuildOptions.Load();

                //Primary look in .XML
                if (!String.IsNullOrEmpty(buildOptions.BuildPath))
                {
                    if (Directory.Exists(buildOptions.BuildPath))
                        return buildOptions.BuildPath;
                    else
                        buildOptions.BuildPath = String.Empty; //Reset the path if it doesn't exist.
                }

                //Fallback to Registry
                if (String.IsNullOrEmpty(buildOptions.BuildPath))
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
                            buildOptions.BuildPath = xRegPath;
                        }
                    }
                }

                //Fallback to calculating from current dir
                if (string.IsNullOrEmpty(buildOptions.BuildPath))
                {
                    var xCalculatedPath = Directory.GetCurrentDirectory().ToLowerInvariant();
                    int xPos = xCalculatedPath.LastIndexOf("source");
                    if (xPos == -1)
                    {
                        xPos = xCalculatedPath.LastIndexOf("buildoutput");
                        if (xPos == -1)
                        {
                            throw new Exception("Unable to find directory named 'BuildOutput' or 'Source' when using CurrentDirectory. (CurrentDirectory = '" + Environment.CurrentDirectory + "')");
                        }
                    }
                    xCalculatedPath = xCalculatedPath.Substring(0, xPos) + @"Build\";
                    buildOptions.BuildPath = xCalculatedPath;
                }

                //If path not in .xml, Registry and also unable to calculate
                if (string.IsNullOrEmpty(buildOptions.BuildPath))
                    throw new DirectoryNotFoundException("Unable to find Cosmos Build Path!");

                //Make sure path has a trailing slash
                if (!buildOptions.BuildPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    buildOptions.BuildPath += Path.DirectorySeparatorChar;

                buildOptions.Save();
                return buildOptions.BuildPath;
            }
            catch (Exception E)
            {
                throw new Exception("Error while getting Cosmos Build Path!", E);
            }
        }





        
        private void OnCompilingMethods(int aCur, int aMax)
        { 
            currentProgress.MethodsProcessed = aCur;
            currentProgress.MaxMethods = aMax; 
            OnBuildProgressChanged();
        }

        private void OnCompilingStaticFields(int aCur, int aMax)
        {
            if (aCur < currentProgress.FieldsProcessed
     || aMax < currentProgress.MaxFields)
                throw new ArgumentException("Trying to set lower"); 

            currentProgress.FieldsProcessed = aCur;
            currentProgress.MaxFields = aMax;
            OnBuildProgressChanged();
        }

        protected void OnBuildProgressChanged()
        {
            if (BuildProgress != null)
                BuildProgress(currentProgress); 
        }

        private void OnLogMessage(LogSeverityEnum aSeverity, string aMessage)
        {
            if (aSeverity == LogSeverityEnum.Informational)
            {
              
                return;
            }
           
            if (LogMessage != null) { LogMessage(aSeverity, aMessage); }
        }

        private void LogTime(string text)
        {
            OnLogMessage(LogSeverityEnum.Performance, text + " " + DateTime.Now.ToLongTimeString());

        }

       


        protected void ThreadExecute(object aParam)
        {
            var options = aParam as BuildOptions;
            //var xEngineParams = BuildOptionsToEngineParam(options);
            
            LogTime("Thread execute start");

            if (options.dotNETFrameworkImplementation == dotNETFrameworkImplementationEnum.Microsoft)
            {
                RunEngine(options);
            }
            else
            {
                throw new NotImplementedException();
            }
            //    MonoCompile(xEngineParams); 

            //SIGNAL END COMPILE
            OnCompileCompleted();

            if (options.CompileIL)
            {
                // We always show the window now since when its shown its
                // for a short time and not in "paralell" as it was before.
                if (options.UseInternalAssembler == false)
                {
                    //  ShowWindow(mConsoleWindow, 1); //HACK remove show and hide console we can put a command to the UI via an event if needed but its quick! 
                    new AssembleStep(options)
                    {
#if OUTPUT_ELF
                        IsELF = true
#endif
                    }.Execute();
                    new LinkStep(options)
                    {
#if OUTPUT_ELF
                        IsELF = true
#endif
                    }.Execute();
                    //  ShowWindow(mConsoleWindow, 0); //Hide!
                }
            }

           
            Process xQEMU = null;

            //TODO factor debug out and then fold into one method
            if (options.DebugMode != DebugMode.None)
                ProcessDebug(options, ref xDebugWindow, ref xQEMU);
            else
                xQEMU = ProcessNonDebugBuilds(options, xQEMU);

            

            LogTime("Thread execute finish"); 

            //SIGNAL END
            OnBuildCompleted();
            
        }

        private void OnCompileCompleted()
        {
            if (CompileCompleted != null)
                CompileCompleted.Invoke();
            LogTime("Compile complete");
        }

        private void OnBuildCompleted()
        {
            if ( BuildCompleted != null)
                BuildCompleted.Invoke();
            LogTime("Build complete");
        }

        private static Process ProcessNonDebugBuilds(BuildOptions options, Process xQEMU)
        {
            switch (options.Target)
            {

                case "None":
                    break;
                case "QEMU":
                    var qemu = new MakeQEMUStep(options);
                    qemu.Execute();
                    xQEMU = qemu.Result;
                    break;

                case "VMWare":

                    new MakeVMWareStep(options).Execute();

                    break;

                case "VPC":
                    new MakeVPCStep(options).Execute();
                    break;

                case "ISO":
                    new MakeISOStep(options).Execute();
                    break;

                case "USB":
                    new MakeUSBStep(options).Execute();
                    break;

                case "PXE":
                    new MakePXEStep(options).Execute();
                    break;

                case "BOCHS":
                    new MakeBOCHSStep(options).Execute();
                    break;

                default:
                    throw new Exception("build mode not supported: " + options.Target);

            } return xQEMU;
        }

        //[Obsolete("Dont use")]
        //private PassedEngineValue BuildOptionsToEngineParam(BuildOptions options) {
        //    DebugMode aDebugMode = options.DebugMode; //TODO fix if not needed
        //    byte aDebugComport = options.DebugPortId;
        //    bool aGDB = options.UseGDB;

        //    if (!Directory.Exists(AsmPath)) {
        //        Directory.CreateDirectory(AsmPath);
        //    }
        //    string[] xPlugs = GetPlugs();

        //    //TODO sort out
        //    var xEngineParams = new PassedEngineValue(TargetAssembly.Location, TargetPlatformEnum.X86
        //        , xPlugs, aDebugMode, aDebugComport, aGDB, AsmPath, options.TraceAssemblies);
        //    return xEngineParams;
        //}


        private void ProcessDebug(BuildOptions options, ref DebugWindowController xDebugWindow, ref Process xQEMU)
        {

            if (options.DebugMode == DebugMode.Source)
            {
                /*var xLabelByAddressMapping = ObjDump.GetLabelByAddressMapping(
                    mBuilder.BuildPath + "output.bin", mBuilder.ToolsPath + @"cygwin\objdump.exe");*/
                var xLabelByAddressMapping = SourceInfo.ParseFile(BuildPath);
                var xSourceMappings = SourceInfo.GetSourceInfo(xLabelByAddressMapping
                   , BuildPath + "Tools/asm/debug.cxdb");

                DebugConnector xDebugConnector = null;


                switch (options.Target)
                {
                    case "QEMU":
                        if (options.DebugComMode ==
                    "TCP: Cosmos Debugger as server on port 4444, QEmu as client")
                        {
                            xDebugConnector = new DebugConnectorTCPServer();
                            Thread.Sleep(250);
                            var qemu = new MakeQEMUStep(options);
                            qemu.Execute();
                            xQEMU = qemu.Result;

                        }

                        else if (options.DebugComMode ==
                    "TCP: Cosmos Debugger as client, QEmu as server on port 4444")
                        {
                            var qemu = new MakeQEMUStep(options);
                            qemu.Execute();
                            xQEMU = qemu.Result; xDebugConnector = new DebugConnectorTCPClient();
                        }

                        else if (options.DebugComMode ==
                            "Named pipe: Cosmos Debugger as client, QEmu as server")
                        {
                            var qemu = new MakeQEMUStep(options);
                            qemu.Execute();
                            xQEMU = qemu.Result; xDebugConnector = new DebugConnectorPipeClient();
                        }
                        else if (options.DebugComMode ==
                            "Named pipe: Cosmos Debugger as server, QEmu as client")
                        {
                            xDebugConnector = new DebugConnectorPipeServer();
                            var qemu = new MakeQEMUStep(options);
                            qemu.Execute();
                            xQEMU = qemu.Result;
                        }
                        break;

                    case "VMWare":
                        xDebugConnector = new DebugConnectorPipeServer();
                        new MakeVMWareStep(options).Execute();

                        break;

                    case "USB":
                        xDebugConnector = new DebugConnectorSerial(options.DebugPortId);
                        break;

                    case "PXE":
                        xDebugConnector = new DebugConnectorSerial(options.DebugPortId);
                        break;

                    default:
                        throw new Exception("Debug mode not supported: " + options.DebugMode.ToString());

                }

                //TODO handle passing to UI better
                xDebugWindow = new DebugWindowController();
                xDebugWindow.mSourceMappings = xSourceMappings; 
                xDebugWindow.mDebugConnector = xDebugConnector;
            }
            else
            {
                throw new Exception("Debug mode not supported: " + options.DebugMode);
            }
        }



        //TODO remove more configurtional so logic should be in confg not here


        //TODO make a build step by cleaning up the engine params
        private void RunEngine(object aParam)
        {
            try
            {
                LogTime("Engine execute started");
                var xOptions = (BuildOptions)aParam;
                var xEntryAsm = Assembly.GetEntryAssembly();
                var xInitMethod = xEntryAsm.EntryPoint.DeclaringType.GetMethod("Init");
                byte xDebugCom = 0;
                if (xOptions.DebugMode != DebugMode.None)
                {
                    xDebugCom = 1;
                }
                var xAsm = new AssemblerNasm(xDebugCom);
                xAsm.DebugMode = xOptions.DebugMode;
                xAsm.TraceAssemblies = xOptions.TraceAssemblies;
#if OUTPUT_ELF
                xAsm.EmitELF = true;
#endif
                xAsm.Initialize();
                var xScanner = new ILScanner(xAsm);
                xScanner.Execute(xInitMethod);
                using (var xOut = new StreamWriter(Path.Combine(AsmPath, "main.asm"), false))
                {
                    xAsm.FlushText(xOut, Path.Combine(AsmPath, "debug.cxdb"));
                }

                LogTime("Engine execute finished");
            }
            catch(Exception E)
            {
                OnLogMessage(LogSeverityEnum.Error, E.ToString());             
            }
            #region Old code
            //var xParam = (PassedEngineValue)aParam;

            //Engine.TraceAssemblies = xParam.TraceAssemblies;
            //Engine.CompilingMethods += OnCompilingMethods;
            //Engine.CompilingStaticFields += OnCompilingStaticFields;
            //try
            //{
            //    Engine.DebugLog += OnLogMessage;

            //    LogTime("Engine execute start");
            //    currentProgress.Step = "Engine Execute";
            //    Engine.Execute(xParam.aAssembly,
            //                   xParam.aTargetPlatform,
            //                   g => Path.Combine(AsmPath, g + ".asm"),
            //                   xParam.aPlugs,
            //                   xParam.aDebugMode,
            //                   xParam.GDBDebug,
            //                   //xParam.aDebugComNumber,
            //                   1,
            //                   xParam.aOutputDir,
            //                   (Cosmos.Compiler.Builder.BuildOptions.Load()).UseInternalAssembler,
            //                   new Indy.IL2CPU.IL.X86.X86OpCodeMap(),
            //                    new CosmosAssembler(((xParam.aDebugMode != DebugMode.None) && (xParam.aDebugMode != DebugMode.MLUsingGDB))
            //                                                                ? xParam.aDebugComNumber
            //                                                                : (byte)0)); //HACK
            //    LogTime("Engine execute finish");
            //}
            //catch (Exception E)
            //{
            //    OnLogMessage(LogSeverityEnum.Error, E.ToString());
            //}
            //finally
            //{
            //    Engine.CompilingMethods -= OnCompilingMethods;
            //    Engine.CompilingStaticFields -= OnCompilingStaticFields;

            //}
            #endregion
        }

        private string GetMonoExeFilename()
        {
            return Path.Combine(Path.Combine(Path.Combine(Path.Combine(GetBuildPath(),
                                                                       "Tools"),
                                                          "Mono"),
                                             "bin"),
                                "mono.exe");
        }

        private string GetMonoLibDir()
        {
            return Path.Combine(Path.Combine(Path.Combine(GetBuildPath(),
                                                          "Tools"),
                                             "Mono"),
                                "lib");
        }

        //TODO redesign
        //protected void MonoThreadExecute(object aBuildOptions)
        //{
        //    var options = aBuildOptions as BuildOptions;
        //    var xParam = BuildOptionsToEngineParam(options);

        //    MonoCompile(xParam);
        //    CompileCompleted.Invoke();
        //    RunBuildPhase();
        //}

        //private void MonoCompile(PassedEngineValue xParam)
        //{
        //    var xExeParamsBuilder = new StringBuilder();
        //    xExeParamsBuilder.AppendFormat("\"{0}\" ",
        //                                   typeof(IL2CPU.Program).Assembly.Location);
        //    xExeParamsBuilder.AppendFormat("/assembly \"{0}\" ",
        //                                   xParam.aAssembly);
        //    if (xParam.aOutputDir.EndsWith("\\"))
        //    {
        //        xExeParamsBuilder.AppendFormat("/output \"{0}\\\" ",
        //                                    xParam.aOutputDir);
        //    }
        //    else
        //    {
        //        xExeParamsBuilder.AppendFormat("/output \"{0}\" ",
        //                                       xParam.aOutputDir);
        //    }
        //    xExeParamsBuilder.AppendFormat("/trace \"{0}\" ",
        //                                   xParam.TraceAssemblies.ToString());
        //    xExeParamsBuilder.AppendFormat("/target X86 ");
        //    xExeParamsBuilder.AppendFormat("/comport 1 ");
        //    foreach (var xPlug in xParam.aPlugs)
        //    {
        //        xExeParamsBuilder.AppendFormat("/plug \"{0}\" ",
        //                                       xPlug);
        //    }
        //    xExeParamsBuilder.AppendFormat("/logpipe CosmosCompileLog");
        //    //var xProcess = Process.Start(@"C:\Program Files\Mono-2.0.1\bin\mono.exe",
        //    //                             xExeParamsBuilder.ToString());
        //    var xProcessStartInfo = new ProcessStartInfo(GetMonoExeFilename());
        //    //var xProcessStartInfo = new ProcessStartInfo(typeof(IL2CPU.Program).Assembly.Location);
        //    xProcessStartInfo.Arguments = xExeParamsBuilder.ToString();
        //    xProcessStartInfo.RedirectStandardError = true;
        //    xProcessStartInfo.RedirectStandardInput = true;
        //    xProcessStartInfo.RedirectStandardOutput = true;
        //    xProcessStartInfo.EnvironmentVariables.Add("MONO_PATH", GetMonoLibDir());
        //    xProcessStartInfo.UseShellExecute = false;
        //    using (var xLogStream = new System.IO.Pipes.NamedPipeServerStream("CosmosCompileLog", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough))
        //    {
        //        AutoResetEvent xWaitForConEvent = new AutoResetEvent(false);
        //        var xAsyncCookie = xLogStream.BeginWaitForConnection(delegate { xWaitForConEvent.Set(); }, null);
        //        var xProcess = Process.Start(xProcessStartInfo);
        //        try
        //        {
        //            while (!xProcess.HasExited)
        //            {
        //                if (xWaitForConEvent.WaitOne(100))
        //                {
        //                    break;
        //                }
        //            }
        //            xLogStream.EndWaitForConnection(xAsyncCookie);
        //            bool xRunning = true;
        //            do
        //            {
        //                //while (!xLogStream.DataAvailable) { Thread.Sleep(15); }
        //                var xBuff = new byte[1];
        //                if (xLogStream.Read(xBuff, 0, 1) != 1) { throw new Exception("No Command received!"); }
        //                switch (xBuff[0])
        //                {
        //                    case (byte)LogChannelCommand.CompilingMethods:
        //                        {
        //                            xBuff = new byte[8];
        //                            if (xLogStream.Read(xBuff, 0, 8) != 8) { throw new Exception("No complete CompilingMethods data received!"); }
        //                            var xCurrent = BitConverter.ToInt32(xBuff, 0);
        //                            var xMax = BitConverter.ToInt32(xBuff, 4);
        //                            OnCompilingMethods(xCurrent, xMax);
        //                            break;
        //                        }
        //                    case (byte)LogChannelCommand.CompilingFields:
        //                        {
        //                            xBuff = new byte[8];
        //                            if (xLogStream.Read(xBuff, 0, 8) != 8) { throw new Exception("No complete CompilingFields data received!"); }
        //                            var xCurrent = BitConverter.ToInt32(xBuff, 0);
        //                            var xMax = BitConverter.ToInt32(xBuff, 4);
        //                            OnCompilingStaticFields(xCurrent, xMax);
        //                            break;
        //                        }
        //                    case (byte)LogChannelCommand.LogMessage:
        //                        {
        //                            xBuff = new byte[5];
        //                            if (xLogStream.Read(xBuff, 0, 5) != 5) { throw new Exception("No complete LogMessage data received!"); }
        //                            var xSeverity = (LogSeverityEnum)xBuff[0];
        //                            var xStringLength = BitConverter.ToInt32(xBuff, 1);
        //                            xBuff = new byte[xStringLength];
        //                            if (xLogStream.Read(xBuff, 0, xStringLength) != xStringLength) { throw new Exception("No complete LogMessage data received! (2)"); }
        //                            var xMessage = Encoding.Unicode.GetString(xBuff);
        //                            OnLogMessage(xSeverity, xMessage);
        //                            break;
        //                        }
        //                    case (byte)LogChannelCommand.EndOfProcessing:
        //                        {
        //                            xRunning = false;
        //                            break;
        //                        }
        //                }
        //            } while (!xProcess.HasExited && xRunning);
        //        }
        //        catch (Exception E)
        //        {
        //            if (xLogStream.IsConnected && !xProcess.HasExited)
        //            {
        //                throw new Exception("Error while processing log command", E);
        //            }
        //        }
        //        if (!xProcess.HasExited) { xProcess.Kill(); }
        //    }
        //    // at the end:
        //}

        // MtW: added as field, so that it can be reused by the test runner
        public Assembly TargetAssembly = Assembly.GetEntryAssembly();

        public string[] GetPlugs() {
            string[] xPlugs;
            // Look in tools path first. If not there, look in same location as builder asm
            if (File.Exists(Path.Combine(Path.Combine(ToolsPath, "Cosmos.Kernel.Plugs"), "Cosmos.Kernel.Plugs.dll"))) {
                xPlugs = new string[] {
                    Path.Combine(Path.Combine(ToolsPath, "Cosmos.Kernel.Plugs"), "Cosmos.Kernel.Plugs.dll"), 
                    Path.Combine(Path.Combine(ToolsPath, "Cosmos.Hardware.Plugs"), "Cosmos.Hardware.Plugs.dll"), 
                    Path.Combine(Path.Combine(ToolsPath, "Cosmos.Sys.Plugs"), "Cosmos.Sys.Plugs.dll")
                };
            } else {
                string xPath = Path.GetDirectoryName(typeof(Builder).Assembly.Location);
                xPlugs = new string[] {
                    Path.Combine(xPath, "Cosmos.Kernel.Plugs.dll"), 
                    Path.Combine(xPath, "Cosmos.Hardware.Plugs.dll"), 
                    Path.Combine(xPath, "Cosmos.Sys.Plugs.dll")
                };
            }
            return xPlugs;
        }

        public void BeginCompile(BuildOptions options)
        {
                var xThread = new Thread(new ParameterizedThreadStart(ThreadExecute));
                xThread.Start(options);

        }

          public string BuildPath
        {
            get { return buildPath; }

        }


    }
}
