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

        private readonly ConcurrentQueue<Incoming> mPendingReads = new ConcurrentQueue<Incoming>();
        private readonly ConcurrentQueue<KeyValuePair<byte[], AutoResetEvent>> mPendingWrites = new ConcurrentQueue<KeyValuePair<byte[], AutoResetEvent>>();

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
            mPendingReads.Enqueue(xIncoming);
        }

        protected abstract int Read(byte[] buffer, int offset, int count);

        private void ThreadMethod()
        {
            // todo: error handling
            InitializeBackground();
            DoConnected();
            Next(1, WaitForSignature);
            while (true)
            {
                var xAnythingDone = false;
                do
                {
                    xAnythingDone = false;
                    Incoming xPendingRead;
                    if (mPendingReads.TryDequeue(out xPendingRead))
                    {
                        while (xPendingRead.CurrentPos < xPendingRead.Packet.Length)
                        {
                            xPendingRead.CurrentPos += Read(xPendingRead.Packet, xPendingRead.CurrentPos, xPendingRead.Packet.Length - xPendingRead.CurrentPos);
                        }

                        // Full packet received, process it
                        xPendingRead.Completed(xPendingRead.Packet);
                        xAnythingDone = true;
                    }

                    // now process sends.
                    KeyValuePair<byte[], AutoResetEvent> xPendingWrite;
                    if (mPendingWrites.TryDequeue(out xPendingWrite))
                    {
                        if (!SendRawData(xPendingWrite.Key))
                        {
                            throw new Exception("SendRawData returned false!");
                        }
                        xPendingWrite.Value.Set();
                        xAnythingDone = true;
                    }
                }
                while (xAnythingDone);
                Thread.Sleep(25);
            }
        }
    }
}
