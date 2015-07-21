using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Cosmos.Debug.Common
{
    partial class DebugConnector
    {
        protected class Incoming
        {
            // Buffer to hold incoming message
            public byte[] Packet;
            // Current # of bytes in mPacket
            public int CurrentPos = 0;
            public Action<byte[]> Completed;
        }

        protected class Outgoing
        {
            // Buffer to hold outcoming message
            public byte[] Packet;

            // signal completion
            public AutoResetEvent Completed;
        }

        private bool IsInBackgroundThread
        {
            get
            {
                return Thread.CurrentThread == mBackgroundThread;
            }
        }

        private Thread mBackgroundThread;

        protected void Start()
        {
            mBackgroundThread = new Thread(ThreadMethod);
            mBackgroundThread.Name = "CosmosDebugConnectorBackgroundThread";

            mBackgroundThread.Start();
        }

        private readonly BlockingCollection<Object> mPendingActions = new BlockingCollection<object>();

        private Action<byte[]> mCompletedAfterSize; // Action to call after size received

        protected abstract void InitializeBackground();

        protected void Next(int aPacketSize, Action<byte[]> aCompleted)
        {
            if (aCompleted == null)
            {
                throw new ArgumentNullException("aCompleted");
            }
            var xIncoming = new Incoming();
            if (aPacketSize == 0)
            {
                // Can occur with variable size packets for exampmle.
                // Dont call read, becuase that will close the stream.
                // So we just call the Completed directly
                aCompleted(new byte[0]);
                return;
            }
            else if (aPacketSize == -1)
            {
                // Variable size packet, split into two reads
                mCompletedAfterSize = aCompleted;
                aPacketSize = 2;
                xIncoming.Completed = SizePacket;
            }
            else
            {
                xIncoming.Completed = bytes =>
                {
                    DoDebugMsg(String.Format("DC - Received: 0x{0}", BytesToString(bytes, 0, bytes.Length)));
                    try
                    {
                        aCompleted(bytes);
                    }
                    catch (Exception E)
                    {
                        HandleError(E);
                    }
                };
            }
            if (aPacketSize > (1024 * 1024))
            {
                throw new Exception("Safety exception. Receiving " + aPacketSize + " bytes!");
            }
            xIncoming.Packet = new byte[aPacketSize];

            DoDebugMsg(String.Format("DC - Next: Expecting: {0}", aPacketSize) + "\r\n");
            if (xIncoming.Completed == null)
            {
                throw new InvalidOperationException("No completed!");
            }
            mPendingActions.Add(xIncoming);
        }

        protected abstract int Read(byte[] buffer, int offset, int count);

        protected abstract bool GetIsConnectedToDebugStub();

        private void ThreadMethod()
        {
            // todo: error handling
            InitializeBackground();
            DoConnected();
            Next(1, WaitForSignature);
            while (true)
            {
                if (!GetIsConnectedToDebugStub())
                {
                    ConnectionLost(null);
                    return;
                }
                object xPendingAction;
                if (!mPendingActions.TryTake(out xPendingAction, 100))
                {
                    break;
                }
                var xPendingRead = xPendingAction as Incoming;
                if (xPendingRead != null)
                {
                    while (xPendingRead.CurrentPos < xPendingRead.Packet.Length)
                    {
                        xPendingRead.CurrentPos += Read(xPendingRead.Packet, xPendingRead.CurrentPos, xPendingRead.Packet.Length - xPendingRead.CurrentPos);
                    }

                    // Full packet received, process it
                    xPendingRead.Completed(xPendingRead.Packet);
                    continue;
                }

                // now process sends.
                Outgoing xPendingWrite = xPendingAction as Outgoing;
                if (xPendingWrite != null)
                {
                    if (!SendRawData(xPendingWrite.Packet))
                    {
                        throw new Exception("SendRawData returned false!");
                    }
                    xPendingWrite.Completed.Set();
                    continue;
                }

                throw new Exception("Pending action '" + xPendingAction.GetType().FullName + "' not implemented!");
            }
        }
    }
}
