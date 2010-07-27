using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Debug;

namespace Cosmos.Debug.Common.CDebugger {
    public abstract class DebugConnectorStream : DebugConnector {
        private Stream mStream;
        
        protected class Incoming {
            public Stream Stream;
            // Buffer to hold incoming message
            public byte[] Packet;
            // Current # of bytes in mPacket
            public int CurrentPos = 0;
            public Action<byte[]> Completed;
        }

        protected override void SendData(byte[] aBytes) {
            if (mStream != null) {
                mStream.Write(aBytes, 0, aBytes.Length);
            } else {
                mPendingSend = (byte[])aBytes.Clone();
            }
        }

        private byte[] mPendingSend = null;

        protected void Start(Stream aStream) {
            mStream = aStream;
            if (mPendingSend != null) {
                SendData(mPendingSend);
                mPendingSend = null;
            }
            // Request first command
            Next(1, PacketCommand);
        }
        
        protected override void PacketTracePoint(byte[] aPacket) {
            CmdTrace(mCurrentMsgType, GetUInt32(aPacket, 0));
            Next(1, PacketCommand);
        }
        
        protected override void PacketText(byte[] aPacket) {
            CmdText(ASCIIEncoding.ASCII.GetString(aPacket));
            Next(1, PacketCommand);
        }
        
        protected override void Next(int aPacketSize, Action<byte[]> aCompleted) {
            var xIncoming = new Incoming() {
                Packet = new byte[aPacketSize]
                , Stream = mStream
                , Completed = aCompleted
            };
            mStream.BeginRead(xIncoming.Packet, 0, aPacketSize, new AsyncCallback(DoRead), xIncoming);
        }
        
        protected void DoRead(IAsyncResult aResult) {
            try
            {
                Console.WriteLine("DoRead");
                var xIncoming = (Incoming)aResult.AsyncState;
                int xCount = xIncoming.Stream.EndRead(aResult);
                xIncoming.CurrentPos += xCount;
                // If 0, end of stream then just exit without calling BeginRead again
                if (xCount == 0)
                {
                    return;
                    // Packet is not full yet, read more data
                }
                else if (xIncoming.CurrentPos < xIncoming.Packet.Length)
                {
                    xIncoming.Stream.BeginRead(xIncoming.Packet, xIncoming.CurrentPos
                        , xIncoming.Packet.Length - xIncoming.CurrentPos
                        , new AsyncCallback(DoRead), xIncoming);
                    // Full packet received, process it
                }
                else
                {
                    xIncoming.Completed(xIncoming.Packet);
                }
            }
            catch (System.IO.IOException ex)
            {
                ConnectionLost(ex);
            }
        }

   }
}
