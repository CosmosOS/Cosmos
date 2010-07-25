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

namespace Cosmos.Debug.VSDebugEngine {
    public class AD7Process : IDebugProcess2 {
        internal Guid mID = Guid.NewGuid();
        private Process mProcess;
        private ProcessStartInfo mProcessStartInfo;
        private EngineCallback mCallback;
        private AD7Thread mThread;
        private AD7Engine mEngine;
        private DebugConnector mDbgConnector;
        internal ReverseSourceInfos mReverseSourceMappings;
        internal SourceInfos mSourceMappings;
        internal uint? mCurrentAddress = null;
        internal string mISO;
        private readonly NameValueCollection mDebugInfo;
        protected TargetHost mTargetHost;

        protected void LaunchQEMU(bool aGDB) {
            var xDebugConnectorStr = "-serial tcp:127.0.0.1:4444";
            var xQT = "\"";

            // Start QEMU
            // QEMU Command Line docs: http://wiki.qemu.org/download/qemu-doc.html#sec_005finvocation
            // Here we actually call our dummy/proxy program (Cosmos.Debug.HostProcess.exe) which in turn calls QEMU.
            mProcessStartInfo.Arguments =
                "false" // Tells proxy to use ShellExecute or not (In this case, not, ie false)
                // Rest of arguments are used to launch another process and its arguments.
                + " " + xQT + Path.Combine(PathUtilities.GetQEmuDir(), "qemu.exe") + xQT // Program for our proxy to run
                + " -L " + xQT + PathUtilities.GetQEmuDir().Replace("\\", "/") + xQT // Directory for the BIOS, VGA BIOS and keymaps
                + " -cdrom " + xQT + mISO.Replace("\\", "/") + xQT // CDRom image
                + " -boot d" // Boot from the CDRom
                + " " + xDebugConnectorStr;

            if (aGDB) {
                mProcessStartInfo.Arguments
                    += " --gdb tcp::8832" // We now use 8832 to be same as VMWare
                    + "-S"; // Pause on startup, wait for GDB to connect and control
            }
            //#if VM_QEMU
            //    #if DEBUG_CONNECTOR_TCP_SERVER
            //                var xDebugConnectorStr = "-serial tcp:127.0.0.1:4444";
            //    #endif
            //    #if DEBUG_CONNECTOR_PIPE_CLIENT
            //                var xDebugConnectorStr = @"-serial pipe:CosmosDebug";
            //    #endif
            //    #if DEBUG_CONNECTOR_PIPE_SERVER
            //                var xDebugConnectorStr = @"-serial pipe:CosmosDebug";
            //    #endif
        }

        protected void LaunchVMWareWorkstation(bool aGDB) {
            //TODO: Change to use Cosmos path
            //TODO: App Roaming doesnt have the vmx.. need to update the insaller
            //string xPath = Path.Combine(PathUtilities.GetBuildDir(), @"VMWare\Workstation") + @"\";
            string xPath = @"M:\source\Cosmos\Build\VMWare\Workstation\";

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
                               //TODO: Update ISO to selected project
                               //xValue = @"m:\source\Cosmos\source2\Users\Kudzu\Breakpoints\bin\Debug\CosmosKernel.iso";
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

            mProcessStartInfo.Arguments = "true " + xPath + "Debug.vmx";
        }

        internal AD7Process(string aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort) {
            System.Diagnostics.Debug.WriteLine("Test message");
            mDebugInfo = new NameValueCollection();
            NameValueCollectionHelper.LoadFromString(mDebugInfo, aDebugInfo);

            mISO = mDebugInfo["ISOFile"];

            var xGDBDebugStub = false;
            Boolean.TryParse(mDebugInfo["EnableGDB"], out xGDBDebugStub);

            mProcessStartInfo = new ProcessStartInfo(Path.Combine(PathUtilities.GetVSIPDir(), "Cosmos.Debug.HostProcess.exe"));
            if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "qemu")) {
                mTargetHost = TargetHost.QEMU;
                LaunchQEMU(xGDBDebugStub);
            } else if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "VMWareWorkstation")) {
                mTargetHost = TargetHost.VMWareWorkstation;
                LaunchVMWareWorkstation(xGDBDebugStub);
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
            
            if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "qemu")) {
                mDbgConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorTCPServer();
            } else if (StringComparer.InvariantCultureIgnoreCase.Equals(mDebugInfo["BuildTarget"], "vmwareworkstation")) {
                mDbgConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorPipeServer();
            } else {
                throw new Exception("BuildTarget value not valid: '" + mDebugInfo["BuildTarget"] + "'!");
            }

            mDbgConnector.CmdTrace += new Action<Cosmos.Compiler.Debug.MsgType, uint>(DbgCmdTrace);
            mDbgConnector.CmdText += new Action<string>(DbgCmdText);
            mDbgConnector.CmdReady += new Action(DbgCmdReady);
            mDbgConnector.ConnectionLost = new Action<Exception>(
                delegate { 
                    mEngine.Callback.OnProcessExit(0);
                }
            );

            System.Threading.Thread.Sleep(250);
            mProcess = Process.Start(mProcessStartInfo);

            mProcess.EnableRaisingEvents = true;
            mProcess.Exited += new EventHandler(mProcess_Exited);

            // Sleep 250 and see if it exited too quickly. Why do we do this? We have .Exited hooked. Is this in case it happens between start and hook?
            // if so, why not hook before start? 
            System.Threading.Thread.Sleep(250);
            if (mProcess.HasExited) {
                Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
                Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
                Trace.WriteLine("ExitCode: " + mProcess.ExitCode);
                throw new Exception("Error while starting application");
            }

            mCallback = aCallback;
            mEngine = aEngine;
            mThread = new AD7Thread(aEngine, this);
            mCallback.OnThreadStart(mThread);
            mPort = aPort;
        }

        protected void DbgCmdReady() {

            System.Diagnostics.Debug.WriteLine("Remote Debugger: Ready");
        }

        public void SetBreakpointAddress(uint aAddress) {
            mDbgConnector.SetBreakpointAddress(aAddress);
        }

        void DbgCmdText(string obj) {
            mCallback.OnOutputString(obj + "\r\n");
        }

        internal AD7Thread Thread
        {
            get
            {
                return mThread;
            }
        }

        void DbgCmdTrace(Cosmos.Compiler.Debug.MsgType arg1, uint arg2) {
            switch (arg1) {
                case Cosmos.Compiler.Debug.MsgType.BreakPoint:
                    {
                        //((IDebugBreakEvent2)null).

                        //var xSourceInfo = mSourceMappings[arg2];

                        //mCallback.OnOutputString("Try to break now");
                        var xActualAddress = arg2 - 5; // - 5 to correct the addres:
                        // when doing a CALL, the return address is pushed, but that's the address of the next instruction, after CALL. call is 5 bytes (for now?)
                        mEngine.Callback.OnOutputString("Hit Breakpoint 0x" + xActualAddress.ToString("X8").ToUpper());
                        var xActionPoints = new List<object>();
                        var xBoundBreakpoints = new List<IDebugBoundBreakpoint2>();
                        foreach (var xBP in mEngine.m_breakpointManager.m_pendingBreakpoints)
                        {
                            foreach(var xBBP in xBP.m_boundBreakpoints){
                                if (xBBP.m_address == xActualAddress)
                                {
                                    xBoundBreakpoints.Add(xBBP);
                                }
                            }
                        }

                        mCurrentAddress = xActualAddress;
                        //mCallback.onb
                        mCallback.OnBreakpoint(mThread, new ReadOnlyCollection<IDebugBoundBreakpoint2>(xBoundBreakpoints), xActualAddress);
                        //mEngine.Callback.OnBreakComplete(mThread, );
                        mEngine.AfterBreak = true;
                        //mEngine.Callback.OnBreak(mThread);
                        break;
                    }
                default:
                    Console.WriteLine("TraceReceived: {0}", arg1);
                    break;
            }
        }


        #region IDebugProcess2 Members

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
            mProcess.Kill();
            return VSConstants.S_OK;
        }

        #endregion

        internal void ResumeFromLaunch()
        {
            // This unpauses our debug host
            // We do this because VS requires a start, and then a resume after. So we have debughost which is a stub
            // that allows VS to "see" that. Here we resume it.
            mProcess.StandardInput.WriteLine();

            if (mTargetHost == TargetHost.QEMU) {
               // QEMU and Pipes - QEMU will stop and wait till we connect. It will not even show until we do.
               // We have to do this after we release the debug host though.
                mDbgConnector.WaitConnect();
            }
        }

        void mProcess_Exited(object sender, EventArgs e) {
            Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
            Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
            //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
            //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);
            mCallback.OnProcessExit((uint)mProcess.ExitCode);
        }

        internal void Continue() {
            mCurrentAddress = null;
            mDbgConnector.SendCommand((byte)Command.Break);
        }

        internal void Step() {
            mDbgConnector.SendCommand((byte)Command.Step);
        }
    }
}
