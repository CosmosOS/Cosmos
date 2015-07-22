// DO NOT remove the following line. Should you want to get rid of read count tracking comment
// out the line. Enabling the symbol will handle a thread safe counter that tracks the count
// of pending reads on the link to the DebugStub.
// #define TRACK_PENDING
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosmos.Debug.Common
{
    /// <summary>Use a Stream to implement wire transfer protocol between a Debug Stub hosted in
    /// a debugged Cosmos Kernel and our Debug Engine hosted in Visual Studio. This class is still
    /// abstract and is further refined by sub-classes handling serial communication line or pipe
    /// for example.</summary>
    public abstract class DebugConnectorStream: DebugConnector
    {
#if TRACK_PENDING
    /// <summary>For debugging purpose we track the number of pending read operations. Should this
    /// counter fail to 0, the connector should be considered stalled.</summary>
    private int _pendingReadsCount = 0;
#endif
        protected Stream mStream;

        protected override int TryRead(byte[] buffer, int offset, int count, int timeout)
        {
            //mStream.ReadTimeout = timeout;
            return mStream.Read(buffer, offset, count);
        }

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
                DoDebugMsg("Error! mStream is null! Cannot send data! Is the debugger shutting down?");
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
        // however does not prevent other kind of DebugConnectorStream descendants from
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
            if (mStream != null)
            {
                mStream.Close();
                mStream = null;
            }
            base.Dispose();
        }

        private List<Incoming> ReadQueue = new List<Incoming>();
        private bool reading = false;
    }
}
