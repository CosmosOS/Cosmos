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
            public PacketReceivedDelegate Completed;
        }
 
        public override void SendCommand(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            mStream.Write(xData, 0, xData.Length);
        }
        
        protected void Start(Stream aStream) {
            mStream = aStream;
            var xIncoming = new Incoming() { 
                Stream = aStream 
            };
            // Request first command
            Next(1, PacketReceived);
        }
        
        private UInt32 GetUInt32(byte[] aBytes, int aOffset) {
           return (UInt32)((aBytes[aOffset + 3] << 24) | (aBytes[aOffset + 2] << 16)
              | (aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }
        
        private UInt16 GetUInt16(byte[] aBytes, int aOffset) {
           return (UInt16)((aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }
        
        private void PacketTracePoint(byte[] aPacket) {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, CmdTrace, GetUInt32(aPacket, 0));
            Next(1, PacketReceived);
        }
        
        private void PacketTextSize(byte[] aPacket) {
            Next(GetUInt16(aPacket, 0), PacketText);
        }
        
        private void PacketText(byte[] aPacket) {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, CmdText
                , ASCIIEncoding.ASCII.GetString(aPacket));
            Next(1, PacketReceived);
        }
        
        private void PacketReceived(byte[] aPacket) {
            // Could change to an array, but really not much benefit
            switch ((MsgType)aPacket[0]) {
                case MsgType.TracePoint:
                    Next(4, PacketTracePoint);            
                    break;
                case MsgType.Text:
                    Next(2, PacketTextSize);            
                    break;
                default:
                    throw new Exception("Unknown debug command");
            }
        }

        private void Next(int aPacketSize, PacketReceivedDelegate aCompleted) {
            var xIncoming = new Incoming() {
                Packet = new byte[aPacketSize]
                , Stream = mStream
                , Completed = aCompleted
            };
            mStream.BeginRead(xIncoming.Packet, 0, aPacketSize, new AsyncCallback(DoRead), xIncoming);
        }
        
        private void DoRead(IAsyncResult aResult) {
            try {
                var xIncoming = (Incoming)aResult.AsyncState;
                int xCount = xIncoming.Stream.EndRead(aResult);
                xIncoming.CurrentPos += xCount;
                // If 0, end of stream then just exit without calling BeginRead again
                if (xCount == 0) {
                    return;
                // Packet is not full yet, read more data
                } else if (xIncoming.CurrentPos < xIncoming.Packet.Length) {
                    xIncoming.Stream.BeginRead(xIncoming.Packet, xIncoming.CurrentPos
                        , xIncoming.Packet.Length - xIncoming.CurrentPos
                        , new AsyncCallback(DoRead), xIncoming);
                // Full packet received, process it
                } else {
                    xIncoming.Completed(xIncoming.Packet);
                }
            } catch (System.IO.IOException ex) {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, ConnectionLost, ex);
            }
        }

   }
}
