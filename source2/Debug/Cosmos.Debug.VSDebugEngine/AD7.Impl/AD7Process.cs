//TODO: Move both of these to project options...
// In fact also eliminate TCP server and keep only Pipes
// Keep a note about servers.. we want to use servers and not clients, because we dont always know when the other side is ready
// and with a server, we are ready and its ready whenever... but sometime after us for sure.
#define DEBUG_CONNECTOR_TCP_SERVER
//#define DEBUG_CONNECTOR_PIPE_CLIENT
//#define DEBUG_CONNECTOR_PIPE_SERVER
//
#define VM_QEMU
//#define VM_VMWare

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;
using Cosmos.Debug.Common.CDebugger;
using System.Collections.ObjectModel;
using System.IO;
using Cosmos.Compiler.Debug;
using System.Collections.Specialized;
using Cosmos.Debug.Common;

namespace Cosmos.Debug.VSDebugEngine
{
    public class AD7Process : IDebugProcess2
    {
        internal Guid mID = Guid.NewGuid();
        private Process mProcess;
        private ProcessStartInfo mProcessStartInfo;
        private EngineCallback mCallback;
        private AD7Thread mThread;
        private AD7Engine mEngine;
        private DebugEngine mDebugEngine;
        internal ReverseSourceInfos mReverseSourceMappings;
        internal SourceInfos mSourceMappings;
        internal uint? mCurrentAddress = null;
        internal string mISO;
        private readonly NameValueCollection mDebugInfo;

        internal AD7Process(string aDebugInfo, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort)
        {
            mDebugInfo = new NameValueCollection();
            NameValueCollectionHelper.LoadFromString(mDebugInfo, aDebugInfo);

            mISO = mDebugInfo["ISOFile"];

            var xGDBDebugStub = false;
            Boolean.TryParse(mDebugInfo["EnableGDB"], out xGDBDebugStub);

            mProcessStartInfo = new ProcessStartInfo(Path.Combine(PathUtilities.GetVSIPDir(), "Cosmos.Debug.HostProcess.exe"));

#if VM_QEMU
    #if DEBUG_CONNECTOR_TCP_SERVER
                var xDebugConnectorStr = "-serial tcp:127.0.0.1:4444";
    #endif
    #if DEBUG_CONNECTOR_PIPE_CLIENT
                var xDebugConnectorStr = @"-serial pipe:CosmosDebug";
    #endif
    #if DEBUG_CONNECTOR_PIPE_SERVER
                var xDebugConnectorStr = @"-serial pipe:CosmosDebug";
    #endif

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

            if (xGDBDebugStub) {
                mProcessStartInfo.Arguments 
                    += " --gdb tcp::8832" // We now use 8832 to be same as VMWare
                    + "-S"; // Pause on startup, wait for GDB to connect and control
            }

#endif
#if VM_VMWare
            mProcessStartInfo.Arguments = @"true C:\source\Cosmos\Build\VMWare\Workstation\Cosmos.vmx";
#endif

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
            if (mSourceMappings.Count == 0)
            {
                throw new Exception("Debug data not found: SourceMappings");
            }
            mReverseSourceMappings = new ReverseSourceInfos(mSourceMappings);
            mDebugEngine = new DebugEngine();

#if DEBUG_CONNECTOR_TCP_SERVER
            mDebugEngine.DebugConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorTCPServer();
#endif
#if DEBUG_CONNECTOR_PIPE_CLIENT
            mDebugEngine.DebugConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorPipeClient();
#endif
#if DEBUG_CONNECTOR_PIPE_SERVER
            mDebugEngine.DebugConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorPipeServer();
#endif

            mDebugEngine.TraceReceived += new Action<Cosmos.Compiler.Debug.MsgType, uint>(mDebugEngine_TraceReceived);
            mDebugEngine.TextReceived += new Action<string>(mDebugEngine_TextReceived);
            mDebugEngine.DebugConnector.ConnectionLost = new Action<Exception>(
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
            if (mProcess.HasExited)
            {
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

        public void SetBreakpointAddress(uint aAddress)
        {
            mDebugEngine.DebugConnector.SetBreakpointAddress(aAddress);
        }

        void mDebugEngine_TextReceived(string obj)
        {
            mCallback.OnOutputString(obj + "\r\n");
        }

        internal AD7Thread Thread
        {
            get
            {
                return mThread;
            }
        }

        void mDebugEngine_TraceReceived(Cosmos.Compiler.Debug.MsgType arg1, uint arg2)
        {
            switch (arg1)
            {
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

            // QEMU and Pipes - QEMU will stop and wait till we connect. It will not even show until we do.
            // We have to do this after we release the debug host though.
            mDebugEngine.DebugConnector.WaitConnect();
        }

        void mProcess_Exited(object sender, EventArgs e)
        {
            Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
            Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
            //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
            //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);
            //mCallback.OnProcessExit((uint)mProcess.ExitCode);
        }

        internal void Continue()
        {
            mCurrentAddress = null;
            mDebugEngine.DebugConnector.SendCommand((byte)Command.Break);
        }

        internal void Step()
        {
            mDebugEngine.DebugConnector.SendCommand((byte)Command.Step);
        }
    }
}
