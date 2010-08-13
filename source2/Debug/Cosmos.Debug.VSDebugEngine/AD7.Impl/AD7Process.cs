using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.IO;
using System.Collections.Specialized;
using Cosmos.Debug.Common.CDebugger;
using Cosmos.Compiler.Debug;
using Cosmos.Debug.Common;
using Cosmos.Build.Common;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

namespace Cosmos.Debug.VSDebugEngine {
    public class AD7Process : IDebugProcess2 {
        public Guid mID = Guid.NewGuid();
        protected Process mProcess;
        protected ProcessStartInfo mProcessStartInfo;
        protected EngineCallback mCallback;
        public AD7Thread mThread;
        protected AD7Engine mEngine;
        public DebugConnector mDbgConnector;
        public ReverseSourceInfos mReverseSourceMappings;
        public SourceInfos mSourceMappings;
        public uint? mCurrentAddress = null;
        protected readonly NameValueCollection mDebugInfo;
        protected TargetHost mTargetHost;
        protected VMwareFlavor mVMWareFlavor=VMwareFlavor.Player;

        private int mProcessExitEventSent = 0;

        protected void LaunchVMWare(bool aGDB) {
            string xPath = Path.Combine(PathUtilities.GetBuildDir(), @"VMWare\Workstation") + @"\";
            
            using (var xSrc = new StreamReader(xPath + "Cosmos.vmx")) {
                // This copy process also leaves the VMX writeable. VMWare doesnt like them read only.
                using (var xDest = new StreamWriter(xPath + "Debug.vmx")) {
                   string xLine;
                   while ((xLine = xSrc.ReadLine()) != null) {
                       var xParts = xLine.Split('=');
                       if (xParts.Length == 2) {
                           string xName = xParts[0].Trim();
                           string xValue = xParts[1].Trim();

                           // We delete uuid entries so VMWare doenst ask the user "Did you move or copy" the file
                           if ((xName == "uuid.location") || (xName == "uuid.bios")) {
                               xValue = null;
                           } else if (xName == "ide1:0.fileName") {
                               xValue = "\"" + mDebugInfo["ISOFile"] + "\"";
                           }

                           if (xValue != null) {
                               xDest.WriteLine(xName + " = " + xValue);
                           }
                       }
                   }
                   if (aGDB) {
                       xDest.WriteLine();
                       xDest.WriteLine("debugStub.listen.guest32 = \"TRUE\"");
                       xDest.WriteLine("debugStub.hideBreakpoints = \"TRUE\"");
                       xDest.WriteLine("monitor.debugOnStartGuest32 = \"TRUE\"");
                       xDest.WriteLine("debugStub.listen.guest32.remote = \"TRUE\"");
                   }
                }
            }

            //TODO: Find this in code. This is hardcoded to default location right now.
            //string xVmwPath = @"C:\Program Files (x86)\VMware\VMware Workstation\";
            string xVmwPath;
            switch (mVMWareFlavor)
            {
                case VMwareFlavor.Workstation:
                    xVmwPath = GetVMWareWorkstationPath();
                    mProcessStartInfo.Arguments = "false \"" + xVmwPath + "\" -x -q \"" + xPath + "Debug.vmx\"";
                    break;
                case VMwareFlavor.Player:
                    xVmwPath = GetVMWarePlayerPath();
                    mProcessStartInfo.Arguments = "false \"" + xVmwPath + "\" \"" + xPath + "Debug.vmx\"";
                    break;
                default:
                    throw new NotImplementedException("VMWare flavor '" + mVMWareFlavor.ToString() + "' not implemented!");
            }
            //mProcessStartInfo.Arguments = "true \"" + xPath + "Debug.vmx\" -x -q";
            // -x: Auto power on VM. Must be small x, big X means something else.
            // -q: Close VMWare when VM is powered off.
            // Options must come beore the vmx, and cannot use shellexecute

            if (String.IsNullOrEmpty(xVmwPath) || !File.Exists(xVmwPath))
            {
                MessageBox.Show("VWMare not installed, probably going to crash now!", "Cosmos DebugEngine", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private static string GetVMWareWorkstationPath()
        {
            using (var xRegKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VMware, Inc.\VMware Workstation", false))
            {
                if (xRegKey == null)
                {
                    return String.Empty;
                }
                return Path.Combine(((string)xRegKey.GetValue("InstallPath")), "vmware.exe");
            }
        }

        private static string GetVMWarePlayerPath()
        {
            using (var xRegKey = Registry.LocalMachine.OpenSubKey(@"Software\VMware, Inc.\VMware Player", false))
            {
                if (xRegKey == null)
                {
                    return String.Empty;
                }
                return Path.Combine(((string)xRegKey.GetValue("InstallPath")), "vmplayer.exe");
            }
        }

        public string mISO;
        public string mProjectFile;
        internal AD7Process(NameValueCollection aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort)
        {
            mCallback = aCallback; 

            // Load passed in values
            mDebugInfo = aDebugInfo;
            
            //
            mISO = mDebugInfo["ISOFile"];
            mProjectFile = mDebugInfo["ProjectFile"];
            //
            var xGDBDebugStub = false;
            Boolean.TryParse(mDebugInfo["EnableGDB"], out xGDBDebugStub);
            //
            var xGDBClient = false;
            Boolean.TryParse(mDebugInfo["StartCosmosGDB"], out xGDBClient);

            mProcessStartInfo = new ProcessStartInfo(Path.Combine(PathUtilities.GetVSIPDir(), "Cosmos.Debug.HostProcess.exe"));
            if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "VMWare")) {
                mTargetHost = TargetHost.VMWare;
                if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["VMWareFlavor"], "Player"))
                {
                    mVMWareFlavor = VMwareFlavor.Player;
                }
                else if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["VMWareFlavor"], "Workstation"))
                {
                    mVMWareFlavor = VMwareFlavor.Workstation;
                }
                else
                {
                    throw new Exception("VMWare Flavor '" + mDebugInfo["VMWareFlavor"] + "' not implemented!");
                }
                LaunchVMWare(xGDBDebugStub);
            } else {
                throw new Exception("Invalid BuildTarget value: '" + mDebugInfo["BuildTarget"] + "'!");
            }

            mProcessStartInfo.UseShellExecute = false;
            mProcessStartInfo.RedirectStandardInput = true;
            mProcessStartInfo.RedirectStandardError = true;
            mProcessStartInfo.RedirectStandardOutput = true;
            mProcessStartInfo.CreateNoWindow = true;

            IDictionary<uint, string> xAddressLabelMappings;
            IDictionary<string, uint> xLabelAddressMappings;
            Cosmos.Debug.Common.CDebugger.SourceInfo.ReadFromFile(Path.ChangeExtension(mISO, "cmap"), out xAddressLabelMappings, out xLabelAddressMappings);
            if (xAddressLabelMappings.Count == 0)
            {
                throw new Exception("Debug data not found: LabelByAddressMapping");
            }
            
            //TODO: This next line takes a long time. See if we can speed it up.
            var xSW = new Stopwatch();
            xSW.Start();
            mSourceMappings = Cosmos.Debug.Common.CDebugger.SourceInfo.GetSourceInfo(xAddressLabelMappings, xLabelAddressMappings, Path.ChangeExtension(mISO, ".cxdb"));
            xSW.Stop();

            Trace.WriteLine("GetSourceInfo took: " + xSW.Elapsed);
            if (mSourceMappings.Count == 0) {
                throw new Exception("Debug data not found: SourceMappings");
            }
            mReverseSourceMappings = new ReverseSourceInfos(mSourceMappings);
            
            if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "vmware")) {
                mDbgConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorPipeServer();
            } else {
                throw new Exception("BuildTarget value not valid: '" + mDebugInfo["BuildTarget"] + "'!");
            }
            aEngine.BPMgr.SetDebugConnector(mDbgConnector);

            mDbgConnector.CmdTrace += new Action<Cosmos.Compiler.Debug.MsgType, uint>(DbgCmdTrace);
            mDbgConnector.CmdText += new Action<string>(DbgCmdText);
            mDbgConnector.CmdStarted += new Action(DbgCmdStarted);
            mDbgConnector.OnDebugMsg += new Action<string>(DebugMsg);
            mDbgConnector.ConnectionLost = new Action<Exception>(DbgConnector_ConnectionLost);

            System.Threading.Thread.Sleep(250);
            System.Diagnostics.Debug.WriteLine(String.Format("Launching process: \"{0}\" {1}", mProcessStartInfo.FileName, mProcessStartInfo.Arguments).Trim());
            mProcess = Process.Start(mProcessStartInfo);

            mProcess.EnableRaisingEvents = true;
            mProcess.Exited += new EventHandler(mProcess_Exited);

            // Sleep 250 and see if it exited too quickly. Why do we do this? We have .Exited hooked. Is this in case it happens between start and hook?
            // if so, why not hook before start? 
            // MtW: we do this for the potential situation where it might exit before the Exited event is hooked. Iirc i had this situation before..
            System.Threading.Thread.Sleep(250);
            if (mProcess.HasExited) {
                Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
                Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
                Trace.WriteLine("ExitCode: " + mProcess.ExitCode);
                throw new Exception("Error while starting application");
            }

            mEngine = aEngine;
            mThread = new AD7Thread(aEngine, this);
            mCallback.OnThreadStart(mThread);
            mPort = aPort;

            // Launch GDB Client
            if (xGDBDebugStub && xGDBClient) {
                // TODO: Need to integrate the GDB client to the build
                // But allow overrides for dev kit, I dont want to have to run the install 
                // for each change to gdb client.
                string xGDBClientEXE = @"m:\source\Cosmos\source2\Debug\Cosmos.Debug.GDB\bin\Debug\Cosmos.Debug.GDB.exe";
                if (File.Exists(xGDBClientEXE)) {
                    var xPSInfo = new ProcessStartInfo(xGDBClientEXE);
                    xPSInfo.Arguments = Path.ChangeExtension(mProjectFile, ".cgdb") + @" /Connect";
                    xPSInfo.UseShellExecute = false;
                    xPSInfo.RedirectStandardInput = false;
                    xPSInfo.RedirectStandardError = false;
                    xPSInfo.RedirectStandardOutput = false;
                    xPSInfo.CreateNoWindow = false;
                    Process.Start(xPSInfo);
                }
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
        public void DebugMsg(string aMsg) {
            mCallback.OnOutputString(aMsg + "\n");
        }

        protected void DbgCmdStarted() {
            DebugMsg("RmtDbg: Started");
            
            // OK, now debugger is ready. Send it a list of breakpoints that were set before
            // program run.
            foreach (var xBP in mEngine.BPMgr.mPendingBPs) {
                foreach (var xBBP in xBP.mBoundBPs) {
                    mDbgConnector.SetBreakpoint(xBBP.RemoteID, xBBP.mAddress);
                }
            }
            mDbgConnector.SendCommand(Command.BatchEnd);
        }

        void DbgCmdText(string obj) {
            mCallback.OnOutputString(obj + "\r\n");
        }

        internal AD7Thread Thread {
            get {
                return mThread;
            }
        }

        void DbgCmdTrace(Cosmos.Compiler.Debug.MsgType arg1, uint arg2) {
            DebugMsg("DbgCmdTrace");
            switch (arg1) {
                case Cosmos.Compiler.Debug.MsgType.BreakPoint: {
                    // When doing a CALL, the return address is pushed, but that's the address of the next instruction, after CALL. call is 5 bytes (for now?)
                    // Dont need to correct the address, becuase DebugStub does it for us.
                    var xActualAddress = arg2; 
                    DebugMsg("BP hit @ " + xActualAddress.ToString("X8").ToUpper());

                    var xActionPoints = new List<object>();
                    var xBoundBreakpoints = new List<IDebugBoundBreakpoint2>();
                    
                    // Search the BPs and find the ones that match our address
                    foreach (var xBP in mEngine.BPMgr.mPendingBPs) {
                        foreach (var xBBP in xBP.mBoundBPs) { 
                            if (xBBP.mAddress == xActualAddress) {
                                xBoundBreakpoints.Add(xBBP);
                            }
                        }
                    }

                    mCurrentAddress = xActualAddress;
                    // if no matching breakpoint, its either a stepping operation, or a code based break
                    if (xBoundBreakpoints.Count == 0) {
                        // Is it a result of stepping operation?
                        if (mEngine.AfterBreak) {
                            mCallback.OnStepComplete();
                        } else {
                            // Code based break. Tell VS to break.
                            mCallback.OnBreakpoint(mThread, new ReadOnlyCollection<IDebugBoundBreakpoint2>(xBoundBreakpoints));
                        }
                    } else {
                        // Found a bound breakpoint
                        mCallback.OnBreakpoint(mThread, new ReadOnlyCollection<IDebugBoundBreakpoint2>(xBoundBreakpoints));
                        mEngine.AfterBreak = true;
                    }
                    break;
                }

                default: {
                    DebugMsg("TraceReceived: " + arg1);
                    break;
                }
            }
        }

        #region IDebugProcess2 Members

        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach) {
            Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
            throw new NotImplementedException();
        }

        public int CanDetach() {
            throw new NotImplementedException();
        }

        public int CauseBreak() {
            throw new NotImplementedException();
        }

        public int Detach() {
            throw new NotImplementedException();
        }

        public int EnumPrograms(out IEnumDebugPrograms2 ppEnum) {
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

        public int GetInfo(uint Fields, PROCESS_INFO[] pProcessInfo)
        {                  throw new NotImplementedException();
        }

        public int GetName(uint gnType, out string pbstrName)
        {
            throw new NotImplementedException();
        }

        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
            pProcessId[0].dwProcessId = (uint)mProcess.Id;
            pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
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
            Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
            pguidProcessId = mID;
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            throw new NotImplementedException();
        }

        public int Terminate()
        {
            if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 0)
            {
                mProcess.Kill();
                mProcess.Exited -= mProcess_Exited;
                if (mDbgConnector != null)
                {
                    mDbgConnector.Dispose();
                    mDbgConnector = null;
                }
            }
            return VSConstants.S_OK;
        }

        #endregion

        internal void ResumeFromLaunch()
        {
            // This unpauses our debug host
            // We do this because VS requires a start, and then a resume after. So we have debughost which is a stub
            // that allows VS to "see" that. Here we resume it.
            mProcess.StandardInput.WriteLine();
        }

        void mProcess_Exited(object sender, EventArgs e) {
            Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
            Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
            Trace.WriteLine(String.Format("Process Exit Code: {0}", mProcess.ExitCode));
            //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
            //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);
            mDbgConnector.Dispose();
            mDbgConnector = null;
            if (Interlocked.CompareExchange(ref mProcessExitEventSent, 1, 0) == 0)
            {
                mCallback.OnProcessExit((uint)mProcess.ExitCode);
            }
        }

        internal void Continue() {
            // F5
            mCurrentAddress = null;
            mDbgConnector.SendCommand(Command.Continue);
        }

        internal void Step(enum_STEPKIND aKind) {
            // F11: STEP_INTO
            // F10: STEP_OVER
            // Shift-F11: STEP_OUT - Doesnt appear in the menus?
            // STEP_BACKWARDS - Supported at all by VS?
            if (aKind == enum_STEPKIND.STEP_INTO) {
                mDbgConnector.SendCommand(Command.Step);
            } else {
                MessageBox.Show("Currently only Trace Into (F11) is supported.");
                // Have to call this otherwise VS gets "stuck"
                mCallback.OnStepComplete();
            }
        }
    }
}
