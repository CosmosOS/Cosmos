using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Cosmos.IL2CPU.Debug;

namespace Cosmos.Build.Windows {
    public abstract class DebugConnectorStream : DebugConnector {
        private Stream mStream;
        // Buffer to hold incoming message
        private byte[] mPacket = new byte[5];
        // Current # of bytes in mPacket
        private int mCurrentPos = 0;
        // Current packet size - Set after first byte (command) is received
        // Set to 0 when waiting on a packet
        private int mPacketSize = 0;
        private MsgType mMsgType;
 
        public override void SendCommand(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            mStream.Write(xData, 0, xData.Length);
        }
        
        protected void Start(Stream aStream) {
            mStream = aStream;
            // Request first command
            aStream.BeginRead(mPacket, 0, 1, new AsyncCallback(DoRead), aStream);
        }

        private void DoRead(IAsyncResult aResult) {
            try {
                var xStream = (Stream)aResult.AsyncState;
                int xCount = xStream.EndRead(aResult);
                int xBytesToRead;
                // If 0, end of stream then just exit without calling BeginRead again
                if (xCount == 0) {
                    return;
                // Command received, determine packet size 
                } else if (mCurrentPos == 0) {
                    mMsgType = (MsgType)mPacket[0];
                    switch (mMsgType) {
                        case MsgType.TracePoint:
                            mPacketSize = 5;
                            break;
                        case MsgType.Text:
                            mPacketSize = 2;
                            break;
                        default:
                            throw new Exception("Unknown debug command");
                    }
                    mCurrentPos = 1;
                    xBytesToRead = mPacketSize - 1;
                // Full packet received, process it
                } else if ((xCount + mCurrentPos) == mPacketSize) {
                    switch (mMsgType) {
                        case MsgType.TracePoint:
                            UInt32 xEIP = (UInt32)((mPacket[1] << 24) | (mPacket[2] << 16)
                                | (mPacket[3] << 8) | mPacket[4]);
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, CmdTrace, xEIP);
                            break;
                        case MsgType.Text:
                            Dispatcher.BeginInvoke(DispatcherPriority.Background, CmdText, mPacket[1].ToString());
                            break;
                    }
                    // Request next command
                    mCurrentPos = 0;
                    mPacketSize = 0;
                    xBytesToRead = 1;
                } else {
                    mCurrentPos += xCount;
                    xBytesToRead = mPacketSize - mCurrentPos;
                }
                xStream.BeginRead(mPacket, mCurrentPos, xBytesToRead, new AsyncCallback(DoRead), xStream);
            } catch (System.IO.IOException ex) {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, ConnectionLost, ex);
            }
        }

   }
}
