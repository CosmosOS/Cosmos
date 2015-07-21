using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Cosmos.Debug.Common
{
//    public class DebugConnectorTestPipeServer: DebugConnectorStream
//    {
//// private AutoResetEvent mWaitConnectEvent = new AutoResetEvent(false);
//        private FileStream mPipe;

//        private SafeFileHandle mHandle;
//        public DebugConnectorTestPipeServer(string aName)
//        {
//            mHandle = CreateNamedPipe(@"\\.\pipe\" + aName, 3 | 0x40000000,
//                                      0,
//                                      1,
//                                      1,
//                                      12,
//                                      0,
//                                      IntPtr.Zero
//                );
//            var xConnect = new Action(() =>
//                                      {
//                                          int success = ConnectNamedPipe(mHandle, IntPtr.Zero);
//                                          Console.WriteLine("A client connected");
//                                          //failed to connect client pipe
//                                          if (success != 1)
//                                          {
//                                              throw new Exception("Unable to connect pipe");
//                                          }

//                                          mPipe = new FileStream(mHandle, FileAccess.ReadWrite, 8192, false);
//                                          Start(mPipe);
//                                      });
//            xConnect.BeginInvoke(r => xConnect.EndInvoke(r), null);
//        }

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern SafeFileHandle CreateNamedPipe(
//            String pipeName,
//            uint dwOpenMode,
//            uint dwPipeMode,
//            uint nMaxInstances,
//            uint nOutBufferSize,
//            uint nInBufferSize,
//            uint nDefaultTimeOut,
//            IntPtr lpSecurityAttributes
//            );

//        [DllImport("kernel32.dll", SetLastError = true)]
//        private static extern int ConnectNamedPipe(
//            SafeFileHandle hNamedPipe,
//            IntPtr lpOverlapped
//            );
//    }
}
