using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Cosmos.Deploy.Pixie {
  public class TrivialFTP {
    public enum OpType { Read = 1, Write, Data, Ack, Error, OptionAck };
    protected byte[] mServerIP;
    protected string mPath;
    protected int Port = 69;
    protected UdpClient mUDP;
    protected IPEndPoint mRecvEP;
    protected IPEndPoint mDataEP;
    protected string mFilename;
    protected string mPathname;
    protected int mBlockSize = 512;

    public void Stop() {
      mUDP.Close();
    }

    public TrivialFTP(byte[] aServerIP, string aPath) {
      mServerIP = aServerIP;
      mPath = aPath;

      mUDP = new UdpClient(new IPEndPoint(new IPAddress(mServerIP), Port));
      mRecvEP = new IPEndPoint(IPAddress.Any, Port);
    }

    public event Action<TrivialFTP, string, long> OnFileStart;
    protected void DoFileStart(string aFilename, long aSize) {
      if (OnFileStart != null) {
        OnFileStart(this, aFilename, aSize);
      }
    }
    protected bool Connect() {
      var xReader = Receive(OpType.Read);
      mDataEP = new IPEndPoint(mRecvEP.Address, mRecvEP.Port);

      mFilename = ReadString(xReader).Replace('/', '\\');
      // Possible path security issues, but this is currently only designed for Cosmos, not for other.
      mPathname = Path.Combine(mPath, mFilename);

      string xMode = ReadString(xReader).ToLower();
      if (xMode != "octet") {
        throw new Exception("[TFTP] Expected octet.");
      }

      // Read Option Extension
      bool xTSize = false;
      if (xReader.BaseStream.Position < xReader.BaseStream.Length) {
        string xName = ReadString(xReader).ToLower();
        string xValue = ReadString(xReader);
        if (xName == "tsize") {
          xTSize = true;
        } else if (xName == "blksize") {
          mBlockSize = int.Parse(xValue);
        }
      }

      if (!File.Exists(mPathname)) {
        SendErrFileNotFound();
        return false;
      }

      // OptionAck
      var xMS = new MemoryStream();
      var xWriter = new BinaryWriter(xMS);

      WriteOp(xWriter, OpType.OptionAck);

      var xFileInfo = new FileInfo(mPathname);
      if (xTSize) {
        WriteString(xWriter, "tsize");
        WriteString(xWriter, xFileInfo.Length.ToString());
      }
      if (mBlockSize != 512) {
        WriteString(xWriter, "blksize");
        WriteString(xWriter, mBlockSize.ToString());
      }

      // Did we write any options?
      if (xWriter.BaseStream.Length > 4) {
        Send(xMS.ToArray());
        WaitAck(0);
      }

      DoFileStart(mFilename, xFileInfo.Length);
      return true;
    }

    protected void WriteOp(BinaryWriter aWriter, OpType aOp) {
      aWriter.Write((byte)0);
      aWriter.Write((byte)aOp);
    }

    protected void WriteString(BinaryWriter aWriter, string aValue) {
      aWriter.Write(Encoding.ASCII.GetBytes(aValue));
      aWriter.Write((byte)0);
    }

    protected BinaryReader Receive(OpType aOp) {
      var xData = mUDP.Receive(ref mRecvEP);
      var xReader = new BinaryReader(new MemoryStream(xData));
      
      // Op MSB which is always 0
      xReader.ReadByte();
      var xOp = (OpType)xReader.ReadByte();
      if (xOp != aOp) {
        throw new Exception("[TFTP] Expected " + aOp + ", got " + xOp + ".");
      }

      return xReader;
    }

    protected void WaitAck(int aBlockID) {
      // TFTP session is port based, but since we are only supporting one client we can reuse 69 for local port.
      //
      // We should watch out for retry read requests, but we are pretty quick and on a LAN usage like we have
      // will beat it so this works "in practice" for our scenario.
      var xReader = Receive(OpType.Ack);
      
      // Separate lines to guarantee calling order
      int xBlockID = xReader.ReadByte() * 256;
      xBlockID = xBlockID + xReader.ReadByte();

      // Currently we dont support retries, timeouts etc
      if (aBlockID != xBlockID) {
        throw new Exception("[TFTP] Block ID mismatch.");
      }
    }

    public void Execute() {
      while (true) {
        if (Connect()) {
          DoTransfer();
        }
      }
    }

    protected void Send(byte[] aBytes) {
      mUDP.Send(aBytes, aBytes.Length, mDataEP);
    }

    public event Action<TrivialFTP, string, long> OnFileTransfer;
    protected void DoFileTransfer(string aFilename, long aPosition) {
      if (OnFileTransfer != null) {
        OnFileTransfer(this, aFilename, aPosition);
      }
    }
    public event Action<TrivialFTP, string> OnFileCompleted;
    protected void DoFileCompleted(string aFilename) {
      if (OnFileCompleted != null) {
        OnFileCompleted(this, aFilename);
      }
    }
    protected void DoTransfer() {
      using (var xFilestream = new FileStream(mPathname, FileMode.Open, FileAccess.Read)) {
        int xBlockID = 1;
        var xReader = new BinaryReader(xFilestream);

        var xPacket = new byte[mBlockSize + 4];
        // xPacket[0] MSB and is always 0
        xPacket[1] = (byte)OpType.Data;
        while (true) {
          xPacket[2] = (byte)(xBlockID / 256);
          xPacket[3] = (byte)(xBlockID % 256);

          int xCount = xReader.Read(xPacket, 4, mBlockSize);
          if (xCount < mBlockSize) {
            xPacket = xPacket.Take(xCount + 4).ToArray();
          }

          Send(xPacket);
          WaitAck(xBlockID);
          DoFileTransfer(mFilename, xReader.BaseStream.Position);

          if (xCount < mBlockSize) {
            break;
          }
          xBlockID++;
        }
      }
      DoFileCompleted(mFilename);
    }

    protected void SendErrFileNotFound() {
      var xMS = new MemoryStream();
      var xWriter = new BinaryWriter(xMS);

      WriteOp(xWriter, OpType.Error);

      // File not found
      xWriter.Write((byte)0);
      xWriter.Write((byte)1);

      WriteString(xWriter, "File not found.");

      Send(xMS.ToArray());
    }

    public string ReadString(BinaryReader aReader) {
      var xResult = new List<byte>();

      while (true) {
        byte xByte = aReader.ReadByte();
        if (xByte == 0) {
          break;
        }
        xResult.Add(xByte);
      }

      return Encoding.ASCII.GetString(xResult.ToArray());
    }

  }
}
