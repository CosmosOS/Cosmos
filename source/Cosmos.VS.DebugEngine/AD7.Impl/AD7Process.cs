using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;

using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Cosmos.Debug.DebugConnectors;
using Cosmos.Debug.Hosts;
using Cosmos.VS.DebugEngine.Engine.Impl;
using Cosmos.VS.DebugEngine.Utilities;

using IL2CPU.Debug.Symbols;
using Label = IL2CPU.Debug.Symbols.Label;

namespace Cosmos.VS.DebugEngine.AD7.Impl
{
    public partial class AD7Process : IDebugProcess2
    {
        public bool ASMSteppingMode = false;

        public Guid ID = Guid.NewGuid();
        protected EngineCallback mCallback;
        public AD7Thread mThread;
        protected AD7Engine mEngine;
        public uint? mCurrentAddress = null;
        public uint? mNextAddress1 = null;
        public string mCurrentASMLine = null;
        public string mNextASMLine1 = null;
        protected readonly Dictionary<string, string> mDebugInfo;
        protected LaunchType mLaunch;
        internal DebugInfo mDebugInfoDb;
        protected int mProcessExitEventSent = 0;
        // Cached stack frame. See comments in AD7Thread regading this.
        public IEnumDebugFrameInfo2 mStackFrame;
        private bool mASMSteppingOut = false;
        private int mASMSteppingOut_NumEndMethodLabelsPassed = 0;
        private Tuple<uint, uint, int> ASMBPToStepTo = null;

        //ASM Breakpoints stored as C# Address -> ASM Address, C# BP ID
        //Allows quick look-up on INT3 occurring
        private List<Tuple<uint, uint, int>> ASMBreakpoints = new List<Tuple<uint, uint, int>>();
        private ManualResetEvent ASMWindow_CurrentLineUpdated = new ManualResetEvent(false);
        private ManualResetEvent ASMWindow_NextLine1Updated = new ManualResetEvent(false);
        private ManualResetEvent ASMWindow_NextAddress1Updated = new ManualResetEvent(false);

        private ManualResetEvent StackDataUpdated = new ManualResetEvent(false);

        // Connection to target environment. Usually serial but is
        // abstracted to allow other transports (ethernet, etc)
        public DebugConnector mDbgConnector;
        //
        // These are static because we need them persistent between debug
        // sessions to avoid reconnection issues. But they are not created
        // until the debug session is ready the first time so that we know
        // the debug window pipes are already ready.
        //
        // Pipe for writing responses to communicate with Cosmos.VS.Windows
        static private Cosmos.Debug.Common.PipeClient mDebugDownPipe = null;
        // Pipe to receive messages from Cosmos.VS.Windows
        static private Cosmos.Debug.Common.PipeServer mDebugUpPipe = null;

        Host mHost;

        public string mISO;
        public string mProjectFile;

        protected void DbgCmdRegisters(byte[] aData)
        {
            mDebugDownPipe.SendCommand(Debugger2Windows.Registers, aData);

            if (aData.Length < 40)
            {
                mCurrentAddress = null;
            }
            else
            {
                uint x32 = (uint)
                    (aData[39] << 24 |
                     aData[38] << 16 |
                     aData[37] << 8 |
                     aData[36]);
                mCurrentAddress = x32;
                //SendAssembly(true);
            }
        }

        protected void DbgCmdFrame(byte[] aData)
        {
            mDebugDownPipe.SendCommand(Debugger2Windows.Frame, aData);
        }

        protected void DbgCmdPong(byte[] aData)
        {
            mDebugDownPipe.SendCommand(Debugger2Windows.PongDebugStub, aData);
        }

        protected void DbgCmdStack(byte[] aData)
        {
            mDebugDownPipe.SendCommand(Debugger2Windows.Stack, aData);
        }

        private void mDebugUpPipe_DataPacketReceived(ushort aCmd, byte[] aData)
        {
            try
            {
                if (aCmd <= 127)
                {
                    switch (aCmd)
                    {
                        case Windows2Debugger.Noop:
                            // do nothing
                            break;

                        case Windows2Debugger.PingVSIP:
                            mDebugDownPipe.SendCommand(Debugger2Windows.PongVSIP);
                            break;

                        case Windows2Debugger.PingDebugStub:
                            mDbgConnector.Ping();
                            break;

                        case Windows2Debugger.SetAsmBreak:
                            {
                                string xLabel = Encoding.UTF8.GetString(aData);
                                uint xAddress = mDebugInfoDb.GetAddressOfLabel(xLabel);
                                mDbgConnector.SetAsmBreakpoint(xAddress);
                                mDbgConnector.Continue();
                            }
                            break;

                        case Windows2Debugger.ToggleAsmBreak2:
                            {
                                string xLabel = Encoding.UTF8.GetString(aData);
                                uint xAddress = mDebugInfoDb.GetAddressOfLabel(xLabel);
                                if (GetASMBreakpointInfoFromASMAddress(xAddress) == null)
                                {
                                    SetASMBreakpoint(xAddress);
                                }
                                else
                                {
                                    ClearASMBreakpoint(xAddress);
                                }
                                break;
                            }
                        case Windows2Debugger.ToggleStepMode:
                            ASMSteppingMode = !ASMSteppingMode;
                            break;

                        case Windows2Debugger.SetStepModeAssembler:
                            ASMSteppingMode = true;
                            break;

                        case Windows2Debugger.SetStepModeSource:
                            ASMSteppingMode = false;
                            break;

                        case Windows2Debugger.CurrentASMLine:
                            {
                                mCurrentASMLine = Encoding.UTF8.GetString(aData);
                                ASMWindow_CurrentLineUpdated.Set();
                                break;
                            }
                        case Windows2Debugger.NextASMLine1:
                            {
                                if (aData.Length == 0)
                                {
                                    mNextASMLine1 = null;
                                    mNextAddress1 = null;
                                }
                                else
                                {
                                    mNextASMLine1 = Encoding.UTF8.GetString(aData);
                                    ASMWindow_NextLine1Updated.Set();
                                }
                                break;
                            }
                        case Windows2Debugger.NextLabel1:
                            {
                                string nextLabel = Encoding.UTF8.GetString(aData);
                                mNextAddress1 = mDebugInfoDb.GetAddressOfLabel(nextLabel);
                                ASMWindow_NextAddress1Updated.Set();
                                break;
                            }
                        //cmd used from assembler window
                        case Windows2Debugger.Continue:
                            Step(enum_STEPKIND.STEP_OVER);
                            break;
                        //cmd used from assembler window
                        case Windows2Debugger.AsmStepInto:
                            Step(enum_STEPKIND.STEP_INTO);
                            break;
                        default:
                            throw new Exception(String.Format("Command value '{0}' not supported in method AD7Process.mDebugUpPipe_DataPacketReceived.", aCmd));
                    }
                }
                else
                {
                    throw new NotImplementedException("Sending other channels not yet supported!");
                }
            }
            catch (Exception ex)
            {
                //We cannot afford to silently break the pipe!
                OutputText("AD7Process UpPipe receive error! " + ex.Message);
                System.Diagnostics.Debug.WriteLine("AD7Process UpPipe receive error! " + ex.ToString());
            }
        }

        private List<Tuple<uint, uint, int>> GetASMBreakpointInfoFromCSAddress(uint csAddress)
        {
            return ASMBreakpoints.Where(x => x.Item1 == csAddress).ToList();
        }
        private Tuple<uint, uint, int> GetASMBreakpointInfoFromASMAddress(uint asmAddress)
        {
            Tuple<uint, uint, int> result = null;

            var posBPs = ASMBreakpoints.Where(x => x.Item2 == asmAddress);
            if (posBPs.Any())
            {
                result = posBPs.First();
            }

            return result;
        }
        private void SetASMBreakpoint(uint aAddress)
        {
            if (GetASMBreakpointInfoFromASMAddress(aAddress) == null)
            {
                bool set = false;
                for (int xID = 0; xID < BreakpointManager.MaxBP; xID++)
                {
                    if (mEngine.BPMgr.mActiveBPs[xID] == null)
                    {
                        uint CSBPAddress = mDebugInfoDb.GetClosestCSharpBPAddress(aAddress);
                        ASMBreakpoints.Add(new Tuple<uint, uint, int>(CSBPAddress, aAddress, xID));

                        mEngine.BPMgr.mActiveBPs[xID] = new AD7BoundBreakpoint(CSBPAddress);
                        var label = mDebugInfoDb.GetLabels(CSBPAddress)[0];
                        INT3sSet.Add(new KeyValuePair<uint, string>(CSBPAddress, label));
                        mDbgConnector.SetBreakpoint(xID, CSBPAddress);

                        set = true;
                        break;
                    }
                }
                if (!set)
                {
                    throw new Exception("Maximum number of active breakpoints exceeded (" + BreakpointManager.MaxBP + ").");
                }
            }
        }
        private void ClearASMBreakpoint(uint aAddress)
        {
            var bp = GetASMBreakpointInfoFromASMAddress(aAddress);
            if (bp != null)
            {
                var xID = bp.Item3;
                int index = INT3sSet.FindIndex(x => x.Key == bp.Item1);
                INT3sSet.RemoveAt(index);
                mDbgConnector.DeleteBreakpoint(xID);
                mEngine.BPMgr.mActiveBPs[xID] = null;
                ASMBreakpoints.Remove(bp);
            }
        }

        private void DebugConnectorConnected()
        {
            OutputText("Connected to DebugStub.");
        }

        /// <summary>Instanciate the <see cref="DebugConnector"/> that will handle communications
        /// between this debug engine hosted process and the emulation environment used to run the
        /// debugged Cosmos kernel. Actual connector to be instanciated is discovered from Cosmos
        /// project properties.</summary>
        private void CreateDebugConnector()
        {
            mDbgConnector = null;
            
            mDebugInfo.TryGetValue(BuildPropertyNames.VisualStudioDebugPortString, out var xPort);

            // using (var xDebug = new StreamWriter(@"e:\debug.info", false))
            // {
            //     foreach (var xItem in mDebugInfo.AllKeys)
            //     {
            //         xDebug.WriteLine("{0}: '{1}'", xItem, mDebugInfo[xItem]);
            //     }
            //     xDebug.Flush();
            // }

            if (String.IsNullOrWhiteSpace(xPort))
            {
                mDebugInfo.TryGetValue(BuildPropertyNames.CosmosDebugPortString, out xPort);
            }

            var xParts = xPort?.Split(' ');
            if ((null == xParts) || (2 > xParts.Length))
            {
                throw new Exception(String.Format("Unable to parse VS debug port: '{0}'", xPort));
                //throw new Exception(string.Format(
                //    "The '{0}' Cosmos project file property is either ill-formed or missing.",
                //    BuildProperties.VisualStudioDebugPortString));
            }
            string xPortType = xParts[0].ToLower();
            string xPortParam = xParts[1].ToLower();

            var xLaunch = mDebugInfo[BuildPropertyNames.LaunchString];

            OutputText("Starting debug connector.");
            switch (xPortType)
            {
                case "pipe:":
                    if (xLaunch == "HyperV")
                    {
                        mDbgConnector = new DebugConnectorPipeClient(xPortParam);
                    }
                    else
                    {
                        mDbgConnector = new DebugConnectorPipeServer(xPortParam);
                    }
                    break;
                case "serial:":
                    if (xLaunch == "IntelEdison")
                    {
                        mDbgConnector = new DebugConnectorEdison(xPortParam, Path.ChangeExtension(mDebugInfo["ISOFile"], ".bin"));
                    }
                    else
                    {
                        mDbgConnector = new DebugConnectorSerial(xPortParam);
                    }
                    break;
                default:
                    throw new Exception("No debug connector found for port type '" + xPortType + "'");

            }
            mDbgConnector.SetConnectionHandler(DebugConnectorConnected);
            mDbgConnector.CmdBreak += new Action<uint>(DbgCmdBreak);
            mDbgConnector.CmdTrace += new Action<uint>(DbgCmdTrace);
            mDbgConnector.CmdText += new Action<string>(DbgCmdText);
            mDbgConnector.CmdSimpleNumber += new Action<uint>(DbgCmdSimpleNumber);
            mDbgConnector.CmdKernelPanic += new Action<uint>(DbgCmdKernelPanic);
            mDbgConnector.CmdSimpleLongNumber += new Action<ulong>(DbgCmdSimpleLongNumber);
            mDbgConnector.CmdComplexNumber += new Action<float>(DbgCmdComplexNumber);
            mDbgConnector.CmdComplexLongNumber += new Action<double>(DbgCmdComplexLongNumber);
            mDbgConnector.CmdStarted += new Action(DbgCmdStarted);
            mDbgConnector.OnDebugMsg += new Action<string>(DebugMsg);
            mDbgConnector.ConnectionLost += new Action<Exception>(DbgConnector_ConnectionLost);
            mDbgConnector.CmdRegisters += new Action<byte[]>(DbgCmdRegisters);
            mDbgConnector.CmdFrame += new Action<byte[]>(DbgCmdFrame);
            mDbgConnector.CmdStack += new Action<byte[]>(DbgCmdStack);
            mDbgConnector.CmdPong += new Action<byte[]>(DbgCmdPong);
            mDbgConnector.CmdStackCorruptionOccurred += DbgCmdStackCorruptionOccurred;
            mDbgConnector.CmdStackOverflowOccurred += DbgCmdStackOverflowOccurred;
            mDbgConnector.CmdNullReferenceOccurred += DbgCmdNullReferenceOccurred;
            mDbgConnector.CmdMessageBox += DbgCmdMessageBox;
            mDbgConnector.CmdChannel += DbgCmdChannel;
            mDbgConnector.CmdCoreDump += DbgCmdCoreDump;
        }

        private void DbgCmdChannel(byte aChannel, byte aCommand, byte[] aData)
        {
            mDebugDownPipe.SendRawToChannel(aChannel, aCommand, aData);
        }

        private void DbgCmdStackCorruptionOccurred(uint lastEIPAddress)
        {
            AD7Util.MessageBox(String.Format("Stack corruption occurred at address 0x{0:X8}! Halting now.", lastEIPAddress));
        }

        private void DbgCmdStackOverflowOccurred(uint lastEIPAddress)
        {
            AD7Util.MessageBox(String.Format("Stack overflow occurred at address 0x{0:X8}! Halting now.", lastEIPAddress));
        }

        private void DbgCmdNullReferenceOccurred(uint lastEIPAddress)
        {
            if (mDebugInfo.TryGetValue(BuildPropertyNames.DebugModeString, out var xDebugMode))
            {
                if (xDebugMode == "Source")
                {
                    try
                    {
                        var xMethod = mDebugInfoDb.GetMethod(lastEIPAddress);
                        var xLabel = mDebugInfoDb.GetLabels(lastEIPAddress)[0];
                        var xMethodIlOp = mDebugInfoDb.TryGetFirstMethodIlOpByLabelName(xLabel.Remove(xLabel.LastIndexOf('.'))).IlOffset;
                        var xSequencePoints = mDebugInfoDb.GetSequencePoints(mDebugInfoDb.GetAssemblyFileById(xMethod.AssemblyFileID).Pathname, xMethod.MethodToken);
                        var xLine = xSequencePoints.Where(q => q.Offset <= xMethodIlOp).Last().LineStart;

                        AD7Util.MessageBox($"NullReferenceException occurred in '{xMethod.LabelCall}'{Environment.NewLine}Document: {mDebugInfoDb.GetDocumentById(xMethod.DocumentID).Pathname}{Environment.NewLine}Line: {xLine}{Environment.NewLine}Address: 0x{lastEIPAddress.ToString("X8")}");
                        return;
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }

            AD7Util.MessageBox(String.Format("NullReferenceException occurred at address 0x{0:X8}! Halting now.", lastEIPAddress));
        }

        private void DbgCmdMessageBox(string message)
        {
            AD7Util.MessageBox("Message from your Cosmos operating system:\r\n\r\n" + message);
        }

        private void DbgCmdCoreDump(CoreDump dump)
        {
            var eax = GetRegister("EAX", dump.EAX);
            var ebx = GetRegister("EBX", dump.EBX);
            var ecx = GetRegister("ECX", dump.ECX);
            var edx = GetRegister("EDX", dump.EDX);

            var edi = GetRegister("EDI", dump.EDI);
            var esi = GetRegister("ESI", dump.ESI);

            var ebp = GetRegister("EBP", dump.EBP);
            var esp = GetRegister("ESP", dump.ESP);
            var eip = GetRegister("EIP", dump.EIP);

            var message = "Core dump:" + Environment.NewLine
                        + $"{eax}    {ebx}    {ecx}    {edx}" + Environment.NewLine
                        + $"{edi}    {esi}" + Environment.NewLine
                        + $"{ebp}    {esp}    {eip}" + Environment.NewLine
                        + Environment.NewLine
                        + "Call stack:"
                        + Environment.NewLine;

            while (dump.StackTrace.Count > 0)
            {
                message += GetStackTraceEntry(dump.StackTrace.Pop()) + Environment.NewLine;
            }

            AD7Util.MessageBox(message);

            string GetRegister(string name, uint value) => $"{name} = 0x{value:X8}";

            string GetStackTraceEntry(uint address)
            {
                var entry = $"at 0x{address:X8}";

                if (mDebugInfo.TryGetValue(BuildPropertyNames.DebugModeString, out var xDebugMode))
                {
                    if (xDebugMode == "Source")
                    {
                        try
                        {
                            var xMethod = mDebugInfoDb.GetMethod(address);
                            var xDocument = mDebugInfoDb.GetDocumentById(xMethod.DocumentID);
                            var xLabel = mDebugInfoDb.GetLabels(address)[0];
                            var xMethodIlOp = mDebugInfoDb.TryGetFirstMethodIlOpByLabelName(xLabel.Remove(xLabel.LastIndexOf('.'))).IlOffset;
                            var xSequencePoints = mDebugInfoDb.GetSequencePoints(mDebugInfoDb.GetAssemblyFileById(xMethod.AssemblyFileID).Pathname, xMethod.MethodToken);
                            var xLine = xSequencePoints.Where(q => q.Offset <= xMethodIlOp).Last().LineStart;

                            entry += $"in {xDocument.Pathname}:line {xLine}";
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }

                return entry;
            }
        }

        internal AD7Process(Dictionary<string, string> aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort)
        {
            mCallback = aCallback;
            mDebugInfo = aDebugInfo;

            mLaunch = (LaunchType)Enum.Parse(typeof(LaunchType), aDebugInfo[BuildPropertyNames.LaunchString]);

            if (mDebugDownPipe == null)
            {
                mDebugDownPipe = new PipeClient(Pipes.DownName);

                mDebugUpPipe = new PipeServer(Pipes.UpName);
                mDebugUpPipe.DataPacketReceived += mDebugUpPipe_DataPacketReceived;
                mDebugUpPipe.Start();
            }
            else
            {
                mDebugUpPipe.CleanHandlers();
                mDebugUpPipe.DataPacketReceived += mDebugUpPipe_DataPacketReceived;
            }

            // Must be after mDebugDownPipe is initialized
            OutputClear();
            OutputText("Debugger process initialized.");

            mISO = mDebugInfo["ISOFile"];
            OutputText("Using ISO file " + mISO + ".");
            mProjectFile = mDebugInfo["ProjectFile"];
            //
            bool xUseGDB = String.Equals(mDebugInfo[BuildPropertyNames.EnableGDBString], "true", StringComparison.InvariantCultureIgnoreCase);
            OutputText("GDB " + (xUseGDB ? "Enabled" : "Disabled") + ".");
            //
            Boolean.TryParse(mDebugInfo[BuildPropertyNames.StartCosmosGDBString], out var xGDBClient);

            switch (mLaunch)
            {
                case LaunchType.VMware:
                    #region CheckIfHyperVServiceIsRunning

                    using (System.ServiceProcess.ServiceController sc = new System.ServiceProcess.ServiceController("vmms"))
                    {
                        try
                        {
                            if (sc.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                            {
                                AD7Util.MessageBox(
                                    "The Hyper-V Virtual Machine Management Service will be stopped. This is needed to allow to run VMware.");
                                sc.Stop();
                            }
                        }
                        catch (InvalidOperationException)
                        {
                            // service not present
                        }
                    }

                    #endregion

                    mHost = new VMware(mDebugInfo, xUseGDB);
                    break;
                case LaunchType.Slave:
                    mHost = new Slave(mDebugInfo, xUseGDB);
                    break;
                case LaunchType.Bochs:
                    // The project has been created on another machine or Bochs has been uninstalled since the project has
                    // been created.
                    if (!BochsSupport.BochsEnabled)
                    {
                        throw new Exception("The Bochs emulator doesn't seem to be installed on this machine.");
                    }

                    string bochsConfigurationFileName;
                    mDebugInfo.TryGetValue(BuildProperties.BochsEmulatorConfigurationFileString, out bochsConfigurationFileName);

                    if (String.IsNullOrEmpty(bochsConfigurationFileName))
                    {
                        bochsConfigurationFileName = BuildProperties.BochsDefaultConfigurationFileName;
                    }

                    if (!Path.IsPathRooted(bochsConfigurationFileName))
                    {
                        // Assume the configuration file name is relative to project output path.
                        bochsConfigurationFileName = Path.Combine(new FileInfo(mDebugInfo["ProjectFile"]).Directory.FullName,
                          mDebugInfo["OutputPath"], bochsConfigurationFileName);
                    }
                    FileInfo bochsConfigurationFile = new FileInfo(bochsConfigurationFileName);
                    // TODO : What if the configuration file doesn't exist ? This will throw a FileNotFoundException in
                    // the Bochs class constructor. Is this appropriate behavior ?
                    mHost = new Bochs(mDebugInfo, xUseGDB, bochsConfigurationFile);

                    //((Host.Bochs)mHost).FixBochsConfiguration(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("IsoFileName", mISO) });
                    break;
                case LaunchType.IntelEdison:
                    mHost = new IntelEdison(mDebugInfo, false);
                    break;
                case LaunchType.HyperV:
                    mHost = new HyperV(mDebugInfo, false);
                    break;
                default:
                    throw new Exception("Invalid Launch value: '" + mLaunch + "'.");
            }
            mHost.OnShutDown += HostShutdown;

            string xDbPath = Path.ChangeExtension(mISO, "cdb");
            if (!File.Exists(xDbPath))
            {
                throw new Exception("Debug data file " + xDbPath + " not found. Could be a omitted build process of Cosmos project so that not created.");
            }

            mDebugInfoDb = new DebugInfo(xDbPath);
            mDebugInfoDb.LoadLookups();

            CreateDebugConnector();
            aEngine.BPMgr.SetDebugConnector(mDbgConnector);

            mEngine = aEngine;
            mThread = new AD7Thread(aEngine, this);
            mCallback.OnThreadStart(mThread);
            mPort = aPort;

            if (xUseGDB && xGDBClient)
            {
                LaunchGdbClient();
            }
        }

        protected void LaunchGdbClient()
        {
            OutputText("Launching GDB client.");
            if (File.Exists(CosmosPaths.GdbClientExe))
            {
                var xPSInfo = new ProcessStartInfo(CosmosPaths.GdbClientExe);
                xPSInfo.Arguments = "\"" + Path.ChangeExtension(mProjectFile, ".cgdb") + "\"" + @" /Connect";
                xPSInfo.UseShellExecute = false;
                xPSInfo.RedirectStandardInput = false;
                xPSInfo.RedirectStandardError = false;
                xPSInfo.RedirectStandardOutput = false;
                xPSInfo.CreateNoWindow = false;
                Process.Start(xPSInfo);
            }
            else
            {
                AD7Util.MessageBox(String.Format(
                    "The GDB-Client could not be found at \"{0}\". Please deactivate it under \"Properties/Debug/Enable GDB\"",
                    CosmosPaths.GdbClientExe), "GDB-Client");
            }
        }

        private void DbgConnector_ConnectionLost(Exception e)
        {
            if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 1)
            {
                return;
            }
            if (mDbgConnector != null)
            {
                mEngine.Callback.OnProcessExit(0);
            }
        }

        // Shows a message in the output window of VS. Needs special treatment,
        // because normally VS only shows msgs from debugged process, not internal
        // stuff like us.
        public void DebugMsg(string aMsg)
        {
            mCallback.OnOutputString(aMsg + "\n");
        }

        protected void DbgCmdStarted()
        {
            OutputText("DebugStub handshake completed.");
            DebugMsg("RmtDbg: Started");

            // OK, now debugger is ready. Send it a list of breakpoints that were set before
            // program run.
            foreach (var xBP in mEngine.BPMgr.mPendingBPs)
            {
                foreach (var xBBP in xBP.mBoundBPs)
                {
                    var label = mDebugInfoDb.GetLabels(xBBP.mAddress)[0];
                    INT3sSet.Add(new KeyValuePair<uint, string>(xBBP.mAddress, label));
                    mDbgConnector.SetBreakpoint(xBBP.RemoteID, xBBP.mAddress);
                }
            }
            mDbgConnector.SendCmd(Vs2Ds.BatchEnd);
        }

        void DbgCmdSimpleNumber(uint nr)
        {
            mCallback.OnOutputStringUser("0x" + nr.ToString("X8").ToUpper() + "\r\n");
        }

        void DbgCmdKernelPanic(uint nr)
        {
            AD7Util.MessageBox("Kernel panic: 0x" + nr.ToString());
        }

        void DbgCmdSimpleLongNumber(ulong nr)
        {
            mCallback.OnOutputStringUser("0x" + nr.ToString("X8").ToUpper() + "\r\n");
        }

        void DbgCmdComplexNumber(float nr)
        {
            mCallback.OnOutputStringUser(nr + "\r\n");
        }

        void DbgCmdComplexLongNumber(double nr)
        {
            mCallback.OnOutputStringUser(nr + "\r\n");
        }

        void DbgCmdText(string obj)
        {
            mCallback.OnOutputStringUser(obj + "\r\n");
        }

        internal AD7Thread Thread => mThread;

        void DbgCmdTrace(uint aAddress)
        {
            DebugMsg("TraceReceived: " + aAddress);
        }

        void DbgCmdBreak(uint aAddress)
        {
            // aAddress will be actual address. Call and other methods push return to (after op), but DS
            // corrects for us and sends us actual op address.
            DebugMsg("DbgCmdBreak " + aAddress + " / " + aAddress.ToString("X8").ToUpper());

            if (mASMSteppingOut)
            {
                string[] currentASMLabels = mDebugInfoDb.GetLabels(aAddress);
                foreach (string aLabel in currentASMLabels)
                {
                    if (aLabel.Contains("END__OF__METHOD_EXCEPTION__2"))
                    {
                        mASMSteppingOut_NumEndMethodLabelsPassed++;
                        break;
                    }
                }
                if (mASMSteppingOut_NumEndMethodLabelsPassed >= 2)
                {
                    mASMSteppingOut = false;
                }
                new System.Threading.Tasks.Task(() =>
                {
                    mDbgConnector.SendCmd(Vs2Ds.AsmStepInto);
                }).Start();
            }
            else
            {
                bool fullUpdate = true;


                var xActionPoints = new List<object>();
                var xBoundBreakpoints = new List<IDebugBoundBreakpoint2>();

                if (!mBreaking)
                {
                    // Search the BPs and find ones that match our address.
                    foreach (var xBP in mEngine.BPMgr.mPendingBPs)
                    {
                        foreach (var xBBP in xBP.mBoundBPs)
                        {
                            if (xBBP.mAddress == aAddress)
                            {
                                xBoundBreakpoints.Add(xBBP);
                            }
                        }
                    }
                }

                mStackFrame = null;
                mCurrentAddress = aAddress;
                mCurrentASMLine = null;
                if (xBoundBreakpoints.Count == 0)
                {
                    // If no matching breakpoints are found then its one of the following:
                    //   - VS Break
                    //   - Stepping operation
                    //   - Asm break


                    //We _must_ respond to the VS commands via callback if VS is waiting on one so check this first...
                    if (mBreaking)
                    {
                        mCallback.OnBreak(mThread);
                        mBreaking = false;
                    }
                    else if (mStepping)
                    {
                        mCallback.OnStepComplete();
                        mStepping = false;
                    }
                    else
                    {
                        //Check if current address is the ASM BP we might be looking for
                        if (ASMBPToStepTo != null && ASMBPToStepTo.Item2 == aAddress)
                        {
                            //There is an ASM BP at this address so break
                            mCallback.OnBreak(mThread);
                            //Clear what we are stepping towards
                            ASMBPToStepTo = null;
                        }
                        else
                        {
                            fullUpdate = false;

                            //Check we aren't already stepping towards an ASM BP
                            if (ASMBPToStepTo == null)
                            {
                                //Check for future ASM breakpoints...

                                //Since we got this far, we know this must be an INT3 for a future ASM BP that has to be in current C# line.
                                //So get the ASM BP based off current address
                                var bp = GetASMBreakpointInfoFromCSAddress(aAddress).First();
                                //Set it as address we are looking for
                                ASMBPToStepTo = bp;
                            }

                            //Step towards the ASM BP(step-over since we don't want to go through calls or anything)

                            //We must check we haven't just stepped and address jumped wildely out of range (e.g. conditional jumps)
                            if (aAddress < ASMBPToStepTo.Item1 || aAddress > ASMBPToStepTo.Item2)
                            {
                                //If we have, just continue execution as this BP won't be hit.
                                mDbgConnector.Continue();
                                ASMBPToStepTo = null;
                            }
                            else
                            {
                                //We must do an update of ASM window so Step-Over can function properly
                                SendAssembly(true);
                                //Delay / wait for asm window to update
                                WaitForAssemblyUpdate();
                                //Do the step-over
                                ASMStepOver();
                            }
                        }
                    }
                }
                else
                {
                    // Found a bound breakpoint
                    mCallback.OnBreakpoint(mThread, xBoundBreakpoints.AsReadOnly());
                }
                if (fullUpdate)
                {
                    RequestFullDebugStubUpdate();
                }
            }
        }

        protected void RequestFullDebugStubUpdate()
        {
            // We catch and resend data rather than using a second serial port because
            // while this would work fine in a VM, it would require 2 serial ports
            // when real hardware is used.
            new System.Threading.Tasks.Task(() =>
            {
                SendAssembly();
                mDbgConnector.SendRegisters();
                mDbgConnector.SendFrame();
                mDbgConnector.SendStack();
            }).Start();
        }

        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
            Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
            throw new NotImplementedException();
        }

        public int CanDetach()
        {
            throw new NotImplementedException();
        }

        bool mBreaking = false;
        public int CauseBreak()
        {
            mBreaking = true;
            mDbgConnector.SendCmd(Vs2Ds.Break);
            return VSConstants.S_OK;
        }

        public int Detach()
        {
            throw new NotImplementedException();
        }

        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum)
        {
            throw new NotImplementedException();
        }

        public int EnumThreads(out IEnumDebugThreads2 ppEnum)
        {
            var xEnum = new AD7ThreadEnum(new IDebugThread2[] { mThread });
            ppEnum = xEnum;
            return VSConstants.S_OK;
        }

        public int GetAttachedSessionName(out string pbstrSessionName)
        {
            throw new NotImplementedException();
        }

        public int GetInfo(enum_PROCESS_INFO_FIELDS Fields, PROCESS_INFO[] pProcessInfo)
        {
            throw new NotImplementedException();
        }

        public int GetName(enum_GETNAME_TYPE gnType, out string pbstrName)
        {
            throw new NotImplementedException();
        }

        public readonly Guid PhysID = Guid.NewGuid();
        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            // http://blogs.msdn.com/b/jacdavis/archive/2008/05/01/what-to-do-if-your-debug-engine-doesn-t-create-real-processes.aspx
            // http://social.msdn.microsoft.com/Forums/en/vsx/thread/fe809686-e5f9-439d-9e52-00017e12300f
            pProcessId[0].guidProcessId = PhysID;
            pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_GUID;

            return VSConstants.S_OK;
        }

        private IDebugPort2 mPort = null;
        public int GetPort(out IDebugPort2 ppPort)
        {
            if (mPort == null)
            {
                throw new Exception("Error");
            }
            ppPort = mPort;
            return VSConstants.S_OK;
        }

        public int GetProcessId(out Guid pguidProcessId)
        {
            pguidProcessId = ID;
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            throw new NotImplementedException();
        }

        public int Terminate()
        {
            OutputText("Debugger terminating.");

            try
            {
                mHost.Stop();

                OutputText("Debugger terminated.");
                return VSConstants.S_OK;
            }
            catch (ApplicationException)
            {
                OutputText("Failed to stop debugger!");
                OutputText("\r\n");
                OutputText("You need to install the VMWare VIX API!");

                AD7Util.MessageBox("Failed to stop debugger! You need to install the VMWare VIX API!", "Information");
            }
            catch (Exception ex)
            {
                OutputText("Failed to stop debugger! Exception message: " + ex.Message);
                OutputText("\r\n");
                OutputText("You probably need to install the VMWare VIX API!");

                AD7Util.MessageBox(
                    "Failed to stop debugger! You probably need to install the VMWare VIX API!\r\n\r\nCheck Output window for more details.",
                    "Information");
            }

            return VSConstants.E_FAIL;
        }

        internal void ResumeFromLaunch()
        {
            mHost.Start();
        }

        void HostShutdown(object sender, EventArgs e)
        {
            //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
            //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);

            // We dont use process info any more, but have to call this to tell
            // VS to stop debugging.
            if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 0)
            {
                try
                {
                    mCallback.OnProcessExit(0);
                }
                catch
                {
                    // swallow exceptions here?
                }
            }

            if (mDbgConnector != null)
            {
                mDbgConnector.Dispose();
                mDbgConnector = null;
            }
            if (mDebugInfoDb != null)
            {
                // Commented for debugging, so we can look at the DB after
                //mDebugInfoDb.DeleteDB();
                mDebugInfoDb.Dispose();
                mDebugInfoDb = null;
            }
        }

        internal void Continue()
        { // F5
            ClearINT3sOnCurrentMethod();

            //Check for a future asm BP on current line
            //If there is, don't do continue, do AsmStepOver

            // The current address may or may not be a C# line due to asm stepping
            //So get the C# INT3 address
            uint csAddress = mDebugInfoDb.GetClosestCSharpBPAddress(mCurrentAddress.Value);
            //Get any Asm BPs for this address
            var bps = GetASMBreakpointInfoFromCSAddress(csAddress).Where(x => x.Item2 > mCurrentAddress.Value).ToList();
            //If there are any, do AsmStepOver on the next one after current address
            if (bps.Count > 0)
            {
                var bp = bps.OrderBy(x => x.Item2).First();
                ASMBPToStepTo = bp;

                ASMStepOver();

                mCurrentAddress = null;
                mCurrentASMLine = null;
            }
            else
            {
                mCurrentAddress = null;
                mCurrentASMLine = null;

                mDbgConnector.Continue();
            }
        }

        bool mStepping = false;
        internal void Step(enum_STEPKIND aKind)
        {
            if (aKind == enum_STEPKIND.STEP_INTO)
            { // F11
                mStepping = true;
                if (ASMSteppingMode)
                {
                    mDbgConnector.SendCmd(Vs2Ds.AsmStepInto);
                    mDbgConnector.SendRegisters();
                }
                else
                {
                    SetINT3sOnCurrentMethod();
                    mDbgConnector.SendCmd(Vs2Ds.StepInto);
                }
            }
            else if (aKind == enum_STEPKIND.STEP_OVER)
            { // F10
                mStepping = true;
                if (ASMSteppingMode)
                {
                    ASMStepOver();
                }
                else
                {
                    SetINT3sOnCurrentMethod();
                    mDbgConnector.SendCmd(Vs2Ds.StepOver);
                }
            }
            else if (aKind == enum_STEPKIND.STEP_OUT)
            { // Shift-F11
                ClearINT3sOnCurrentMethod();

                mStepping = true;
                if (ASMSteppingMode)
                {
                    mASMSteppingOut = true;
                    mASMSteppingOut_NumEndMethodLabelsPassed = 0;
                    mDbgConnector.SendCmd(Vs2Ds.AsmStepInto);

                    //Set a condition to say we should be doing step out
                    //On break, check line just stepped.

                    //If current line is RET - do one more step then break.
                    //Else do another step
                }
                else
                {
                    mDbgConnector.SendCmd(Vs2Ds.StepOut);
                }
            }
            else if (aKind == enum_STEPKIND.STEP_BACKWARDS)
            {
                // STEP_BACKWARDS - Supported at all by VS?
                //
                // Possibly, by dragging the execution location up
                // or down through the source code? -Orvid
                AD7Util.MessageBox("Step backwards is not supported.");
                mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"
            }
            else
            {
                AD7Util.MessageBox("Unknown step type requested.");
                mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"
            }
        }
        internal void SetINT3sOnCurrentMethod()
        {
            //Set all the CS Tracepoints to INT3 but without setting them like BPs
            //Don't bother setting existing CS BPs

            ChangeINT3sOnCurrentMethod(false);
        }
        internal void ClearINT3sOnCurrentMethod()
        {
            //Clear all the CS Tracepoints to NOP but without treating them like BPs
            //Don't clear existing/actual CS BPs

            ChangeINT3sOnCurrentMethod(true);
        }
        public List<KeyValuePair<uint, string>> INT3sSet = new List<KeyValuePair<uint, string>>();

        internal void ChangeINT3sOnCurrentMethod(bool clear)
        {
            if (mCurrentAddress.HasValue)
            {
                var currMethod = mDebugInfoDb.GetMethod(mCurrentAddress.Value);
                //Clear out the full list so we don't accidentally accumulate INT3s all over the place
                //Or set INT3s for all places in current method
                var tpAdresses = clear ? new List<KeyValuePair<uint, string>>(INT3sSet.Count) : mDebugInfoDb.GetAllINT3AddressesForMethod(currMethod, true);
                //If we just do a stright assigment then we get a collection modified exception in foreach loop below
                if (clear)
                {
                    tpAdresses.AddRange(INT3sSet);
                }

                var bps = mEngine.BPMgr.mPendingBPs.Select(x => x.mBoundBPs).ToList();
                var bpAddressessUnified = new List<uint>();
                foreach (var bp in bps)
                {
                    bpAddressessUnified.AddRange(bp.Select(x => x != null ? x.mAddress : 0));
                }
                bpAddressessUnified.AddRange(mEngine.BPMgr.mActiveBPs.Select(x => x != null ? x.mAddress : 0));

                foreach (var addressInfo in tpAdresses)
                {
                    var address = addressInfo.Key;

                    //Don't set/clear actual BPs
                    if (!bpAddressessUnified.Contains(address))
                    {
                        int index = INT3sSet.FindIndex(x => x.Key == address);
                        bool set = index != -1;

                        if (clear && set)
                        {
                            //Clear the INT3
                            mDbgConnector.ClearINT3(address);
                            INT3sSet.RemoveAt(index);
                        }
                        else if (!clear && !set)
                        {
                            //Set the INT3
                            mDbgConnector.SetINT3(address);
                            INT3sSet.Add(addressInfo);
                        }
                    }
                }
            }
        }

        internal void ASMStepOver()
        {
            //ASM Step over : Detect calls and treat them specially.
            //If current line has been stepped, get next line (since that is what we will step over)
            //If current line hasn't been stepped, use current line

            //If current line is CALL, set INT3 on next line and do continue
            //Else do asm step-into

            string currentASMLine = mCurrentASMLine;

            if (String.IsNullOrEmpty(currentASMLine))
            {
                mDbgConnector.SendCmd(Vs2Ds.AsmStepInto);
            }
            else
            {
                currentASMLine = currentASMLine.Trim();
                string currentASMOp = currentASMLine.Split(' ')[0].ToUpper();
                if (currentASMOp == "CALL")
                {
                    //Get the line after the call
                    string nextASMLine = mNextASMLine1;
                    uint? nextAddress = mNextAddress1;
                    if (String.IsNullOrEmpty(nextASMLine) || !nextAddress.HasValue)
                    {
                        mDbgConnector.SendCmd(Vs2Ds.AsmStepInto);
                    }
                    else
                    {
                        //Set the INT3 at next address
                        mDbgConnector.SetAsmBreakpoint(nextAddress.Value);
                        mDbgConnector.Continue();
                    }
                }
                else
                {
                    mDbgConnector.SendCmd(Vs2Ds.AsmStepInto);
                }
            }
        }

        public void SendAssembly(bool noDisplay = false)
        {
            AD7Util.Log("SendAssembly");
            ASMWindow_CurrentLineUpdated.Reset();
            ASMWindow_NextAddress1Updated.Reset();
            ASMWindow_NextLine1Updated.Reset();

            uint xAddress = mCurrentAddress.Value;
            var xSourceInfos = mDebugInfoDb.GetSourceInfos(xAddress);
            AD7Util.Log("SendAssembly - SourceInfos retrieved for address 0x{0}", xAddress.ToString("X8"));
            if (xSourceInfos.Count > 0)
            {
                //We should be able to display the asesembler source for any address regardless of whether a C#
                //line is associated with it.
                //However, we do not store all labels in the debug database because that would make the compile
                //time insane.
                //So:
                // - We take the current address amd find the method it is part of
                // - We use the method header label as a start point and find all asm labels till the method footer label
                // - We then find all the asm for these labels and display it.

                Label[] xLabels = mDebugInfoDb.GetMethodLabels(xAddress);
                AD7Util.Log("SendAssembly - MethodLabels retrieved");
                // get the label of our current position, or the closest one before
                var curPosLabel = xLabels.Where(i => i.Address <= xAddress).OrderByDescending(i => i.Address).FirstOrDefault();
                // if curPosLabel is null, grab the first one.
                if (curPosLabel == null)
                {
                    curPosLabel = xLabels[0];
                }

                var curPosIndex = Array.IndexOf(xLabels, curPosLabel);
                // we want 50 items before and after the current item, so 100 in total.
                var itemsBefore = 10;
                var itemsAfter = 10;

                if (curPosIndex < itemsBefore)
                {
                    // there are no 50 items before the current one, so adjust
                    itemsBefore = curPosIndex;
                }
                if ((curPosIndex + itemsAfter) >= xLabels.Length)
                {
                    // there are no 50 items after the current one, so adjust
                    itemsAfter = xLabels.Length - curPosIndex;
                }

                var newArr = new Label[itemsBefore + itemsAfter];
                for (int i = 0; i < newArr.Length; i++)
                {
                    newArr[i] = xLabels[(curPosIndex - itemsBefore) + i];
                }
                xLabels = newArr;

                //The ":" has to be added in because labels in asm code have it on the end - it's easier to add it here than
                //strip them out of the read asm
                var xLabelNames = xLabels.Select(x => x.Name + ":").ToList();

                // Get assembly source
                var xCode = AsmSource.GetSourceForLabels(Path.ChangeExtension(mISO, ".asm"), xLabelNames);
                AD7Util.Log("SendAssembly - SourceForLabels retrieved");

                // Get label for current address.
                // A single address can have multiple labels (IL, Asm). Because of this we search
                // for the one with the Asm tag. We dont have the tags in this debug info though,
                // so instead if there is more than one label we use the longest one which is the Asm tag.
                string xCurrentLabel = "";
                var xCurrentLabels = mDebugInfoDb.GetLabels(xAddress);
                if (xCurrentLabels.Length > 0)
                {
                    xCurrentLabel = xCurrentLabels.OrderBy(q => q.Length).Last();
                }
                if (String.IsNullOrEmpty(xCurrentLabel))
                {
                    xCurrentLabel = "NO_METHOD_LABEL_FOUND";
                }

                // Insert filter labels list as THIRD(!) line of our data stream
                string filterLabelsList = "";
                foreach (var addressInfo in INT3sSet)
                {
                    //"We have to add the ".00:" because of how the ASM window works...
                    filterLabelsList += "|" + addressInfo.Value + ".00";
                }
                if (filterLabelsList.Length > 0)
                {
                    filterLabelsList = filterLabelsList.Substring(1);
                }
                xCode.Insert(0, filterLabelsList + "\r\n");
                // Insert parameters as SECOND(!) line of our data stream
                xCode.Insert(0, (noDisplay ? "NoDisplay" : "") + "|" + (ASMSteppingMode ? "AsmStepMode" : "") + "\r\n");
                // Insert current line's label as FIRST(!) line of our data stream
                xCode.Insert(0, xCurrentLabel + "\r\n");
                //THINK ABOUT THE ORDER that he above lines occur in and where they insert data into the stream - don't switch it!
                AD7Util.Log("SendAssembly - Sending through pipe now");
                mDebugDownPipe.SendCommand(Debugger2Windows.AssemblySource, Encoding.UTF8.GetBytes(xCode.ToString()));
                AD7Util.Log("SendAssembly - Done");
            }
        }
        public void WaitForAssemblyUpdate()
        {
            ASMWindow_CurrentLineUpdated.WaitOne(5000);
            ASMWindow_NextAddress1Updated.WaitOne(5000);
            ASMWindow_NextLine1Updated.WaitOne(5000);
        }

        //TODO: At some point this will probably need to be exposed for access outside of AD7Process
        protected void OutputText(string aText)
        {
            // With Bochs this method may be invoked before the pipe is created.
            if (null == mDebugDownPipe) { return; }
            mDebugDownPipe.SendCommand(Debugger2Windows.OutputPane, Encoding.UTF8.GetBytes(aText + "\r\n"));
        }

        protected void OutputClear()
        {
            // With Bochs this method may be invoked before the pipe is created.
            if (null == mDebugDownPipe) { return; }
            mDebugDownPipe.SendCommand(Debugger2Windows.OutputClear);
        }
    }
}
