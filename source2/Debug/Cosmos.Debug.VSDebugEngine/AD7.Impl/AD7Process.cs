using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Debugger.Interop;
using Microsoft.VisualStudio;
using System.Diagnostics;


namespace Cosmos.Debug.VSDebugEngine
{
    public class AD7Process: IDebugProcess2
    {
        private string mISO;
        internal Guid mID = Guid.NewGuid();
        private Process mProcess;
        private ProcessStartInfo mProcessStartInfo;
        private EngineCallback mCallback;
        private AD7Thread mThread;
        private AD7Engine mEngine;
        internal AD7Process(string aISOFile, EngineCallback aCallback, AD7Thread aThread, AD7Engine aEngine)
        {
            mISO = aISOFile;
            
            mProcessStartInfo = new ProcessStartInfo(typeof(Cosmos.Debug.HostProcess.Program).Assembly.Location);
            mProcessStartInfo.Arguments = @"e:\Cosmos\Build\Tools\qemu\qemu.exe" + @" -L e:/Cosmos/Build/Tools/qemu -cdrom " + '"' + mISO.Replace('\\', '/').Replace(" ", "\\ ") + "\" -boot d";
            mProcessStartInfo.CreateNoWindow = true;
            mProcessStartInfo.UseShellExecute = false;
            mProcessStartInfo.RedirectStandardInput = true;
            mProcessStartInfo.RedirectStandardError = true;
            mProcessStartInfo.RedirectStandardOutput = true;
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
            mThread = aThread;
        }


        #region IDebugProcess2 Members

        public int Attach(IDebugEventCallback2 pCallback, Guid[] rgguidSpecificEngines, uint celtSpecificEngines, int[] rghrEngineAttach)
        {
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
            throw new NotImplementedException();
        }

        public int GetAttachedSessionName(out string pbstrSessionName)
        {
            throw new NotImplementedException();
        }

        public int GetInfo(uint Fields, PROCESS_INFO[] pProcessInfo)
        {
            throw new NotImplementedException();
        }

        public int GetName(uint gnType, out string pbstrName)
        {
            throw new NotImplementedException();
        }

        public int GetPhysicalProcessId(AD_PROCESS_ID[] pProcessId)
        {
            pProcessId[0].dwProcessId = (uint)mProcess.Id;
            pProcessId[0].ProcessIdType = (uint)enum_AD_PROCESS_ID.AD_PROCESS_ID_SYSTEM;
            return VSConstants.S_OK;
        }

        private AD7Port mPort = null;

        public int GetPort(out IDebugPort2 ppPort)
        {
            if (mPort == null)
            {
                mPort = new AD7Port();
            }
            ppPort = mPort;
            return VSConstants.S_OK;
        }

        public int GetProcessId(out Guid pguidProcessId)
        {
            pguidProcessId = mID;
            return VSConstants.S_OK;
        }

        public int GetServer(out IDebugCoreServer2 ppServer)
        {
            throw new NotImplementedException();
        }

        public int Terminate()
        {
            return VSConstants.S_OK;
        }

        #endregion

        internal void ResumeFromLaunch()
        {
            mProcess.StandardInput.WriteLine("");
        }

        void mProcess_Exited(object sender, EventArgs e)
        {
            Trace.WriteLine("Error while running: " + mProcess.StandardError.ReadToEnd());
            Trace.WriteLine(mProcess.StandardOutput.ReadToEnd());
            //AD7ThreadDestroyEvent.Send(mEngine, mThread, (uint)mProcess.ExitCode);
            //mCallback.OnProgramDestroy((uint)mProcess.ExitCode);
            //mCallback.OnProcessExit((uint)mProcess.ExitCode);
        }
    }
}