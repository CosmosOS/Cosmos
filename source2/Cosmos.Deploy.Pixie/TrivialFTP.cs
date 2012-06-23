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
    protected IPEndPoint mRecvEndPoint;
    string mFilename;

    public TrivialFTP(byte[] aServerIP, string aPath) {
      mServerIP = aServerIP;
      mPath = aPath;

      mUDP = new UdpClient(new IPEndPoint(new IPAddress(mServerIP), Port));
      mRecvEndPoint = new IPEndPoint(IPAddress.Any, Port);
    }

    protected void ProcessRequest() {
      var xData = mUDP.Receive(ref mRecvEndPoint);
      var xReader = new BinaryReader(new MemoryStream(xData));

      // Op MSB which is always 0
      xReader.ReadByte();
      var xOp = (OpType)xReader.ReadByte();
      if (xOp != OpType.Read) {
        throw new Exception("[TFTP] Expected read");
      }

      mFilename = ReadString(xReader);

      string xMode = ReadString(xReader).ToLower();
      if (xMode != "octet") {
        throw new Exception("[TFTP] Expected octet");
      }
    }

    public void Execute() {
      ProcessRequest();

      // TODO Read Option packet size for larger packet and faster speed

      var xDataEP = new IPEndPoint(mRecvEndPoint.Address, mRecvEndPoint.Port);
      int xBlockSize = 512;
      var xData = new byte[xBlockSize + 4];

      // xData[0] MSB is 0
      xData[1] = (byte)OpType.Data;
      
      // xData[2] MSB is 0
      xData[3] = 1;

      mUDP.Send(xData, xData.Length, xDataEP);
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
