using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Debug;

namespace Cosmos.Debug.Common.CDebugger
{
    public abstract class DebugConnector {
        //TODO: These should not be this way and should in fact
        // be checked or better yet done by constructor arguments
        // but that puts a restriction on where the sub classes
        // are created.
        public Action<Exception> ConnectionLost;
        public Action<MsgType, UInt32> CmdTrace;
        public Action<string> CmdText;
        public Action CmdReady;
        
        protected MsgType mCurrentMsgType;

        public abstract void WaitConnect();
        
        protected abstract void SendData(byte[] aBytes);
        protected abstract void Next(int aPacketSize, Action<byte[]> aCompleted);        
        protected abstract void PacketTracePoint(byte[] aPacket);
        protected abstract void PacketText(byte[] aPacket);
    
        public void SendCommand(byte aCmd) {
            var xData = new byte[1];
            xData[0] = aCmd;
            SendData(xData);
        }

        public void SetBreakpointAddress(uint aAddress) {
            var xData = new byte[5];
            xData[0] = (byte)Command.BreakOnAddress;
            Array.Copy(BitConverter.GetBytes(aAddress), 0, xData, 1, 4);
            SendData(xData);
        }

        protected UInt32 GetUInt32(byte[] aBytes, int aOffset) {
           return (UInt32)((aBytes[aOffset + 3] << 24) | (aBytes[aOffset + 2] << 16)
              | (aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }
        
        protected UInt16 GetUInt16(byte[] aBytes, int aOffset) {
           return (UInt16)((aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }
        
        protected void PacketCommand(byte[] aPacket) {
            mCurrentMsgType = (MsgType)aPacket[0];
            // Could change to an array, but really not much benefit
            switch (mCurrentMsgType) {
                case MsgType.TracePoint:
                case MsgType.BreakPoint:
                    Next(4, PacketTracePoint);            
                    break;
                case MsgType.Message:
                    Next(2, PacketTextSize);
                    break;
                case MsgType.Ready:
                    CmdReady();
                    break;
                case MsgType.Noop:
                    // MtW: When implementing Serial support for debugging on real hardware, it appears
                    //      that when booting a machine, in the bios it emits zero's to the serial port.
                    // Kudzu: Made a Noop command to handle this
                    Next(1, PacketCommand);
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
