// DO NOT remove the following line. Should you want to get rid of read count tracking comment
// out the line. Enabling the symbol will handle a thread safe counter that tracks the count
// of pending reads on the link to the DebugStub.
// #define TRACK_PENDING
using System;
using System.IO;

namespace Cosmos.Debug.DebugConnectors
{
    /// <summary>Use a Stream to implement wire transfer protocol between a Debug Stub hosted in
    /// a debugged Cosmos Kernel and our Debug Engine hosted in Visual Studio. This class is still
    /// abstract and is further refined by sub-classes handling serial communication line or pipe
    /// for example.</summary>
    public abstract class DebugConnectorStreamWithoutTimeouts: DebugConnector
    {
#if TRACK_PENDING
    /// <summary>For debugging purpose we track the number of pending read operations. Should this
    /// counter fail to 0, the connector should be considered stalled.</summary>
    private int _pendingReadsCount = 0;
#endif
        protected volatile Stream mStream;

        protected override bool SendRawData(byte[] aBytes)
        {
            bool OK = false;

            System.Diagnostics.Debug.WriteLine(String.Format("DC - sending: {0}", BytesToString(aBytes, 0, aBytes.Length)));
            if (mStream != null)
            {
                try
                {
                    mStream.Write(aBytes, 0, aBytes.Length);
                    mStream.Flush();
                    OK = true;
                }
                catch (IOException)
                {
                    //Catches the stream terminate exception
                    OK = false;
                }
            }
            else
            {
                DebugLog("Error! mStream is null! Cannot send data! Is the debugger shutting down?");
            }
            return OK;
        }

        public override bool IsConnected
        {
            get
            {
                return mStream != null;
            }
        }

        // Start is not in ctor, because for servers we have to wait for the callback. This
        // however does not prevent other kind of DebugConnectorStreamWithTimeouts descendants from
        // invoking this method from their constructor.
        protected void Start(Stream aStream)
        {
            if (aStream == null)
            {
                throw new ArgumentNullException("aStream");
            }
            mStream = aStream;
            Start();
        }

        public override void Dispose()
        {
            base.Dispose();
            // do our own dispose after base, because base dispose cleans up the thread.
            if (mStream != null)
            {
                //TODO: Change to mStream.Close() when Stream.Close() is supported
                mStream.Dispose();
                //mStream.Close();
                mStream = null;
            }
        }
    }
}
