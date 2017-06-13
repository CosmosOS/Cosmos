using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.Debug.DebugConnectors
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

        private bool RawSendHelper(byte[] aData, bool aWait)
        {
            if (IsInBackgroundThread)
            {
                return SendRawData(aData);
            }
            if (!IsConnected)
            {
                return false;
            }
            if (aWait)
            {
                using (var xEvent = new ManualResetEventSlim(false))
                {
                    mPendingWrites.Add(new Outgoing {Packet = aData, Completed = xEvent});
                    while (IsConnected)
                    {
                        if (xEvent.Wait(25))
                        {
                            break;
                        }
                    }
                    return IsConnected; // ??
                }
            }
            else
            {
                mPendingWrites.Add(new Outgoing {Packet = aData});
                return true;
            }

        }

        private byte? mCurrentSendingCmd;
        private byte[] mCurrentSendingData;
        private string mCurrentSendingStackTrace;
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
                    try
                    {
                        // for debug:
                        mCurrentSendingCmd = aCmd;
                        mCurrentSendingData = aData;
                        mCurrentSendingStackTrace = Environment.StackTrace;
                        // end - for debug


                        DebugLog("DC Send: " + aCmd.ToString() + ", data.Length = " + aData.Length + ", aWait = " + aWait);
                        DebugLog("Send locked...");
                        if (IsInBackgroundThread)
                        {
                            DebugLog("In background thread already");
                        }

                        BeforeSendCmd();

                        if (aCmd == Vs2Ds.Noop)
                        {
                            // Noops dont have any data.
                            // This is becuase Noops are used to clear out the
                            // channel and are often not received. Sending noop + data
                            // usually causes the data to be interpreted as a command
                            // as its often the first byte received.

                            RawSendHelper(new byte[1]
                                          {
                                              Vs2Ds.Noop
                                          }, aWait);
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

                            if (RawSendHelper(xData, aWait))
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
                                            mCmdWait.WaitOne(200 /*60000*/);

                                            if (IsInBackgroundThread)
                                            {
                                                // we're running inside our message loop (see ThreadMethod). Which means we need to kick it off once, to allow
                                                // it to read new stuff.
                                                ProcessPendingActions();
                                            }
                                        }
                                        while ((
                                                   (!resetID && lastCmdCompletedID < mCommandID) ||
                                                   (resetID && lastCmdCompletedID > 5)
                                               )
                                               &&
                                               ++attempts < 100);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        mCurrentSendingCmd = null;
                        mCurrentSendingData = null;
                        mCurrentSendingStackTrace = null;
                    }
                }

                DebugLog("Send unlocked.");
            }
            else
            {
                DebugLog("Tried to send command " + aCmd + ", but signature was not yet received!");
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


    }
}
