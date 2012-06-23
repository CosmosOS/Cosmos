using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Cosmos.Deploy.Pixie {
  public class TrivialFTP {
    public enum OpType { Read = 1, Write, Data, Ack, Error };
    protected byte[] mServerIP;
    protected string mPath;
    protected int Port = 69;
    protected UdpClient mUDP;
    protected IPEndPoint mRecvEP;
    protected IPEndPoint mDataEP;
    string mFilename;

    public TrivialFTP(byte[] aServerIP, string aPath) {
      mServerIP = aServerIP;
      mPath = aPath;

      mUDP = new UdpClient(new IPEndPoint(new IPAddress(mServerIP), Port));
      mRecvEP = new IPEndPoint(IPAddress.Any, Port);
    }

    protected void Connect() {
      OpType xOp;
      var xReader = Receive(OpType.Read);
      mDataEP = new IPEndPoint(mRecvEP.Address, mRecvEP.Port);
      mFilename = ReadString(xReader);

      string xMode = ReadString(xReader).ToLower();
      if (xMode != "octet") {
        throw new Exception("[TFTP] Expected octet.");
      }
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
        WaitForTransfer();
      }
    }

    protected void Send(byte[] aBytes) {
      mUDP.Send(aBytes, aBytes.Length, mDataEP);
    }

    protected void DoTransfer(string aPathname) {
      int xBlockSize = 512;

      using (var xFilestream = new FileStream(aPathname, FileMode.Open, FileAccess.Read)) {
        int xBlockID = 1;
        var xReader = new BinaryReader(xFilestream);

        var xPacket = new byte[xBlockSize + 4];
        // xPacket[0] MSB and is always 0
        xPacket[1] = (byte)OpType.Data;
        while (true) {
          xPacket[2] = (byte)(xBlockID / 256);
          xPacket[3] = (byte)(xBlockID % 256);

          int xCount = xReader.Read(xPacket, 4, xBlockSize);
          if (xCount < xBlockSize) {
            xPacket = xPacket.Take(xCount + 4).ToArray();
          }

          Send(xPacket);
          WaitAck(xBlockID);

          if (xCount < xBlockSize) {
            break;
          }
          xBlockID++;
        }
      }
    }

    protected void WaitForTransfer() {
      Connect();
      // TODO Read Option packet size for larger packet and faster speed

      // Possible path security issues, but this is currently only designed for Cosmos, not for other.
      string xPathname = Path.Combine(mPath, mFilename);
      if (File.Exists(xPathname)) {
        DoTransfer(xPathname);
      } else {
        SendErrFileNotFound();
      }
    }

    protected void SendErrFileNotFound() {
      var xMS = new MemoryStream();
      var xWriter = new BinaryWriter(xMS);

      xWriter.Write((byte)0);
      xWriter.Write((byte)OpType.Error);

      // File not found
      xWriter.Write((byte)0);
      xWriter.Write((byte)1);

      xWriter.Write(Encoding.ASCII.GetBytes("File not found."));
      xWriter.Write((byte)0);

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
