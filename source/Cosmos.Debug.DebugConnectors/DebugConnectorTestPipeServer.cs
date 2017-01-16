using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Cosmos.Debug.DebugConnectors
{
    public class DebugConnectorTestPipeServer : DebugConnectorStreamWithoutTimeouts
    {
        // private AutoResetEvent mWaitConnectEvent = new AutoResetEvent(false);
        private FileStream mPipe;

        private SafeFileHandle mHandle;
        public DebugConnectorTestPipeServer(string aName)
        {
            mHandle = CreateNamedPipe(@"\\.\pipe\" + aName, 3 | 0x40000000,
                                      0,
                                      1,
                                      1,
                                      12,
                                      0,
                                      IntPtr.Zero
                );
            Start();
        }

        protected override int TryRead(byte[] buffer, int offset, int count, int timeout)
        {
            mStream.ReadTimeout = timeout;
            var xStream = mStream;
            if (xStream == null)
            {
                return 0;
            }
            return xStream.Read(buffer, offset, count);
        }



        protected override void InitializeBackground()
        {
            int success = ConnectNamedPipe(mHandle, IntPtr.Zero);
            Console.WriteLine("A client connected");
            //failed to connect client pipe
            if (success != 1)
            {
                throw new Exception("Unable to connect pipe");
            }

            mPipe = new FileStream(mHandle, FileAccess.ReadWrite, 8192, false);
            Start(mPipe);
        }

        protected override bool GetIsConnectedToDebugStub()
        {
            return mPipe.CanRead;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateNamedPipe(
            String pipeName,
            uint dwOpenMode,
            uint dwPipeMode,
            uint nMaxInstances,
            uint nOutBufferSize,
            uint nInBufferSize,
            uint nDefaultTimeOut,
            IntPtr lpSecurityAttributes
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int ConnectNamedPipe(
            SafeFileHandle hNamedPipe,
            IntPtr lpOverlapped
            );
    }
}
