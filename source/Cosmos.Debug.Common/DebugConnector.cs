using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Common;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Cosmos.Debug.Common
{
    /// <summary>Handles the dialog between the Debug Stub embedded in a debugged Cosmos Kernel and
    /// our Debug Engine hosted in Visual Studio. This abstract class is communication protocol
    /// independent. Sub-classes exist that manage the wire level details of the communications.
    /// </summary>
    public abstract class DebugConnector : IDisposable
    {
        public Action<Exception> ConnectionLost;
        public Action<UInt32> CmdTrace;
        public Action<UInt32> CmdBreak;
        public Action<byte[]> CmdMethodContext;
        public Action<string> CmdText;
        public Action<string> CmdMessageBox;
        public Action CmdStarted;
        public Action<string> OnDebugMsg;
        public Action<byte[]> CmdRegisters;
        public Action<byte[]> CmdFrame;
        public Action<byte[]> CmdStack;
        public Action<byte[]> CmdPong;
        public Action<byte, byte, byte[]> CmdChannel;
        public Action<UInt32> CmdStackCorruptionOccurred;
        public Action<UInt32> CmdNullReferenceOccurred;

        protected byte mCurrentMsgType;
        protected AutoResetEvent mCmdWait = new AutoResetEvent(false);

        private StreamWriter mDebugWriter = StreamWriter.Null; //new StreamWriter(@"e:\dcdebug.txt", false) { AutoFlush = true };

        // This member used to be public. The SetConnectionHandler has been added.
        private Action Connected;

        /// <summary>Descendants must invoke this method whenever they detect an incoming connection.</summary>
        public void DoConnected()
        {
            if (Connected != null)
            {
                Connected();
            }
        }

        /// <summary>Defines the handler to be invoked when a connection occurs on this condector. This
        /// method is for use by the AD7Process instance.</summary>
        /// <param name="handler">The handler to be notified when a connection occur.</param>
        public void SetConnectionHandler(Action handler)
        {
            Connected = handler;
        }

        protected virtual void DoDebugMsg(string aMsg)
        {
            mDebugWriter.WriteLine(aMsg);
            mDebugWriter.Flush();
            mOut.WriteLine(aMsg);
            mOut.Flush();
            DoDebugMsg(aMsg, true);
        }

        //private static StreamWriter mOut = new StreamWriter(@"c:\data\sources\dcoutput.txt", false)
        //                            {
        //                                AutoFlush = true
        //                            };
        private static StreamWriter mOut = StreamWriter.Null;

        protected void DoDebugMsg(string aMsg, bool aOnlyIfConnected)
        {
            if (IsConnected || aOnlyIfConnected == false)
            {
                System.Diagnostics.Debug.WriteLine(aMsg);
                if (OnDebugMsg != null)
                {
                    OnDebugMsg(aMsg);
                }
            }
        }

        protected bool mSigReceived = false;
        public bool SigReceived
        {
            get { return mSigReceived; }
        }

        // Is stream alive? Other side may not be responsive yet, use SigReceived instead for this instead
        public abstract bool IsConnected
        {
            get;
        }

        protected void SendCmd(byte aCmd, byte[] aData)
        {
            SendCmd(aCmd, aData, true);
        }

        protected virtual void BeforeSendCmd()
        {

        }

        protected void SendCmd(byte aCmd, byte[] aData, bool aWait)
        {
            //System.Windows.Forms.MessageBox.Show(xSB.ToString());

            // If no sig received yet, we dont send anything. Things like BPs etc can be set before connected.
            // The debugger must resend these after the start command hits.
            // We dont queue them, as it would end up with a lot of overlapping ops, ie set and then remove.
            // We also dont check connected at caller, becuase its a lot of extra code.
            // So we just ignore any commands sent before ready, and its part of the contract
            // that the caller (Debugger) knows when the Start msg is received that it must
            // send over initializing information such as breakpoints.
            if (SigReceived)
            {
                // This lock is used for:
                //  1) VSDebugEngine is threaded and could send commands concurrently
                //  2) Becuase in VSDebugEngine and commands from Debug.Windows can occur concurrently
                lock (mSendCmdLock)
                {
                    //var xSB = new StringBuilder();
                    //foreach(byte x in aBytes) {
                    //    xSB.AppendLine(x.ToString("X2"));
                    //}
                    //System.Windows.Forms.MessageBox.Show(xSB.ToString());
                    DoDebugMsg("DC Send: " + aCmd.ToString() + ", data.Length = " + aData.Length + ", aWait = " + aWait);

                    DoDebugMsg("Send locked...");

                    BeforeSendCmd();

                    if (aCmd == Vs2Ds.Noop)
                    {
                        // Noops dont have any data.
                        // This is becuase Noops are used to clear out the
                        // channel and are often not received. Sending noop + data
                        // usually causes the data to be interpreted as a command
                        // as its often the first byte received.
                        SendRawData(new byte[1]
                                    {
                                        Vs2Ds.Noop
                                    });
                    }
                    else
                    {
                        // +2 - Leave room for Cmd and CmdID
                        var xData = new byte[aData.Length + 2];
                        // See comments about flow control in the DebugStub class
                        // to see why we limit to 16.
                        if (aData.Length > 16)
                        {
                            throw new Exception("Command is too large. 16 bytes max.");
                        }

                        xData[0] = (byte)aCmd;
                        aData.CopyTo(xData, 2);

                        bool resetID = false;
                        if (mCommandID == 255)
                        {
                            mCommandID = 0;
                            resetID = true;
                        }
                        else
                        {
                            mCommandID++;
                        }
                        xData[1] = mCommandID;
                        mCurrCmdID = mCommandID;

                        if (SendRawData(xData))
                        {
                            if (aWait)
                            {
                                // All commands except NOOP reply back from the DebugStub
                                // with an ACK. The ACK will set the event and allow us to proceed.
                                // This wait causes this method to wait on the ACK to be receive back from
                                // DebugStub.

                                //Sometimes we get out of sync or recieve something we aren't expecting
                                //So this forces us to only return when we are back in-sync or after we think we've frozen the system for
                                //too long
                                //If we haven't gone past the command already!
                                    if ((!resetID && lastCmdCompletedID < mCommandID)
                                        || (resetID && lastCmdCompletedID > 5))
                                    {
                                        int attempts = 0;
                                        do
                                        {
                                            mCmdWait.WaitOne(2000 /*60000*/);
                                        } while ((
                                                     (!resetID && lastCmdCompletedID < mCommandID) ||
                                                     (resetID && lastCmdCompletedID > 5)
                                                 )
                                                 &&
                                                 ++attempts < 10);
                                    }
                                }
                            }
                        }
                    }

                DoDebugMsg("Send unlocked.");
            }
            else
            {
                DoDebugMsg("Tried to send command " + aCmd + ", but signature was not yet received!");
            }
        }

        protected abstract bool SendRawData(byte[] aBytes);
        protected abstract void Next(int aPacketSize, Action<byte[]> aCompleted);

        protected bool SendRawData(string aData, Encoding aEncoding = null)
        {
            if (aEncoding == null)
            {
                aEncoding = Encoding.UTF8;
            }

            if (aData == null)
            {
                return true;
            }

            var xBytes = aEncoding.GetBytes(aData);
            return SendRawData(xBytes);
        }

        protected byte mCommandID = 0;
        protected byte mCurrCmdID;

        // Prevent more than one command from happening at once.
        // The debugger is user driven so should not happen, but maybe could
        // happen while a previous command is waiting on a reply msg.
        protected object mSendCmdLock = new object();
        public void SendCmd(byte aCmd)
        {
            SendCmd(aCmd, new byte[0], true);
        }

        public void SendRegisters()
        {
            SendCmd(Vs2Ds.SendRegisters);
        }

        public void SendFrame()
        {
            SendCmd(Vs2Ds.SendFrame);
        }

        public void SendStack()
        {
            SendCmd(Vs2Ds.SendStack);
        }

        public void Ping()
        {
            SendCmd(Vs2Ds.Ping);
        }

        public void SetBreakpoint(int aID, uint aAddress)
        {
            if (aAddress == 0)
            {
                DoDebugMsg("DS Cmd: BP " + aID + " deleted.");
            }
            else
            {
                DoDebugMsg("DS Cmd: BP " + aID + " @ " + aAddress.ToString("X8").ToUpper());
            }

            var xData = new byte[5];
            Array.Copy(BitConverter.GetBytes(aAddress), 0, xData, 0, 4);
            xData[4] = (byte)aID;
            SendCmd(Vs2Ds.BreakOnAddress, xData);
        }

        public void SetAsmBreakpoint(uint aAddress)
        {
            var xData = BitConverter.GetBytes(aAddress);
            SendCmd(Vs2Ds.SetAsmBreak, xData);
        }

        public void SetINT3(uint aAddress)
        {
            var xData = BitConverter.GetBytes(aAddress);
            SendCmd(Vs2Ds.SetINT3, xData);
        }
        public void ClearINT3(uint aAddress)
        {
            var xData = BitConverter.GetBytes(aAddress);
            SendCmd(Vs2Ds.ClearINT3, xData);
        }

        public void Continue()
        {
            SendCmd(Vs2Ds.Continue);
        }

        public byte[] GetMemoryData(uint address, uint size, int dataElementSize = 1)
        {
            //return new byte[size];

            // from debugstub:
            //// sends a stack value
            //// Serial Params:
            ////  1: x32 - address
            ////  2: x32 - size of data to send

            if (!IsConnected)
            {
                return null;
            }
            else if (size == 0)
            {
                // no point in retrieving 0 bytes, better not request at all. also, debugstub "crashes" then
                throw new NotSupportedException("Requested memory data of size = 0");
            }
            else if (size > 512)
            {
                // for now refuse to retrieve large amounts of data:
                throw new NotSupportedException("Too large amount of data requested");
            }
            var xData = new byte[8];
            mDataSize = (int)size;
            Array.Copy(BitConverter.GetBytes(address), 0, xData, 0, 4);
            Array.Copy(BitConverter.GetBytes(size), 0, xData, 4, 4);
            SendCmd(Vs2Ds.SendMemory, xData);
            var xResult = MemoryDatas.First();
            MemoryDatas.RemoveAt(0);
            if (xResult.Length != size)
            {
                throw new Exception("Retrieved a different size than requested!");
            }
            return xResult;
        }

        public byte[] GetStackData(int offsetToEBP, uint size)
        {
            //return new byte[size];

            // from debugstub:
            //// sends a stack value
            //// Serial Params:
            ////  1: x32 - offset relative to EBP
            ////  2: x32 - size of data to send

            if (!IsConnected)
            {
                return null;
            }
            var xData = new byte[8];
            mDataSize = (int)size;

            // EBP is first
            //offsetToEBP += 4;

            Array.Copy(BitConverter.GetBytes(offsetToEBP), 0, xData, 0, 4);
            Array.Copy(BitConverter.GetBytes(size), 0, xData, 4, 4);
            SendCmd(Vs2Ds.SendMethodContext, xData);

            // todo: make "crossplatform". this code assumes stack space of 32bit per "item"

            byte[] xResult;

            xResult = MethodContextDatas.First();
            MethodContextDatas.RemoveAt(0);
            return xResult;
        }

        private int mDataSize;
        private List<byte[]> MethodContextDatas = new List<byte[]>();
        private List<byte[]> MemoryDatas = new List<byte[]>();

        public void DeleteBreakpoint(int aID)
        {
            SetBreakpoint(aID, 0);
        }

        protected UInt32 GetUInt32(byte[] aBytes, int aOffset)
        {
            return (UInt32)((aBytes[aOffset + 3] << 24) | (aBytes[aOffset + 2] << 16)
               | (aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }

        protected UInt16 GetUInt16(byte[] aBytes, int aOffset)
        {
            return (UInt16)((aBytes[aOffset + 1] << 8) | aBytes[aOffset + 0]);
        }

        protected void PacketMsg(byte[] aPacket)
        {
            mCurrentMsgType = aPacket[0];

            System.Diagnostics.Debug.WriteLine(String.Format("DC - PacketMsg: {0}", DebugConnectorStream.BytesToString(aPacket, 0, aPacket.Length)));
            System.Diagnostics.Debug.WriteLine("DC - " + mCurrentMsgType);
            DoDebugMsg(String.Format("DC - PacketMsg: {0}", DebugConnectorStream.BytesToString(aPacket, 0, aPacket.Length)));
            DoDebugMsg("DC - " + mCurrentMsgType);
            // Could change to an array, but really not much benefit
            switch (mCurrentMsgType)
            {
                case Ds2Vs.TracePoint:
                    DoDebugMsg("DC Recv: TracePoint");
                    Next(4, PacketTracePoint);
                    break;

                case Ds2Vs.BreakPoint:
                    DoDebugMsg("DC Recv: BreakPoint");
                    Next(4, PacketBreakPoint);
                    break;

                case Ds2Vs.Message:
                    DoDebugMsg("DC Recv: Message");
                    Next(2, PacketTextSize);
                    break;

                case Ds2Vs.MessageBox:
                    DoDebugMsg("DC Recv: MessageBox");
                    Next(2, PacketMessageBoxTextSize);
                    break;

                case Ds2Vs.Started:
                    DoDebugMsg("DC Recv: Started");
                    // Call WaitForMessage first, else it blocks because Ds2Vs.Started triggers
                    // other commands which need responses.
                    WaitForMessage();

                    // Guests never get the first byte sent. So we send a noop.
                    // This dummy byte seems to clear out the serial channel.
                    // Its never received, but if it ever is, its a noop anyways.
                    SendCmd(Vs2Ds.Noop);

                    // Send signature
                    var xData = new byte[4];
                    Array.Copy(BitConverter.GetBytes(Cosmos.Debug.Common.Consts.SerialSignature), 0, xData, 0, 4);
                    SendRawData(xData);

                    CmdStarted();
                    break;

                case Ds2Vs.Noop:
                    DoDebugMsg("DC Recv: Noop");
                    // MtW: When implementing Serial support for debugging on real hardware, it appears
                    //      that when booting a machine, in the bios it emits zero's to the serial port.
                    // Kudzu: Made a Noop command to handle this
                    WaitForMessage();
                    break;

                case Ds2Vs.CmdCompleted:
                    DoDebugMsg("DC Recv: CmdCompleted");
                    Next(1, PacketCmdCompleted);
                    break;

                case Ds2Vs.MethodContext:
                    DoDebugMsg("DC Recv: MethodContext");
                    Next(mDataSize, PacketMethodContext);
                    break;

                case Ds2Vs.MemoryData:
                    DoDebugMsg("DC Recv: MemoryData");
                    Next(mDataSize, PacketMemoryData);
                    break;

                case Ds2Vs.Registers:
                    DoDebugMsg("DC Recv: Registers");
                    Next(40, PacketRegisters);
                    break;

                case Ds2Vs.Frame:
                    DoDebugMsg("DC Recv: Frame");
                    Next(-1, PacketFrame);
                    break;

                case Ds2Vs.Stack:
                    DoDebugMsg("DC Recv: Stack");
                    Next(-1, PacketStack);
                    break;

                case Ds2Vs.Pong:
                    DoDebugMsg("DC Recv: Pong");
                    Next(0, PacketPong);
                    break;

                case Ds2Vs.StackCorruptionOccurred:
                    DoDebugMsg("DC Recv: StackCorruptionOccurred");
                    Next(4, PacketStackCorruptionOccurred);
                    break;

                case Ds2Vs.NullReferenceOccurred:
                    DoDebugMsg("DC Recv: NullReferenceOccurred");
                    Next(4, PacketNullReferenceOccurred);
                    break;
                default:
                    if (mCurrentMsgType > 128)
                    {
                        // other channels than debugstub
                        DoDebugMsg("DC Recv: Console");
                        // copy to local variable, so the anonymous method will get the correct value!
                        var xChannel = mCurrentMsgType;
                        Next(1, data => PacketOtherChannelCommand(xChannel, data));
                        break;
                    }
                    // Exceptions crash VS so use MsgBox instead
                    DoDebugMsg("Unknown debug command: " + mCurrentMsgType);
                    // Despite it being unkonwn, we try again. Normally this will
                    // just cause more unknowns, but can be useful for debugging.
                    WaitForMessage();
                    break;
            }
        }

        public virtual void Dispose()
        {

            if (mDebugWriter != null)
            {
                mDebugWriter.Dispose();
                mDebugWriter = null;
            }
            GC.SuppressFinalize(this);
        }

        // Signature is sent after garbage emitted during init of serial port.
        // For more info see note in DebugStub where signature is transmitted.
        protected byte[] mSigCheck = new byte[4] { 0, 0, 0, 0 };
        protected virtual void WaitForSignature(byte[] aPacket)
        {
            mSigCheck[0] = mSigCheck[1];
            mSigCheck[1] = mSigCheck[2];
            mSigCheck[2] = mSigCheck[3];
            mSigCheck[3] = aPacket[0];
            var xSig = GetUInt32(mSigCheck, 0);
            DoDebugMsg("DC: Sig Byte " + aPacket[0].ToString("X2").ToUpper() + " : " + xSig.ToString("X8").ToUpper());
            if (xSig == Consts.SerialSignature)
            {
                // Sig found, wait for messages
                mSigReceived = true;
                SendTextToConsole("SigReceived!");
                WaitForMessage();
            }
            else
            {
                SendPacketToConsole(aPacket);
                // Sig not found, keep looking
                Next(1, WaitForSignature);
            }
        }

        protected void SendPacketToConsole(byte[] aPacket)
        {
            CmdChannel(129, 0, aPacket);
        }

        protected void SendTextToConsole(string aText)
        {
            SendPacketToConsole(Encoding.UTF8.GetBytes(aText));
        }

        protected void WaitForMessage()
        {
            Next(1, PacketMsg);
        }

        protected void PacketTextSize(byte[] aPacket)
        {
            Next(GetUInt16(aPacket, 0), PacketText);
        }

        protected void PacketOtherChannelCommand(byte aChannel, byte[] aPacket)
        {
            Next(4, data => PacketOtherChannelSize(aChannel, aPacket[0], data));
        }

        protected void PacketOtherChannelSize(byte aChannel, byte aCommand, byte[] aPacket)
        {
            var xPacketSize = (int)GetUInt32(aPacket, 0);
            xPacketSize &= 0xFFF;
            Next(xPacketSize, data => PacketChannel(aChannel, aCommand, data));
        }

        protected void PacketMessageBoxTextSize(byte[] aPacket)
        {
            Next(GetUInt16(aPacket, 0), PacketMessageBoxText);
        }

        protected void PacketMethodContext(byte[] aPacket)
        {
            MethodContextDatas.Add(aPacket.ToArray());
            WaitForMessage();
        }

        protected void PacketMemoryData(byte[] aPacket)
        {
            MemoryDatas.Add(aPacket.ToArray());
            WaitForMessage();
        }

        protected void PacketRegisters(byte[] aPacket)
        {
            if (CmdRegisters != null)
            {
                CmdRegisters(aPacket.ToArray());
            }
            WaitForMessage();
        }

        protected void PacketFrame(byte[] aPacket)
        {
            if (CmdFrame != null)
            {
                CmdFrame(aPacket.ToArray());
            }
            WaitForMessage();
        }

        protected void PacketPong(byte[] aPacket)
        {
            if (CmdPong != null)
            {
                CmdPong(aPacket.ToArray());
            }
            WaitForMessage();
        }

        protected void PacketChannel(byte channel, byte command, byte[] aPacket)
        {
            if (CmdChannel != null)
            {
                CmdChannel(channel, command, aPacket);
            }
            WaitForMessage();
        }

        protected void PacketNullReferenceOccurred(byte[] aPacket)
        {
            if (CmdNullReferenceOccurred != null)
            {
                CmdNullReferenceOccurred(GetUInt32(aPacket, 0));
            }
            WaitForMessage();
        }

        protected void PacketStackCorruptionOccurred(byte[] aPacket)
        {
            if (CmdStackCorruptionOccurred != null)
            {
                CmdStackCorruptionOccurred(GetUInt32(aPacket, 0));
            }
            WaitForMessage();
        }

        protected void PacketStack(byte[] aPacket)
        {
            if (CmdStack != null)
            {
                CmdStack(aPacket.ToArray());
            }
            WaitForMessage();
        }

        private int lastCmdCompletedID = -1;
        protected void PacketCmdCompleted(byte[] aPacket)
        {
            byte xCmdID = aPacket[0];
            DoDebugMsg("DS Msg: Cmd " + xCmdID + " Complete");
            if (mCurrCmdID != xCmdID)
            {
                DoDebugMsg("DebugStub CmdCompleted Mismatch. Expected " + mCurrCmdID + ", received " + xCmdID + ".");
            }
            // Release command
            lastCmdCompletedID = xCmdID;
            mCmdWait.Set();
            WaitForMessage();
        }

        protected void PacketTracePoint(byte[] aPacket)
        {
            // WaitForMessage must be first. CmdTrace issues
            // more commands and if we dont issue this, the pipe wont be waiting for a response.
            WaitForMessage();
            CmdTrace(GetUInt32(aPacket, 0));
        }

        protected void PacketBreakPoint(byte[] aPacket)
        {
            WaitForMessage();
            CmdBreak(GetUInt32(aPacket, 0));
        }

        protected void PacketText(byte[] aPacket)
        {
            WaitForMessage();
            CmdText(ASCIIEncoding.ASCII.GetString(aPacket));
            //CmdChannel(129, 0, aPacket);
        }

        protected void PacketMessageBoxText(byte[] aPacket)
        {
            WaitForMessage();
            CmdMessageBox(ASCIIEncoding.ASCII.GetString(aPacket));
        }
    }
}
