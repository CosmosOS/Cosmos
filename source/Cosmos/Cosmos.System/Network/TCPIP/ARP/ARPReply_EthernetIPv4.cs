using HW = Cosmos.Hardware;

namespace Cosmos.Sys.Network.TCPIP.ARP
{
    public class ARPReply_EthernetIPv4 : ARPPacket_EthernetIPv4
    {
        public ARPReply_EthernetIPv4(byte[] rawData)
            : base(rawData)
        {}

        public ARPReply_EthernetIPv4(HW.Network.MACAddress ourMAC, IPv4Address ourIP, HW.Network.MACAddress targetMAC, IPv4Address targetIP)
            : base(2, ourMAC, ourIP, targetMAC, targetIP, 42)
        {}
    }
}
