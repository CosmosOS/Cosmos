using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Deploy.Pixie {
  public class BOOTP {
    protected const int ServerPort = 67;
    protected UdpClient mUDP;

    public BOOTP() {
      var xBindIP = new IPAddress(new byte[] { 192, 168, 42, 1 });
      var xBind = new IPEndPoint(xBindIP, ServerPort);
      mUDP = new UdpClient(xBind);

      var xEndpoint = new IPEndPoint(IPAddress.Any, ServerPort);
      var xData = mUDP.Receive(ref xEndpoint);
      var xIn = new BootpPacket(xData);

      var xOut = new BootpPacket();
      xOut.Op = 2;
      xOut.TxID = xIn.TxID;
      xOut.YourAddr = BitConverter.ToUInt32(new byte[] { 192, 168, 42, 2 }, 0);
      xOut.ServerAddr = BitConverter.ToUInt32(new byte[] { 192, 168, 42, 1 }, 0);
      xOut.HwAddr = xIn.HwAddr;

      var xOutBytes = xOut.GetBytes();
      var xBroadcastIP = new IPAddress(new byte[] { 192, 168, 42, 255 });
      mUDP.Send(xOutBytes, xOutBytes.Length, new IPEndPoint(xBroadcastIP, 68));
    }

  }
}
