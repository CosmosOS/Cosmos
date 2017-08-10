using HW = Cosmos.Hardware2;

namespace Cosmos.Sys.Network.TCPIP.ARP
{
    internal class ARPReply_EthernetIPv4 : ARPPacket_EthernetIPv4
    {
        internal ARPReply_EthernetIPv4(byte[] rawData)
            : base(rawData)
        {}

        internal ARPReply_EthernetIPv4(HW.Network.MACAddress ourMAC, IPv4Address ourIP, HW.Network.MACAddress targetMAC, IPv4Address targetIP)
            : base(2, ourMAC, ourIP, targetMAC, targetIP, 42)
        {}

        public override string ToString()
        {
            return "ARP Reply Src=" + srcMAC + ", Dest=" + destMAC + ", Sender=" + mSenderIP + ", Target=" + mTargetIP;
        }
    }
}
