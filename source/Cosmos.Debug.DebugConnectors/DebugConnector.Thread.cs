using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Cosmos.Debug.DebugConnectors
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
            public ManualResetEventSlim Completed;
        }

        private bool IsInBackgroundThread
        {
            get
            {
                return Thread.CurrentThread == mBackgroundThread;
            }
        }

        private CancellationTokenSource mCancellationTokenSource;
        private Thread mBackgroundThread;

        protected void Start()
        {
            mCancellationTokenSource = new CancellationTokenSource();

            mBackgroundThread = new Thread(() => ThreadMethod(mCancellationTokenSource.Token));
            mBackgroundThread.Name = "CosmosDebugConnectorBackgroundThread";

            mBackgroundThread.Start();
        }

        private readonly BlockingCollection<Incoming> mPendingReads = new BlockingCollection<Incoming>();
        private readonly BlockingCollection<Outgoing> mPendingWrites = new BlockingCollection<Outgoing>();

        private Action<byte[]> mCompletedAfterSize; // Action to call after size received
        private Incoming mIncompletePendingRead;

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
                    DebugLog(String.Format("DC - Received: 0x{0}", BytesToString(bytes, 0, bytes.Length)));
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

            DebugLog(String.Format("DC - Next: Expecting: {0}", aPacketSize) + "\r\n");
            if (xIncoming.Completed == null)
            {
                throw new InvalidOperationException("No completed!");
            }
            mPendingReads.Add(xIncoming);
        }

        protected abstract int TryRead(byte[] buffer, int offset, int count, int timeout);

        protected abstract bool GetIsConnectedToDebugStub();

        private void ThreadMethod(CancellationToken aCancellationToken)
        {
            // todo: error handling
            mIncompletePendingRead = null;
            try
            {
                InitializeBackground();
                DoConnected();
                Next(1, WaitForSignature);
                while (true)
                {
                    aCancellationToken.ThrowIfCancellationRequested();

                    if (!GetIsConnectedToDebugStub())
                    {
                        ConnectionLost(null);
                        return;
                    }
                    ProcessPendingActions();
                }
            }
            catch (OperationCanceledException)
            {
                while (true)
                {
                    Outgoing xPendingOutgoing;
                    if (!mPendingWrites.TryTake(out xPendingOutgoing))
                    {
                        break;
                    }
                    xPendingOutgoing.Packet = null;
                    xPendingOutgoing.Completed.Set();
                }

                return;
            }
            catch (Exception E)
            {
                CmdMessageBox("Error occurred in DebugConnector.ThreadMethod: " + E.ToString());
            }
            ConnectionLost(null);
        }

        private void ProcessPendingActions()
        {
            Incoming xPendingRead = mIncompletePendingRead;
            if (xPendingRead != null || mPendingReads.TryTake(out xPendingRead, 5))
            {
                var xNrOfBytesToRead = xPendingRead.Packet.Length - xPendingRead.CurrentPos;
                var xBytesRead = TryRead(xPendingRead.Packet, xPendingRead.CurrentPos, xNrOfBytesToRead, 5);

                xPendingRead.CurrentPos += xBytesRead;

                if (xPendingRead.CurrentPos == xPendingRead.Packet.Length)
                {
                    // Full packet received, process it
                    xPendingRead.Completed(xPendingRead.Packet);
                    mIncompletePendingRead = null;
                }
                else
                {
                    mIncompletePendingRead = xPendingRead;
                }
            }

            //process_writes:

            // now process sends.
            Outgoing xPendingWrite;
            if (mPendingWrites.TryTake(out xPendingWrite, 5))
            {
                if (!SendRawData(xPendingWrite.Packet))
                {
                    throw new Exception("SendRawData returned false!");
                }
                if (xPendingWrite.Completed != null)
                {
                    xPendingWrite.Completed.Set();
                }
            }
        }
    }
}
