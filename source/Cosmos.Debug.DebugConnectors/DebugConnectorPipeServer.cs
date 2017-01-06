using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Cosmos.Debug.DebugConnectors
{
    /// <summary>Use a named pipe server to implement wire transfer protocol between a Debug Stub
    /// hosted in a debugged Cosmos Kernel and our Debug Engine hosted in Visual Studio.
    /// Both VMware and Bochs use a pipe to expose guest serial ports to the host.</summary>
    public class DebugConnectorPipeServer: DebugConnectorStreamWithoutTimeouts
    {
        // private AutoResetEvent mWaitConnectEvent = new AutoResetEvent(false);
        private NamedPipeServerStream mPipe;

        public const string DefaultCosmosPipeName = "Cosmos\\Serial";

        public DebugConnectorPipeServer(string aName)
        {
            mPipe = new NamedPipeServerStream(aName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                                              PipeOptions.None, 1024, 1);
            Start();
        }

        protected override int TryRead(byte[] buffer, int offset, int count, int timeout)
        {
            var xStream = mStream;
            if (xStream == null)
            {
                return 0;
            }
            uint xBytesAvailable = 0;
            if (PeekNamedPipe(mPipe.SafePipeHandle.DangerousGetHandle(), null, 0, IntPtr.Zero, ref xBytesAvailable, IntPtr.Zero))
            {
                if (xBytesAvailable > 0)
                {
                    return xStream.Read(buffer, offset, count);
                }
            }
            Thread.Sleep(timeout);
            if (PeekNamedPipe(mPipe.SafePipeHandle.DangerousGetHandle(), null, 0, IntPtr.Zero, ref xBytesAvailable, IntPtr.Zero))
            {
                if (xBytesAvailable > 0)
                {
                    return xStream.Read(buffer, offset, count);
                }
            }
            return 0;
        }

        public override bool IsConnected
        {
            get
            {
                return base.IsConnected && mPipe.IsConnected;
            }
        }

        protected override void InitializeBackground()
        {
            mPipe.WaitForConnection();
            mStream = mPipe;
        }

        //public void DoWaitForConnection(IAsyncResult aResult)
        //{
        //    var xPipe = (NamedPipeServerStream)aResult.AsyncState;
        //    xPipe.EndWaitForConnection(aResult);
        //    // mWaitConnectEvent.Set();
        //    Start(xPipe);
        //}

        protected override bool GetIsConnectedToDebugStub()
        {
            return mPipe.IsConnected;
        }

        [DllImport("kernel32.dll", EntryPoint = "PeekNamedPipe", SetLastError = true)]
        private static extern bool PeekNamedPipe(IntPtr handle,
                                                 byte[] buffer, uint nBufferSize, IntPtr bytesRead,
                                                 ref uint bytesAvail, IntPtr BytesLeftThisMessage);
    }
}
