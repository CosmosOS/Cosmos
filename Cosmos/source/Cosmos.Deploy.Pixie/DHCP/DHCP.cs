using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Cosmos.Deploy.Pixie {
  public class DHCP {
    protected const int ServerPort = 67;
    protected const int ClientPort = 68;
    protected UdpClient mUDP;
    protected string mBootFile;

    protected byte[] mServerIP;
    protected byte[] mClientIP;
    protected IPEndPoint mRecvEndPoint;

    public void Stop() {
      mUDP.Close();
    }

    // Need full path to boot file because it needs to get the size
    public DHCP(byte[] aServerIP, string aBootFile) {
      mServerIP = aServerIP;
      mBootFile = aBootFile;

      mClientIP = (byte[])mServerIP.Clone();
      mClientIP[3] = 2;

      mUDP = new UdpClient(new IPEndPoint(new IPAddress(mServerIP), ServerPort));

      mRecvEndPoint = new IPEndPoint(IPAddress.Any, ServerPort);
    }

    protected DhcpPacket Receive(DhcpPacket.MsgType aWaitFor) {
      while (true) {
        var xData = mUDP.Receive(ref mRecvEndPoint);
        var xPacket = new DhcpPacket(xData);
        if (xPacket.Msg == aWaitFor) {
          return xPacket;
        }
      }
    }

    protected void Send(DhcpPacket aPacket) {
      var xBytes = aPacket.GetBytes();
      mUDP.Send(xBytes, xBytes.Length, new IPEndPoint(IPAddress.Broadcast, 68));
    }

    protected DhcpPacket SendOffer(DhcpPacket aDiscover) {
      var xOut = new DhcpPacket();
      xOut.Op = DhcpPacket.OpType.Reply;
      xOut.TxID = aDiscover.TxID;
      xOut.YourAddr = BitConverter.ToUInt32(mClientIP, 0);
      xOut.ServerAddr = BitConverter.ToUInt32(mServerIP, 0);
      xOut.HwAddr = aDiscover.HwAddr;
      xOut.Flags = aDiscover.Flags;
      xOut.Msg = DhcpPacket.MsgType.Offer;

      xOut.Options.Add(1, new byte[] { 255, 255, 255, 0 });
      xOut.Options.Add(54, mServerIP);
      xOut.AddTextOption(60, "PXEClient");

      xOut.BootFile = Path.GetFileName(mBootFile);
      var xFileInfo = new FileInfo(mBootFile);
      byte xBlockCount = (byte)(xFileInfo.Length / 512);
      xOut.Options.Add(13, new byte[] { 0, xBlockCount });

      Send(xOut);
      return xOut;
    }

    protected DhcpPacket SendAck(DhcpPacket aDiscover, DhcpPacket aRequest) {
      aDiscover.Msg = DhcpPacket.MsgType.Ack;
      Send(aDiscover);
      return aDiscover;
    }

    public event Action<DHCP, string> OnLog;
    protected void DoLog(string aText) {
      if (OnLog != null) {
        OnLog(this, aText);
      }
    }

    public void Execute() {
      DhcpPacket xRequest;
      DhcpPacket xReply;

      xRequest = Receive(DhcpPacket.MsgType.Discover);
      DoLog("Discover received.");

      xReply = SendOffer(xRequest);
      string xClientIP = mClientIP[0] + "." + mClientIP[1] + "." + mClientIP[2] + "." + mClientIP[3];
      DoLog("Offer sent: " + xClientIP);

      // Wait for REQUEST. We need to filter out other DISCOVER that may have been sent
      xRequest = Receive(DhcpPacket.MsgType.Request);
      DoLog("Request received.");

      xReply = SendAck(xReply, xRequest);
      DoLog("Acknowledgement sent.");
    }

  }
}
