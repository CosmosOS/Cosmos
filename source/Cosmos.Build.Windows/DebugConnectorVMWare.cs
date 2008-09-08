using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.Build.Windows {
    public class DebugConnectorVMWare : DebugConnector {
        NamedPipeServerStream mPipe;
        protected byte[] mTCPData = new byte[4];
        protected int mCurrentPos = 0;
    
        public DebugConnectorVMWare() {
            // \\.\pipe\CosmosDebug
            mPipe = new NamedPipeServerStream("CosmosDebug", PipeDirection.InOut);
            mPipe.BeginWaitForConnection(new AsyncCallback(DoWaitForConnection), mPipe);
        }

        public void DoWaitForConnection(IAsyncResult aResult) {
            var xPipe = (NamedPipeServerStream)aResult.AsyncState;
            xPipe.EndWaitForConnection(aResult);
            xPipe.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(DoRead), xPipe);
        }

        //TODO: Can merge some with QEMU, they both work on streams
        // Make a DebugConnectorStream and they can share that
        protected void DoRead(IAsyncResult aResult) {
            try {
                var xStream = (Stream)aResult.AsyncState;
                int xCount = xStream.EndRead(aResult);
                if (xCount != 4) {
                    if ((xCount + mCurrentPos) != 4) {
                        mCurrentPos += xCount;
                        xStream.BeginRead(mTCPData, mCurrentPos, 4 - mCurrentPos
                            , new AsyncCallback(DoRead), xStream);
                        return;
                    }
                }
                mCurrentPos = 0;
                UInt32 xEIP = (UInt32)((mTCPData[0] << 24) | (mTCPData[1] << 16)
                    | (mTCPData[2] << 8) | mTCPData[3]);
                xStream.BeginRead(mTCPData, 0, mTCPData.Length, new AsyncCallback(DoRead), xStream);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, DebugPacketReceived, xEIP);
            } catch (System.IO.IOException ex) {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, ConnectionLost, ex);
            }
        }

        public override void SendCommand(byte aCmd) {
        }
    }
}
