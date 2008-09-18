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
        
        protected delegate void PacketReceivedDelegate(byte[] aBytes);
        
        protected class Incoming {
            public Stream Stream;
            // Buffer to hold incoming message
            public byte[] Packet = new byte[5];
            // Current # of bytes in mPacket
            public int CurrentPos = 0;
            // Current packet size - Set after first byte (command) is received
            // Set to 0 when waiting on a packet
            public int PacketSize = 0;
            public MsgType MsgType;
        }
 
        public override void SendCommand(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            mStream.Write(xData, 0, xData.Length);
        }
        
        protected void Start(Stream aStream) {
            mStream = aStream;
            var xIncoming = new Incoming() { Stream = aStream };
            // Request first command
            aStream.BeginRead(xIncoming.Packet, 0, 1, new AsyncCallback(DoRead), xIncoming);
        }
        
        private void CmdReceived(byte[] aBytes) {
        }

        private void DoRead(IAsyncResult aResult) {
            try {
            } catch (System.IO.IOException ex) {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, ConnectionLost, ex);
            }
        }
        
        private void DoRead2(IAsyncResult aResult) {
            var xIncoming = (Incoming)aResult.AsyncState;
            int xCount = xIncoming.Stream.EndRead(aResult);
            int xBytesToRead;
            // If 0, end of stream then just exit without calling BeginRead again
            if (xCount == 0) {
                return;
            // Command received, determine packet size 
            } else if (xIncoming.CurrentPos == 0) {
                xIncoming.MsgType = (MsgType)xIncoming.Packet[0];
                switch (xIncoming.MsgType) {
                    case MsgType.TracePoint:
                        xIncoming.PacketSize = 5;
                        break;
                    case MsgType.Text:
                        xIncoming.PacketSize = 2;
                        break;
                    default:
                        throw new Exception("Unknown debug command");
                }
                xIncoming.CurrentPos = 1;
                xBytesToRead = xIncoming.PacketSize - 1;
            // Read size byte
            } else if ((xIncoming.MsgType == MsgType.Text) && (xIncoming.CurrentPos == 1)) {
                xBytesToRead = xIncoming.Packet[1];
                xIncoming.CurrentPos = 2;
                xIncoming.PacketSize = xBytesToRead + xIncoming.CurrentPos;
                xIncoming.Packet = new byte[xIncoming.PacketSize];
            // Full packet received, process it
            } else if ((xCount + xIncoming.CurrentPos) == xIncoming.PacketSize) {
                switch (xIncoming.MsgType) {
                    case MsgType.TracePoint:
                        UInt32 xEIP = (UInt32)((xIncoming.Packet[4] << 24) | (xIncoming.Packet[3] << 16)
                            | (xIncoming.Packet[2] << 8) | xIncoming.Packet[1]);
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, CmdTrace, xEIP);
                        break;
                    case MsgType.Text:
                        string xText = ASCIIEncoding.ASCII.GetString(xIncoming.Packet, 2
                            , xIncoming.Packet.Length - 2);
                        Dispatcher.BeginInvoke(DispatcherPriority.Background, CmdText, xText);
                        break;
                }
                // Request next command
                xIncoming.CurrentPos = 0;
                xIncoming.PacketSize = 0;
                xBytesToRead = 1;
                xIncoming.Packet = new byte[5];
            } else {
                xIncoming.CurrentPos += xCount;
                xBytesToRead = xIncoming.PacketSize - xIncoming.CurrentPos;
            }
            xIncoming.Stream.BeginRead(xIncoming.Packet, xIncoming.CurrentPos, xBytesToRead
                , new AsyncCallback(DoRead), xIncoming);
        }

   }
}
