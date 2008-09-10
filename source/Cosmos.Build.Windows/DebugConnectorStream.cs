using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Cosmos.Build.Windows {
    public abstract class DebugConnectorStream : DebugConnector {
        private Stream mStream;
        private byte[] mPacket = new byte[4];
        private int mCurrentPos = 0;
 
        public override void SendCommand(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            mStream.Write(xData, 0, xData.Length);
        }
        
        protected void Start(Stream aStream) {
            mStream = aStream;
            aStream.BeginRead(mPacket, 0, mPacket.Length, new AsyncCallback(DoRead), aStream);
        }

        private void DoRead(IAsyncResult aResult) {
            try {
                var xStream = (Stream)aResult.AsyncState;
                int xCount = xStream.EndRead(aResult);
                if (xCount != 4) {
                    if ((xCount + mCurrentPos) != 4) {
                        mCurrentPos += xCount;
                        xStream.BeginRead(mPacket, mCurrentPos, 4 - mCurrentPos
                            , new AsyncCallback(DoRead), xStream);
                        return;
                    }
                }
                mCurrentPos = 0;
                UInt32 xEIP = (UInt32)((mPacket[0] << 24) | (mPacket[1] << 16)
                    | (mPacket[2] << 8) | mPacket[3]);
                xStream.BeginRead(mPacket, 0, mPacket.Length, new AsyncCallback(DoRead), xStream);
                Dispatcher.BeginInvoke(DispatcherPriority.Background, DebugPacketReceived, xEIP);
            } catch (System.IO.IOException ex) {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, ConnectionLost, ex);
            }
        }

   }
}
