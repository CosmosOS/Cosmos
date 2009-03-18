using HW = Cosmos.Hardware;

namespace Cosmos.Sys.Network.TCPIP.ARP
{
    public class ARPRequest_EthernetIPv4 : ARPPacket_EthernetIPv4
    {
        public ARPRequest_EthernetIPv4(byte[] rawData)
            : base(rawData)
        {}

        public ARPRequest_EthernetIPv4(HW.Network.MACAddress ourMAC, IPv4Address ourIP, HW.Network.MACAddress targetMAC, IPv4Address targetIP)
            : base(1, ourMAC, ourIP, targetMAC, targetIP, 42)
        { }
    }
}
