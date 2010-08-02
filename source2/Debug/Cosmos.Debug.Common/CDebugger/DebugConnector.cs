using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Debug;
using System.IO;
using System.Windows.Forms;

namespace Cosmos.Debug.Common.CDebugger
{
    public abstract class DebugConnector: IDisposable {
        //TODO: These should not be this way and should in fact
        // be checked or better yet done by constructor arguments
        // but that puts a restriction on where the sub classes
        // are created.
        public Action<Exception> ConnectionLost;
        public Action<MsgType, UInt32> CmdTrace;
        public Action<string> CmdText;
        public Action CmdStarted;
        
        protected MsgType mCurrentMsgType;

        public abstract void WaitConnect();

        public abstract bool Connected {
            get;
        }

        protected void SendCommandData(byte[] aBytes) {
            //System.Windows.Forms.MessageBox.Show(xSB.ToString());
            // If not connected, we dont send anything. Things like BPs etc can be set before connected.
            // The debugger must resend these after the start command hits.
            // We dont queue them, as it would end up with a lot of overlapping ops, ie set and then remove.
            // We also dont check connected at caller, becuase its a lot of extra code.
            // So we just ignore any commands sent before ready, and its part of the contract
            // that the caller (Debugger) knows when the Start msg is received that it must
            // send over initializing information such as breakpoints.
            if (Connected) {
                //var xSB = new StringBuilder();
                //foreach(byte x in aBytes) {
                //    xSB.AppendLine(x.ToString("X2"));
                //}
                //System.Windows.Forms.MessageBox.Show(xSB.ToString());

                SendRawData(aBytes);
            }
        }

        protected abstract void SendRawData(byte[] aBytes);
        protected abstract void Next(int aPacketSize, Action<byte[]> aCompleted);        
        protected abstract void PacketTracePoint(byte[] aPacket);
        protected abstract void PacketText(byte[] aPacket);

        protected const int CmdSize = 2;
        protected byte mCommandID = 0;

        protected byte[] CreateCommand(Command aCmd, int aDataSize) {
            var xResult = new byte[2 + aDataSize];
            xResult[0] = (byte)aCmd;
            if (mCommandID == 255) {
                mCommandID = 0;
            } else {
                mCommandID++;
            }
            xResult[1] = mCommandID;
            return xResult;
        }

        public void SendCommand(Command aCmd) {
            if (aCmd == Command.Noop) {
                // Noops dont have any data.
                // This is becuase Noops are used to clear out the 
                // channel and are often not received. Sending noop + data
                // usually causes the data to be interpreted as a command
                // as its often the first byte received.
                SendCommandData(new byte[1] { (byte)Command.Noop });
            } else {
                SendCommandData(CreateCommand(aCmd, 0));
            }
        }

        public void SetBreakpoint(int aID, uint aAddress) {
            var xData = CreateCommand(Command.BreakOnAddress, 5);
            Array.Copy(BitConverter.GetBytes(aAddress), 0, xData, CmdSize, 4);
            xData[CmdSize + 4] = (byte)aID;
            SendCommandData(xData);
        }

        public void DeleteBreakpoint(int aID) {
            SetBreakpoint(aID, 0);
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
                case MsgType.Started:
                    CmdStarted();
                    Next(1, PacketCommand);
                    break;
                case MsgType.Noop:
                    // MtW: When implementing Serial support for debugging on real hardware, it appears
                    //      that when booting a machine, in the bios it emits zero's to the serial port.
                    // Kudzu: Made a Noop command to handle this
                    Next(1, PacketCommand);
                    break;
                default:
                    // Exceptions crash VS.
                    MessageBox.Show("Unknown debug command");
                    break;
            }
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        protected void PacketTextSize(byte[] aPacket) {
            Next(GetUInt16(aPacket, 0), PacketText);
        }
        
    }
}
