using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Deploy.Pixie {
  public class DHCP {
    protected const int ServerPort = 67;
    protected const int ClientPort = 68;
    protected UdpClient mUDP;

    protected byte[] mServerIP;
    protected byte[] mClientIP;
    protected byte[] mBroadcastIP;
    protected IPEndPoint mRecvEndPoint;

    public DHCP(byte aNet) {
      mServerIP = new byte[] { 192, 168, aNet, 1 };
      mClientIP = new byte[] { 192, 168, aNet, 2 };
      // Certain DHCP clients require specific subnet broadcast so this is safer than 255.255.255.255
      mBroadcastIP = new byte[] { 192, 168, aNet, 255 };

      mUDP = new UdpClient(new IPEndPoint(new IPAddress(mServerIP), ServerPort));

      mRecvEndPoint = new IPEndPoint(IPAddress.Any, ServerPort);
    }

    protected DhcpPacket Receive() {
      var xData = mUDP.Receive(ref mRecvEndPoint);
      return new DhcpPacket(xData);
    }

    protected void Send(DhcpPacket aPacket) {
      var xBytes = aPacket.GetBytes();
      mUDP.Send(xBytes, xBytes.Length, new IPEndPoint(new IPAddress(mBroadcastIP), 68));
    }

    public void Execute() {
      // Discover
      var xIn = Receive();
      if (xIn.Msg != DhcpPacket.MsgType.Discover) {
        throw new Exception("Expected Discover");
      }

      var xOut = new DhcpPacket();
      xOut.Op = DhcpPacket.OpType.Reply;
      xOut.TxID = xIn.TxID;
      xOut.YourAddr = BitConverter.ToUInt32(mClientIP, 0);
      xOut.ServerAddr = BitConverter.ToUInt32(mServerIP, 0);
      xOut.HwAddr = xIn.HwAddr;
      xOut.Flags = xIn.Flags;
      xOut.Msg = DhcpPacket.MsgType.Offer;
      xOut.Options.Add(1, new byte[] { 255, 255, 255, 0 });
      xOut.Options.Add(51, new byte[] { 255, 255, 255, 255 });
      xOut.Options.Add(54, mServerIP);

      // Intel PXE ROM wont send REQUEST unless there is a boot file
      xOut.BootFile = "Test";
      xOut.Options.Add(13, new byte[] { 4, 255 });

      Send(xOut);

      // Wait for REQUEST
      while (true) {
        xIn = Receive();
        if (xIn.Msg == DhcpPacket.MsgType.Discover) {
          break;
        } else if (xIn.Msg != DhcpPacket.MsgType.Request) {
          throw new Exception("Unexpected DHCP message.");
        }
      }

      // ACK
      xOut.Msg = DhcpPacket.MsgType.Ack;
      Send(xOut);

      mUDP = new UdpClient(new IPEndPoint(new IPAddress(mServerIP), 69));
      var xRecvEndPoint = new IPEndPoint(IPAddress.Any, ServerPort);
      var xTFTP = mUDP.Receive(ref xRecvEndPoint);
      int xxx = 4;
    }

  }
}
