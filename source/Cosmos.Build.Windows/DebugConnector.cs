using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Debug;

namespace Cosmos.Build.Windows {
    public abstract class DebugConnector {
        protected delegate void PacketReceivedDelegate(byte[] aBytes);
        public delegate void ConnectionLostDelegate(Exception ex);
        public delegate void CmdTraceDelegate(UInt32 aEIP);
        public delegate void CmdTextDelegate(string aText);
        
        //TODO: These should not be this way and should in fact
        // be checked or better yet done by constructor arguments
        // but that puts a dependency on where the sub classes
        // are created.
        public ConnectionLostDelegate ConnectionLost;
        public CmdTraceDelegate CmdTrace;
        public CmdTextDelegate CmdText;
        // Cannot use Dispatcher.CurrentDispatcher - it doesnt work
        // Must use same dispatcher as the Window. Could also change
        // delegates to catch them in a thread and then redispatch on their own
        public System.Windows.Threading.Dispatcher Dispatcher;
        
        protected abstract void SendData(byte[] aBytes);
        protected abstract void Next(int aPacketSize, PacketReceivedDelegate aCompleted);        
        protected abstract void PacketTracePoint(byte[] aPacket);
        protected abstract void PacketText(byte[] aPacket);
    
        public void SendCommand(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            SendData(xData);
        }

        protected UInt32 GetUInt32(byte[] aBytes, int aOffset) {
           return (UInt32)((aBytes[aOffset + 3] << 24) | (aBytes[aOffset + 2] << 16)
              | (aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }
        
        protected UInt16 GetUInt16(byte[] aBytes, int aOffset) {
           return (UInt16)((aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }
        
        protected void PacketReceived(byte[] aPacket) {
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

        protected void PacketTextSize(byte[] aPacket) {
            Next(GetUInt16(aPacket, 0), PacketText);
        }
        
    }
}
