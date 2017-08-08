using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;

namespace Cosmos.Debug.DebugConnectors
{
    /// <summary>Use a named pipe client to implement wire transfer protocol between a Debug Stub
    /// hosted in a debugged Cosmos Kernel and our Debug Engine hosted in Visual Studio.
    /// Hyper-V provides a pipe server to expose guest serial ports.</summary>
    public class DebugConnectorPipeClient : DebugConnectorStreamWithoutTimeouts
    {
        // private AutoResetEvent mWaitConnectEvent = new AutoResetEvent(false);
        private NamedPipeClientStream mPipe;

        public const string DefaultCosmosPipeName = "CosmosSerial";

        public DebugConnectorPipeClient(string aName)
        {
            mPipe = new NamedPipeClientStream(".", aName, PipeDirection.InOut, PipeOptions.WriteThrough);
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
            mPipe.Connect();
            mStream = mPipe;
        }

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
