using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Consts;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Cosmos.Debug.Common {
  public abstract class DebugConnector : IDisposable {
    public Action<Exception> ConnectionLost;
    public Action<byte, UInt32> CmdTrace;
    public Action<byte[]> CmdMethodContext;
    public Action<string> CmdText;
    public Action CmdStarted;
    public Action<string> OnDebugMsg;
    public Action<byte[]> CmdRegisters;
    public Action<byte[]> CmdFrame;
    public Action<byte[]> CmdStack;
    public Action<byte[]> CmdPong;

    protected byte mCurrentMsgType;
    protected AutoResetEvent mCmdWait = new AutoResetEvent(false);

    //        private StreamWriter mDebugWriter = new StreamWriter(@"c:\dsdebug.txt", false) { AutoFlush = true };

    // Must be called from descendant via DoConnected
    public Action Connected;
    public void DoConnected() {
      if (Connected != null) {
        Connected();
      }
    }

    protected void DoDebugMsg(string aMsg) {
      DoDebugMsg(aMsg, true);
    }

    protected void DoDebugMsg(string aMsg, bool aOnlyIfConnected) {
      if (IsConnected || aOnlyIfConnected == false) {
        System.Diagnostics.Debug.WriteLine(aMsg);
        if (OnDebugMsg != null) {
          OnDebugMsg(aMsg);
        }
      }
    }

    protected bool mSigReceived = false;
    public bool SigReceived {
      get { return mSigReceived; }
    }

    // Is stream alive? Other side may not be responsive yet, use SigReceived instead for this instead
    public abstract bool IsConnected {
      get;
    }

    protected void SendCmd(byte aCmd, byte[] aData) {
      SendCmd(aCmd, aData, true);
    }

    protected void SendCmd(byte aCmd, byte[] aData, bool aWait) {
      //System.Windows.Forms.MessageBox.Show(xSB.ToString());

      // If no sig received yet, we dont send anything. Things like BPs etc can be set before connected.
      // The debugger must resend these after the start command hits.
      // We dont queue them, as it would end up with a lot of overlapping ops, ie set and then remove.
      // We also dont check connected at caller, becuase its a lot of extra code.
      // So we just ignore any commands sent before ready, and its part of the contract
      // that the caller (Debugger) knows when the Start msg is received that it must
      // send over initializing information such as breakpoints.
      if (SigReceived) {
        // This lock is used for:
        //  1) VSDebugEngine is threaded and could send commands concurrently
        //  2) Becuase in VSDebugEngine and commands from Debug.Windows can occur concurrently
        lock (mSendCmdLock) {
          //var xSB = new StringBuilder();
          //foreach(byte x in aBytes) {
          //    xSB.AppendLine(x.ToString("X2"));
          //}
          //System.Windows.Forms.MessageBox.Show(xSB.ToString());
          DoDebugMsg("DC Send: " + aCmd.ToString());

          if (aCmd == Vs2Ds.Noop) {
            // Noops dont have any data.
            // This is becuase Noops are used to clear out the 
            // channel and are often not received. Sending noop + data
            // usually causes the data to be interpreted as a command
            // as its often the first byte received.
            SendRawData(new byte[1] { Vs2Ds.Noop });
          } else {
            var xData = new byte[aData.Length + 2];
            // See comments about flow control in the DebugStub class
            // to see why we limit to 16.
            if (aData.Length > 16) {
              throw new Exception("Command is too large. 16 bytes max.");
            }

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
              // All commands except NOOP reply back from the DebugStub
              // with an ACK. The ACK will set the event and allow us to proceed.
              // This wait causes this method to wait on the ACK to be receive back from
              // DebugStub.
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
    public void SendCmd(byte aCmd) {
      SendCmd(aCmd, new byte[0], true);
    }

    public void SendRegisters() {
      SendCmd(Vs2Ds.SendRegisters);
    }

    public void SendFrame() {
      SendCmd(Vs2Ds.SendFrame);
    }

    public void SendStack() {
      SendCmd(Vs2Ds.SendStack);
    }

    public void Ping() {
      SendCmd(Vs2Ds.Ping);
    }

    public void SetBreakpoint(int aID, uint aAddress) {
      if (aAddress == 0) {
        DoDebugMsg("DS Cmd: BP " + aID + " deleted.");
      } else {
        DoDebugMsg("DS Cmd: BP " + aID + " @ " + aAddress.ToString("X8").ToUpper());
      }

      var xData = new byte[5];
      Array.Copy(BitConverter.GetBytes(aAddress), 0, xData, 0, 4);
      xData[4] = (byte)aID;
      SendCmd(Vs2Ds.BreakOnAddress, xData);
    }

    public void SetAsmBreakpoint(uint aAddress) {
      var xData = BitConverter.GetBytes(aAddress);
      SendCmd(Vs2Ds.SetAsmBreak, xData);
    }

    public void Continue() {
      SendCmd(Vs2Ds.Continue);
    }

    public byte[] GetMemoryData(uint address, uint size, int dataElementSize = 1) {
      // from debugstub:
      //// sends a stack value
      //// Serial Params:
      ////  1: x32 - offset relative to EBP
      ////  2: x32 - size of data to send

      if (!IsConnected) {
        return null;
      } else if (size == 0) {
        // no point in retrieving 0 bytes, better not request at all. also, debugstub "crashes" then
        throw new NotSupportedException("Requested memory data of size = 0");
      } else if (size > 512) {
        // for now refuse to retrieve large amounts of data:
        throw new NotSupportedException("Too large amount of data requested");
      }
      var xData = new byte[8];
      mDataSize = (int)size;
      Array.Copy(BitConverter.GetBytes(address), 0, xData, 0, 4);
      Array.Copy(BitConverter.GetBytes(size), 0, xData, 4, 4);
      SendCmd(Vs2Ds.SendMemory, xData);
      var xResult = mData;
      mData = null;
      if (xResult.Length != size) {
        throw new Exception("Retrieved a different size than requested!");
      }
      return xResult;
    }

    public byte[] GetStackData(int offsetToEBP, uint size) {
      // from debugstub:
      //// sends a stack value
      //// Serial Params:
      ////  1: x32 - offset relative to EBP
      ////  2: x32 - size of data to send

      if (!IsConnected) {
        return null;
      }
      var xData = new byte[8];
      mDataSize = (int)size;
      ////TODO find out wherefrom this discrepancy
      //offsetToEBP --;

      // EBP is first
      //offsetToEBP += 4;

      Array.Copy(BitConverter.GetBytes(offsetToEBP), 0, xData, 0, 4);
      Array.Copy(BitConverter.GetBytes(size), 0, xData, 4, 4);
      SendCmd(Vs2Ds.SendMethodContext, xData);
      // todo: make "crossplatform". this code assumes stack space of 32bit per "item"

      byte[] xResult;

      xResult = mData;
      mData = null;
      return xResult;
    }

    private int mDataSize;
    private byte[] mData;

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
      mCurrentMsgType = aPacket[0];
      // Could change to an array, but really not much benefit
      switch (mCurrentMsgType) {
        case Ds2Vs.TracePoint:
        case Ds2Vs.BreakPoint:
          DoDebugMsg("DC Recv: TracePoint / BreakPoint");
          Next(4, PacketTracePoint);
          break;

        case Ds2Vs.Message:
          DoDebugMsg("DC Recv: Message");
          Next(2, PacketTextSize);
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
          Array.Copy(BitConverter.GetBytes(Cosmos.Debug.Consts.Consts.SerialSignature), 0, xData, 0, 4);
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

        default:
          // Exceptions crash VS so use MsgBox instead
          MessageBox.Show("Unknown debug command: " + mCurrentMsgType);
          // Despite it being unkonwn, we try again. Normally this will
          // just cause more unknowns, but can be useful for debugging.
          WaitForMessage();
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
    protected byte[] mSigCheck = new byte[4] { 0, 0, 0, 0 };
    protected void WaitForSignature(byte[] aPacket) {
      mSigCheck[0] = mSigCheck[1];
      mSigCheck[1] = mSigCheck[2];
      mSigCheck[2] = mSigCheck[3];
      mSigCheck[3] = aPacket[0];
      var xSig = GetUInt32(mSigCheck, 0);
      DoDebugMsg("DC: Sig Byte " + aPacket[0].ToString("X2").ToUpper() + " : " + xSig.ToString("X8").ToUpper());
      if (xSig == Cosmos.Debug.Consts.Consts.SerialSignature) {
        // Sig found, wait for messages
        mSigReceived = true;
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

    protected void PacketMethodContext(byte[] aPacket) {
      mData = aPacket.ToArray();
      WaitForMessage();
    }

    protected void PacketMemoryData(byte[] aPacket) {
      mData = aPacket.ToArray();
      WaitForMessage();
    }

    protected void PacketRegisters(byte[] aPacket) {
      mData = aPacket.ToArray();
      if (CmdRegisters != null) {
        CmdRegisters(mData);
      }
      WaitForMessage();
    }

    protected void PacketFrame(byte[] aPacket) {
      mData = aPacket.ToArray();
      if (CmdFrame != null) {
        CmdFrame(mData);
      }
      WaitForMessage();
    }

    protected void PacketPong(byte[] aPacket) {
      mData = aPacket;
      if (CmdPong != null) {
        CmdPong(mData);
      }
      WaitForMessage();
    }

    protected void PacketStack(byte[] aPacket) {
      mData = aPacket;
      if (CmdStack != null) {
        CmdStack(mData);
      }
      WaitForMessage();
    }

    protected void PacketCmdCompleted(byte[] aPacket) {
      byte xCmdID = aPacket[0];
      DoDebugMsg("DS Msg: Cmd " + xCmdID + " Complete");
      if (mCurrCmdID != xCmdID) {
        System.Windows.Forms.MessageBox.Show("DebugStub CmdCompleted Mismatch. Expected " + mCurrCmdID + ", received " + xCmdID + ".");
      }
      // Release command
      mCmdWait.Set();
      WaitForMessage();
    }

    protected void PacketTracePoint(byte[] aPacket) {
      // WaitForMessage must be first. CmdTrace issues
      // more commands and if we dont issue this, the pipe wont be waiting for a response.
      WaitForMessage();
      CmdTrace(mCurrentMsgType, GetUInt32(aPacket, 0));
    }

    protected void PacketText(byte[] aPacket) {
      WaitForMessage();
      CmdText(ASCIIEncoding.ASCII.GetString(aPacket));
    }
  }
}
