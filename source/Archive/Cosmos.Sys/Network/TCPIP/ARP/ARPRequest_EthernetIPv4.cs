using HW = Cosmos.Hardware2;

namespace Cosmos.Sys.Network.TCPIP.ARP
{
    internal class ARPRequest_EthernetIPv4 : ARPPacket_EthernetIPv4
    {
        internal ARPRequest_EthernetIPv4(byte[] rawData)
            : base(rawData)
        {}

        internal ARPRequest_EthernetIPv4(HW.Network.MACAddress ourMAC, IPv4Address ourIP, HW.Network.MACAddress targetMAC, IPv4Address targetIP)
            : base(1, ourMAC, ourIP, targetMAC, targetIP, 42)
        { }

        public override string ToString()
        {
            return "ARP Request Src=" + srcMAC + ", Dest=" + destMAC + ", Sender=" + mSenderIP + ", Target=" + mTargetIP;
        }
    }
}
