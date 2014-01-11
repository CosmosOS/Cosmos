using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Cosmos.Build.Common;
using Cosmos.Debug.Common;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.Win32;

namespace Cosmos.Debug.VSDebugEngine
{
    public class AD7Process : IDebugProcess2
    {
        private bool ASMSteppingMode = false;

        public Guid ID = Guid.NewGuid();
        protected EngineCallback mCallback;
        public AD7Thread mThread;
        protected AD7Engine mEngine;
        public UInt32? mCurrentAddress = null;
        public UInt32? mNextAddress1 = null;
        public string mCurrentASMLine = null;
        public string mNextASMLine1 = null;
        protected readonly NameValueCollection mDebugInfo;
        protected LaunchType mLaunch;
        internal DebugInfo mDebugInfoDb;
        protected int mProcessExitEventSent = 0;
        // Cached stack frame. See comments in AD7Thread regading this.
        public IEnumDebugFrameInfo2 mStackFrame;
        private bool mASMSteppingOut = false;
        private int mASMSteppingOut_NumEndMethodLabelsPassed = 0;

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

        Host.Base mHost;

        public string mISO;
        public string mProjectFile;

        protected void DbgCmdRegisters(byte[] aData)
        {
            mDebugDownPipe.SendCommand(Debugger2Windows.Registers, aData);
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

        private void mDebugUpPipe_DataPacketReceived(byte aCmd, byte[] aData)
        {
            try
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
                            UInt32 xAddress = mDebugInfoDb.AddressOfLabel(xLabel);
                            mDbgConnector.SetAsmBreakpoint(xAddress);
                            mDbgConnector.Continue();
                        }
                        break;

                    case Windows2Debugger.Continue:
                        {
                            mDbgConnector.Continue();
                        }
                        break;

                    case Windows2Debugger.ToggleStepMode:
                        ASMSteppingMode = !ASMSteppingMode;
                        break;

                    case Windows2Debugger.CurrentASMLine:
                        {
                            mCurrentASMLine = Encoding.UTF8.GetString(aData);
                        }
                        break;
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
                            }
                        }
                        break;
                    case Windows2Debugger.NextLabel1:
                        {
                            string nextLabel = Encoding.UTF8.GetString(aData);
                            mNextAddress1 = mDebugInfoDb.AddressOfLabel(nextLabel);
                        }
                        break;

                    default:
                        throw new Exception(String.Format("Command value '{0}' not supported in method AD7Process.mDebugUpPipe_DataPacketReceived.", aCmd));
                }
            }
            catch(Exception ex)
            {
                //We cannot afford to silently break the pipe!
                OutputText("AD7Process UpPipe receive error! " + ex.Message);
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

            string xPort = mDebugInfo[BuildProperties.VisualStudioDebugPortString];
            var xParts = (null == xPort) ? null : xPort.Split(' ');
            if ((null == xParts) || (2 > xParts.Length))
            {
                throw new Exception(string.Format(
                    "The '{0}' Cosmos project file property is either ill-formed or missing.",
                    BuildProperties.VisualStudioDebugPortString));
            }
            string xPortType = xParts[0].ToLower();
            string xPortParam = xParts[1].ToLower();

            OutputText("Starting debug connector.");
            if (xPortType == "pipe:")
            {
                mDbgConnector = new Cosmos.Debug.Common.DebugConnectorPipeServer(xPortParam);
            }
            else if (xPortType == "serial:")
            {
                mDbgConnector = new Cosmos.Debug.Common.DebugConnectorSerial(xPortParam);
            }

            if (mDbgConnector == null)
            {
                throw new Exception("No debug connector found.");
            }
            mDbgConnector.SetConnectionHandler(DebugConnectorConnected);
            mDbgConnector.CmdBreak += new Action<UInt32>(DbgCmdBreak);
            mDbgConnector.CmdTrace += new Action<UInt32>(DbgCmdTrace);
            mDbgConnector.CmdText += new Action<string>(DbgCmdText);
            mDbgConnector.CmdStarted += new Action(DbgCmdStarted);
            mDbgConnector.OnDebugMsg += new Action<string>(DebugMsg);
            mDbgConnector.ConnectionLost += new Action<Exception>(DbgConnector_ConnectionLost);
            mDbgConnector.CmdRegisters += new Action<byte[]>(DbgCmdRegisters);
            mDbgConnector.CmdFrame += new Action<byte[]>(DbgCmdFrame);
            mDbgConnector.CmdStack += new Action<byte[]>(DbgCmdStack);
            mDbgConnector.CmdPong += new Action<byte[]>(DbgCmdPong);
        }

        internal AD7Process(NameValueCollection aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort)
        {
            mCallback = aCallback;
            mDebugInfo = aDebugInfo;

            mLaunch = (LaunchType)Enum.Parse(typeof(LaunchType), aDebugInfo[BuildProperties.LaunchString]);

            if (mDebugDownPipe == null)
            {
                mDebugDownPipe = new Cosmos.Debug.Common.PipeClient(Pipes.DownName);

                mDebugUpPipe = new Cosmos.Debug.Common.PipeServer(Pipes.UpName);
                mDebugUpPipe.DataPacketReceived += new Action<byte, byte[]>(mDebugUpPipe_DataPacketReceived);
                mDebugUpPipe.Start();
            }
            else
            {
                mDebugUpPipe.CleanHandlers();
                mDebugUpPipe.DataPacketReceived += new Action<byte, byte[]>(mDebugUpPipe_DataPacketReceived);
            }
            // Must be after mDebugDownPipe is initialized
            OutputClear();
            OutputText("Debugger process initialized.");

            mISO = mDebugInfo["ISOFile"];
            OutputText("Using ISO file " + mISO + ".");
            mProjectFile = mDebugInfo["ProjectFile"];
            //
            bool xUseGDB = string.Equals(mDebugInfo[BuildProperties.EnableGDBString], "true", StringComparison.InvariantCultureIgnoreCase);
            OutputText("GDB " + (xUseGDB ? "Enabled" : "Disabled") + ".");
            //
            var xGDBClient = false;
            Boolean.TryParse(mDebugInfo[BuildProperties.StartCosmosGDBString], out xGDBClient);

            switch (mLaunch)
            {
                case LaunchType.VMware:
                    mHost = new Host.VMware(mDebugInfo, xUseGDB);
                    break;
                case LaunchType.Slave:
                    mHost = new Host.Slave(mDebugInfo, xUseGDB);
                    break;
                case LaunchType.Bochs:
                    // The project has been created on another machine or Bochs has been uninstalled since the project has
                    // been created.
                    if (!BochsSupport.BochsEnabled) { throw new Exception(ResourceStrings.BochsIsNotInstalled); }
                    string bochsConfigurationFileName = mDebugInfo[BuildProperties.BochsEmulatorConfigurationFileString];
                    if (string.IsNullOrEmpty(bochsConfigurationFileName))
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
                    mHost = new Host.Bochs(mDebugInfo, xUseGDB, bochsConfigurationFile);
                    ((Host.Bochs)mHost).FixBochsConfiguration(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("IsoFileName", mISO) }
                    );
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
            if (File.Exists(Cosmos.Build.Common.CosmosPaths.GdbClientExe))
            {
                var xPSInfo = new ProcessStartInfo(Cosmos.Build.Common.CosmosPaths.GdbClientExe);
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
                MessageBox.Show(string.Format(
                    "The GDB-Client could not be found at \"{0}\". Please deactivate it under \"Properties/Debug/Enable GDB\"",
                    Cosmos.Build.Common.CosmosPaths.GdbClientExe), "GDB-Client", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
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
                    mDbgConnector.SetBreakpoint(xBBP.RemoteID, xBBP.mAddress);
                }
            }
            mDbgConnector.SendCmd(Vs2Ds.BatchEnd);
        }

        void DbgCmdText(string obj)
        {
            mCallback.OnOutputStringUser(obj + "\r\n");
        }

        internal AD7Thread Thread
        {
            get
            {
                return mThread;
            }
        }

        void DbgCmdTrace(UInt32 aAddress)
        {
            DebugMsg("TraceReceived: " + aAddress);
        }

        void DbgCmdBreak(UInt32 aAddress)
        {
            // aAddress will be actual address. Call and other methods push return to (after op), but DS
            // corrects for us and sends us actual op address.
            DebugMsg("DbgCmdBreak " + aAddress + " / " + aAddress.ToString("X8").ToUpper());

            if (mASMSteppingOut)
            {
                string[] currentASMLabels = mDebugInfoDb.GetLabels(aAddress);
                foreach (string aLabel in currentASMLabels)
                {
                    if(aLabel.Contains("END__OF__METHOD_EXCEPTION__2"))
                    {
                        mASMSteppingOut_NumEndMethodLabelsPassed++;
                        break;
                    }
                }
                if(mASMSteppingOut_NumEndMethodLabelsPassed >= 2)
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
                var xActionPoints = new List<object>();
                var xBoundBreakpoints = new List<IDebugBoundBreakpoint2>();

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

                mStackFrame = null;
                mCurrentAddress = aAddress;
                mCurrentASMLine = null;
                if (xBoundBreakpoints.Count == 0)
                {
                    // if no matching breakpoints are found then its one of the following:
                    //   - Stepping operation
                    //   - Code based break
                    //   - Asm stepping

                    if (mStepping)
                    {
                        mCallback.OnStepComplete();
                        mStepping = false;
                    }
                }
                else
                {
                    // Found a bound breakpoint
                    mCallback.OnBreakpoint(mThread, xBoundBreakpoints.AsReadOnly());
                }
                RequestFullDebugStubUpdate();
            }
        }

        protected void RequestFullDebugStubUpdate()
        {
            // We catch and resend data rather than using a second serial port because
            // while this would work fine in a VM, it would require 2 serial ports
            // when real hardware is used.
            SendAssembly();
            mDbgConnector.SendRegisters();
            mDbgConnector.SendFrame();
            mDbgConnector.SendStack();
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

        public int CauseBreak()
        {
            throw new NotImplementedException();
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
            catch (Exception ex)
            {
                OutputText("Failed to stop debugger! Exception message: " + ex.Message);
                OutputText("\r\n");
                OutputText("You probably need to install the VMWare VIX API!");

                MessageBox.Show("Failed to stop debugger! You probably need to install the VMWare VIX API! \r\n\r\nCheck Output window for more details.");
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
                mCallback.OnProcessExit(0);
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
            mCurrentAddress = null;
            mCurrentASMLine = null;
            mDbgConnector.Continue();
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
                }
                else
                {
                    mDbgConnector.SendCmd(Vs2Ds.StepInto);
                }
            }
            else if (aKind == enum_STEPKIND.STEP_OVER)
            { // F10
                mStepping = true;
                if (ASMSteppingMode)
                {
                    //ASM Step over : Detect calls and treat them specially.
                    //If current line has been stepped, get next line (since that is what we will step over)
                    //If current line hasn't been stepped, use current line

                    //If current line is CALL, set INT3 on next line and do continue
                    //Else do asm step-into

                    string currentASMLine = mCurrentASMLine;

                    if (string.IsNullOrEmpty(currentASMLine))
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
                            UInt32? nextAddress = mNextAddress1;
                            if (string.IsNullOrEmpty(nextASMLine) || !nextAddress.HasValue)
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
                else
                {
                    mDbgConnector.SendCmd(Vs2Ds.StepOver);
                }
            }
            else if (aKind == enum_STEPKIND.STEP_OUT)
            { // Shift-F11
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
                MessageBox.Show("Step backwards is not supported.");
                mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"
            }
            else
            {
                MessageBox.Show("Unknown step type requested.");
                mCallback.OnStepComplete(); // Have to call this otherwise VS gets "stuck"
            }
        }

        public void SendAssembly()
        {
            UInt32 xAddress = mCurrentAddress.Value;
            var xSourceInfos = mDebugInfoDb.GetSourceInfos(xAddress);
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

                var xLabels = mDebugInfoDb.GetMethodLabels(xAddress);
                //The ":" has to be added in because labels in asm code have it on the end - it's easier to add it here than
                //strip them out of the read asm
                var xLabelNames = xLabels.ToList().ConvertAll<string>(x => x.Name + ":").ToList();

                // Get assembly source
                var xCode = AsmSource.GetSourceForLabels(Path.ChangeExtension(mISO, ".asm"), xLabelNames);

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
                if (string.IsNullOrEmpty(xCurrentLabel))
                {
                    xCurrentLabel = "NO_METHOD_LABEL_FOUND";
                }

                // Insert it to the first line of our data stream
                xCode.Insert(0, xCurrentLabel + "\r\n");
                mDebugDownPipe.SendCommand(Debugger2Windows.AssemblySource, Encoding.UTF8.GetBytes(xCode.ToString()));

            }
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