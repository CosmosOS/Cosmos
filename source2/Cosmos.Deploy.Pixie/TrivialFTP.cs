using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Deploy.Pixie {
  public class TrivialFTP {
    protected byte[] mServerIP;
    protected string mPath;
    protected int Port = 69;
    protected UdpClient mUDP;
    protected IPEndPoint mRecvEndPoint;

    public TrivialFTP(byte[] aServerIP, string aPath) {
      mServerIP = aServerIP;
      mPath = aPath;

      mUDP = new UdpClient(new IPEndPoint(new IPAddress(mServerIP), Port));
      mRecvEndPoint = new IPEndPoint(IPAddress.Any, Port);
    }

    protected TrivialFtpPacket Receive() {
      var xData = mUDP.Receive(ref mRecvEndPoint);
      return new TrivialFtpPacket(xData);
    }

    public void Execute() {
      TrivialFtpPacket xPacket;

      xPacket = Receive();
      if (xPacket.Op != TrivialFtpPacket.OpType.Read) {
        throw new Exception("[TFTP] Expected read");
      } else if (xPacket.Mode.ToLower() != "octet") {
        throw new Exception("[TFTP] Expected octet");
      }

      // TODO Read Option packet size for larger packet and faster speed
    }
  }
}
