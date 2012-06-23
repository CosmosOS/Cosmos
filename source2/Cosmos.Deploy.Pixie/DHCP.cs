using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Deploy.Pixie {
  public class DHCP {
    protected const int ServerPort = 67;
    protected UdpClient mUDP;

    public DHCP() {
      var xBindIP = new IPAddress(new byte[] { 192, 168, 42, 1 });
      var xBind = new IPEndPoint(xBindIP, ServerPort);
      mUDP = new UdpClient(xBind);

      var xEndpoint = new IPEndPoint(IPAddress.Any, ServerPort);

      // Discover
      var xData = mUDP.Receive(ref xEndpoint);
      var xIn = new DhcpPacket(xData);
      if (xIn.Msg != DhcpPacket.MsgType.Discover) {
        throw new Exception("Expected Discover");
      }

      var xOut = new DhcpPacket();
      xOut.Op = DhcpPacket.OpType.Reply;
      xOut.TxID = xIn.TxID;
      xOut.YourAddr = BitConverter.ToUInt32(new byte[] { 192, 168, 42, 2 }, 0);
      xOut.ServerAddr = BitConverter.ToUInt32(new byte[] { 192, 168, 42, 1 }, 0);
      xOut.HwAddr = xIn.HwAddr;
      xOut.Flags = xIn.Flags;
      xOut.Msg = DhcpPacket.MsgType.Offer;
      xOut.Options.Add(1, new byte[] { 255, 255, 255, 0 });
      xOut.Options.Add(51, new byte[] { 0, 0, 255, 255 });
      xOut.Options.Add(54, new byte[] { 192, 168, 42, 1 });
      xOut.Options.Add(13, new byte[] { 4, 255 });

      var xOutBytes = xOut.GetBytes();
      var xBroadcastIP = new IPAddress(new byte[] { 192, 168, 42, 255 });
      mUDP.Send(xOutBytes, xOutBytes.Length, new IPEndPoint(IPAddress.Broadcast, 68));

      while (true) {
        xData = mUDP.Receive(ref xEndpoint);
        xIn = new DhcpPacket(xData);
        if (xIn.Msg == DhcpPacket.MsgType.Discover) {
          break;
        } else if (xIn.Msg != DhcpPacket.MsgType.Request) {
          throw new Exception("Unexpected DHCP message.")
        }
      }


    }

  }
}
