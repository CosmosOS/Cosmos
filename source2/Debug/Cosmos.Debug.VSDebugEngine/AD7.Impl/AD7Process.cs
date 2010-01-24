//#define DEBUG_CONNECTOR_TCP_CLIENT
#define DEBUG_CONNECTOR_TCP_SERVER
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


namespace Cosmos.Debug.VSDebugEngine
{
    public class AD7Process : IDebugProcess2
    {
        internal string mISO;
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

        internal AD7Process(string aISOFile, EngineCallback aCallback, AD7Engine aEngine, IDebugPort2 aPort)
        {
            mISO = aISOFile;

            mProcessStartInfo = new ProcessStartInfo(typeof(Cosmos.Debug.HostProcess.Program).Assembly.Location);
#if DEBUG_CONNECTOR_TCP_SERVER
            var xDebugConnectorStr = "-serial tcp:127.0.0.1:4444";
#endif
#if DEBUG_CONNECTOR_TCP_CLIENT
            var xDebugConnectorStr = "-serial tcp::4444,server";
#endif
            mProcessStartInfo.Arguments = @"e:\Cosmos\Build\Tools\qemu\qemu.exe" + @" -L e:/Cosmos/Build/Tools/qemu -cdrom " + '"' + mISO.Replace('\\', '/').Replace(" ", "\\ ") + "\" -boot d " + xDebugConnectorStr;
            mProcessStartInfo.CreateNoWindow = true;
            mProcessStartInfo.UseShellExecute = false;
            mProcessStartInfo.RedirectStandardInput = true;
            mProcessStartInfo.RedirectStandardError = true;
            mProcessStartInfo.RedirectStandardOutput = true;
            var xLabelByAddressMapping = Cosmos.Debug.Common.CDebugger.SourceInfo.ParseFile(Path.GetDirectoryName(aISOFile));
            mSourceMappings = Cosmos.Debug.Common.CDebugger.SourceInfo.GetSourceInfo(xLabelByAddressMapping, Path.ChangeExtension(aISOFile, ".cxdb"));
            mReverseSourceMappings = new ReverseSourceInfos(mSourceMappings);
            mDebugEngine = new DebugEngine();
#if DEBUG_CONNECTOR_TCP_SERVER
            mDebugEngine.DebugConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorTCPServer();
#endif
            mDebugEngine.TraceReceived += new Action<Cosmos.Compiler.Debug.MsgType, uint>(mDebugEngine_TraceReceived);
            mDebugEngine.TextReceived += new Action<string>(mDebugEngine_TextReceived);
#if!DEBUG_CONNECTOR_TCP_SERVER
            throw new NotImplementedException();
#else
            mDebugEngine.DebugConnector.ConnectionLost = new Action<Exception>(delegate { mEngine.Callback.OnProcessExit(0); });
#endif

            System.Threading.Thread.Sleep(250);
            mProcess = Process.Start(mProcessStartInfo);
            mProcess.EnableRaisingEvents = true;
            mProcess.Exited += new EventHandler(mProcess_Exited);
            if (mProcess.HasExited)
            {
                Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
                Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
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
        Trace.WriteLine(new StackTrace(false).GetFrame(0).GetMethod().GetFullName());
        throw new NotImplementedException();
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
#if DEBUG_CONNECTOR_TCP_SERVER
            mProcess.StandardInput.WriteLine("");
#endif
#if DEBUG_CONNECTOR_TCP_CLIENT
            mProcess.StandardInput.WriteLine("");
            mDebugEngine.DebugConnector = new Cosmos.Debug.Common.CDebugger.DebugConnectorTCPClient();
#endif
           
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