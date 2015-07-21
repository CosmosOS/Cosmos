using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Debug.Common
{
    partial class DebugConnector
    {
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

        protected byte mCommandID = 0;
        protected byte mCurrCmdID;

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
    }
}
