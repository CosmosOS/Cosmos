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

namespace Cosmos.Debug.Common {
  /// <summary>Use a Stream to implement wire transfer protocol between a Debug Stub hosted in
  /// a debugged Cosmos Kernel and our Debug Engine hosted in Visual Studio. This class is still
  /// abstract and is further refined by sub-classes handling serial communication line or pipe
  /// for example.</summary>
  public abstract class DebugConnectorStream : DebugConnector {
#if TRACK_PENDING
    /// <summary>For debugging purpose we track the number of pending read operations. Should this
    /// counter fail to 0, the connector should be considered stalled.</summary>
    private int _pendingReadsCount = 0;
#endif
    private Stream mStream;

    protected class Incoming {
      public Stream Stream;
      // Buffer to hold incoming message
      public byte[] Packet;
      // Current # of bytes in mPacket
      public int CurrentPos = 0;
      public Action<byte[]> Completed;
    }

    private static string BytesToString(byte[] bytes, int index, int count) {
      if (count > 100) {
        return String.Empty;
      }
      var xSB = new StringBuilder(2 + (bytes.Length * 2));
      xSB.Append("0x");
      for (int i = index; i < index + count; i++) {
        xSB.Append(bytes[i].ToString("X2").ToString());
      }
      return xSB.ToString();
    }

    protected override void SendRawData(byte[] aBytes) {
      System.Diagnostics.Debug.WriteLine(String.Format("DC - sending: {0}", BytesToString(aBytes, 0, aBytes.Length)));
      mStream.Write(aBytes, 0, aBytes.Length);
    }

    public override bool IsConnected {
      get { return mStream != null; }
    }

    // Start is not in ctor, because for servers we have to wait for the callback. This
    // however does not prevent other kind of DebugConnectorStream descendants from
    // invoking this method from their constructor.
    protected void Start(Stream aStream) {
      DoConnected();
      mStream = aStream;
      Next(1, WaitForSignature);
    }

    public override void Dispose() {
      if (mStream != null) {
        mStream.Close();
        mStream = null;
      }
      base.Dispose();
    }

    protected Action<byte[]> mCompletedAfterSize; // Action to call after size received
    protected void SizePacket(byte[] aPacket) {
      int xSize = aPacket[0] + (aPacket[1] << 8);
      Next(xSize, mCompletedAfterSize);
    }

    protected override void Next(int aPacketSize, Action<byte[]> aCompleted) {
      var xIncoming = new Incoming();
      if (aPacketSize == 0) {
        // Can occur with variable size packets for exampmle.
        // Dont call read, becuase that will close the stream.
        // So we just call the Completed directly
        aCompleted(new byte[0]);
      } else if (aPacketSize == -1) {
        // Variable size packet, split into two reads
        mCompletedAfterSize = aCompleted;
        aPacketSize = 2;
        xIncoming.Completed = SizePacket;
      } else {
        xIncoming.Completed = aCompleted;
      }
      xIncoming.Packet = new byte[aPacketSize];
      xIncoming.Stream = mStream;
#if TRACK_PENDING
      try {
        Interlocked.Increment(ref _pendingReadsCount);
#endif
        mStream.BeginRead(xIncoming.Packet, 0, aPacketSize, new AsyncCallback(DoRead), xIncoming);
#if TRACK_PENDING
      }
      catch (Exception e) {
        Interlocked.Decrement(ref _pendingReadsCount);
        throw;
      }
#endif
    }

    protected void DoRead(IAsyncResult aResult) {
      try {
        var xIncoming = (Incoming)aResult.AsyncState;
        int xCount = xIncoming.Stream.EndRead(aResult);

        System.Diagnostics.Debug.WriteLine(String.Format("DC - Received: {0}", BytesToString(xIncoming.Packet, xIncoming.CurrentPos, xCount)));
        xIncoming.CurrentPos += xCount;
        if (xCount == 0) {
          // If 0, end of stream then just exit without calling BeginRead again
          return;
        }
        else if (xIncoming.CurrentPos < xIncoming.Packet.Length) {
          // Packet is not full yet, read more data
#if TRACK_PENDING
          try {
            Interlocked.Increment(ref _pendingReadsCount);
#endif
            xIncoming.Stream.BeginRead(xIncoming.Packet, xIncoming.CurrentPos
                  , xIncoming.Packet.Length - xIncoming.CurrentPos
                  , new AsyncCallback(DoRead), xIncoming);
#if TRACK_PENDING
          }
          catch (Exception e) {
            Interlocked.Decrement(ref _pendingReadsCount);
            throw;
          }
#endif
        }
        else {
          // Full packet received, process it
          xIncoming.Completed(xIncoming.Packet);
        }
      }
      catch (System.IO.IOException ex) {
        ConnectionLost(ex);
      }
#if TRACK_PENDING
      finally {
        Interlocked.Decrement(ref _pendingReadsCount);
      }
#endif
    }
  }
}
