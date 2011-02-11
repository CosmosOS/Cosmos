using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Compiler.Debug;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Cosmos.Debug.Common
{
    public abstract class DebugConnector: IDisposable {
        public Action<Exception> ConnectionLost;
        public Action<MsgType, UInt32> CmdTrace;
        public Action<byte[]> CmdMethodContext;
        public Action<string> CmdText;
        public Action CmdStarted;
        public Action<string> OnDebugMsg;

        protected MsgType mCurrentMsgType;
        public abstract void WaitConnect();
        protected AutoResetEvent mCmdWait = new AutoResetEvent(false);

//        private StreamWriter mDebugWriter = new StreamWriter(@"c:\dsdebug.txt", false) { AutoFlush = true };

        protected void DoDebugMsg(string aMsg) {
//            mDebugWriter.WriteLine(aMsg);
            if (OnDebugMsg != null) {
                OnDebugMsg(aMsg);
            }
        }

        public abstract bool Connected {
            get;
        }

        protected void SendCommandData(Command aCmd, byte[] aData, bool aWait) {
            //System.Windows.Forms.MessageBox.Show(xSB.ToString());
            // If not connected, we dont send anything. Things like BPs etc can be set before connected.
            // The debugger must resend these after the start command hits.
            // We dont queue them, as it would end up with a lot of overlapping ops, ie set and then remove.
            // We also dont check connected at caller, becuase its a lot of extra code.
            // So we just ignore any commands sent before ready, and its part of the contract
            // that the caller (Debugger) knows when the Start msg is received that it must
            // send over initializing information such as breakpoints.
            if (Connected) {
                lock (mSendCmdLock) {
                    //var xSB = new StringBuilder();
                    //foreach(byte x in aBytes) {
                    //    xSB.AppendLine(x.ToString("X2"));
                    //}
                    //System.Windows.Forms.MessageBox.Show(xSB.ToString());
                    DoDebugMsg("DC Send: " + aCmd.ToString());

                    if (aCmd == Command.Noop) {
                        // Noops dont have any data.
                        // This is becuase Noops are used to clear out the 
                        // channel and are often not received. Sending noop + data
                        // usually causes the data to be interpreted as a command
                        // as its often the first byte received.
                        SendRawData(new byte[1] { (byte)Command.Noop });
                    } else {
                        var xData = new byte[aData.Length + 2];
                        xData[0] = (byte)aCmd;
                        aData.CopyTo(xData, 2);

                        if (mCommandID == 255) {
                            mCommandID = 0;
                        } else {
                            mCommandID++;
                        }
                        xData[1] = mCommandID;
                        mCurrCmdID = mCommandID;

                        SendRawData(xData);
                        if (aWait) {
                            mCmdWait.WaitOne();
                        }
                    }
                }

            }
        }

        protected abstract void SendRawData(byte[] aBytes);
        protected abstract void Next(int aPacketSize, Action<byte[]> aCompleted);        

        protected byte mCommandID = 0;
        protected byte mCurrCmdID;

        // Prevent more than one command from happening at once.
        // The debugger is user driven so should not happen, but maybe could
        // happen while a previous command is waiting on a reply msg.
        protected object mSendCmdLock = new object();
        public void SendCommand(Command aCmd) {
          SendCommandData(aCmd, new byte[0], true);
        }

        public void SetBreakpoint(int aID, uint aAddress) {
            // Not needed as SendCommand will do it, but it saves
            // some execution, but more importantly stops it from 
            // logging messages to debug output for events that
            // dont happen.
            if (!Connected) {
                return;
            }

            if (aAddress == 0) {
                DoDebugMsg("DS Cmd: BP " + aID + " deleted");
            } else {
                DoDebugMsg("DS Cmd: BP " + aID + " @ " + aAddress.ToString("X8").ToUpper());
            }

            var xData = new byte[5];
            Array.Copy(BitConverter.GetBytes(aAddress), 0, xData, 0, 4);
            xData[4] = (byte)aID;
            SendCommandData(Command.BreakOnAddress, xData, true);
        }

        public byte[] GetStackData(int offsetToEBP, uint size)
        {
            // from debugstub:
            //// sends a stack value
            //// Serial Params:
            ////  1: x32 - offset relative to EBP
            ////  2: x32 - size of data to send

            if (!Connected)
            {
                return null;
            }
            var xData = new byte[8];
            mStackDataSize = (int)size;
            Array.Copy(BitConverter.GetBytes(offsetToEBP), 0, xData, 0, 4);
            Array.Copy(BitConverter.GetBytes(size), 0, xData, 4, 4);
            SendCommandData(Command.SendMethodContext, xData, true);
            var xResult = mStackData;
            mStackData = null;
            return xResult;
        }
        private int mStackDataSize;
        private byte[] mStackData;

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
        
        protected void PacketMsg(byte[] aPacket) {
            mCurrentMsgType = (MsgType)aPacket[0];
            DoDebugMsg("DC Recv: " + Enum.GetName(typeof(MsgType), mCurrentMsgType));
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
                    // Call WaitForMessage first, else it blocks becuase started triggers
                    // other commands which need responses.
                    WaitForMessage();
                    // Guests never get the first byte sent. So we send a noop.
                    // This dummy byte seems to clear out the serial channel.
                    // Its never received, but if it ever is, its a noop anyways.
                    SendCommand(Command.Noop);

                    // Send signature
                    var xData = new byte[4];
                    Array.Copy(BitConverter.GetBytes(Cosmos.Compiler.Debug.Consts.SerialSignature), 0, xData, 0, 4);
                    SendRawData(xData);

                    CmdStarted();
                    break;

                case MsgType.Noop:
                    // MtW: When implementing Serial support for debugging on real hardware, it appears
                    //      that when booting a machine, in the bios it emits zero's to the serial port.
                    // Kudzu: Made a Noop command to handle this
                    WaitForMessage();
                    break;

                case MsgType.CmdCompleted:
                    Next(1, PacketCmdCompleted);
                    break;

                case MsgType.MethodContext:
                    Next(mStackDataSize, PacketMethodContext);
                    break;

                default:
                    // Exceptions crash VS.
                    MessageBox.Show("Unknown debug command");
                    break;
            }
        }

        public virtual void Dispose() {
            //if (mDebugWriter != null)
            //{
            //    mDebugWriter.Dispose();
            //    mDebugWriter = null;
            //    
            //}
            GC.SuppressFinalize(this);
        }

        // Signature is sent after garbage emitted during init of serial port.
        // For more info see note in DebugStub where signature is transmitted.
        protected byte[] mSigCheck = new byte[4] { 0, 0, 0, 0} ;
        protected void WaitForSignature(byte[] aPacket) {
            mSigCheck[0] = mSigCheck[1];
            mSigCheck[1] = mSigCheck[2];
            mSigCheck[2] = mSigCheck[3];
            mSigCheck[3] = aPacket[0];
            var xSig = GetUInt32(mSigCheck, 0);
            DoDebugMsg("DC: Sig Byte " + aPacket[0].ToString("X2").ToUpper() + " : " + xSig.ToString("X8").ToUpper());
            if (xSig == Cosmos.Compiler.Debug.Consts.SerialSignature) {
                // Sig found, wait for messages
                WaitForMessage();
            } else {
                // Sig not found, keep looking
                Next(1, WaitForSignature);
            }
        }

        protected void WaitForMessage() {
            Next(1, PacketMsg);
        }

        protected void PacketTextSize(byte[] aPacket) {
            Next(GetUInt16(aPacket, 0), PacketText);
        }

        protected void PacketMethodContext(byte[] aPacket)
        {
            WaitForMessage(); 
            mStackData = aPacket;
             // not really nice to use this one?
            //mCmdWait.Set();
            //WaitForMessage();
        }

        protected void PacketCmdCompleted(byte[] aPacket) {
            byte xCmdID = aPacket[0];
            DoDebugMsg("DS Msg: Cmd " + xCmdID + " Complete");
            if (mCurrCmdID != xCmdID) {
                System.Windows.Forms.MessageBox.Show("DS Cmd Completed Mismatch. Expected " + mCurrCmdID + ", received " + xCmdID + ".");
            }
            // Release command
            mCmdWait.Set();
            WaitForMessage();
        }

        protected void PacketTracePoint(byte[] aPacket) {
            WaitForMessage();
            CmdTrace(mCurrentMsgType, GetUInt32(aPacket, 0));
        }

        protected void PacketText(byte[] aPacket) {
            WaitForMessage();
            CmdText(ASCIIEncoding.ASCII.GetString(aPacket));
        }
    }
}