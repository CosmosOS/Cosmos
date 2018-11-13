using Cosmos.HAL;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.ARP
{
    internal class ARPPacket : EthernetPacket
    {
        protected ushort aHardwareType;
        protected ushort aProtocolType;
        protected byte aHardwareLen;
        protected byte aProtocolLen;
        protected ushort aOperation;

        internal static void ARPHandler(byte[] packetData)
        {
            ARPPacket arp_packet = new ARPPacket(packetData);
            //Sys.Console.WriteLine("Received ARP Packet");
            //Sys.Console.WriteLine(arp_packet.ToString());
            if (arp_packet.Operation == 0x01)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    ARPRequest_Ethernet arp_request = new ARPRequest_Ethernet(packetData);
                    if (arp_request.SenderIP == null)
                    {
                        NetworkStack.debugger.Send("SenderIP null in ARPHandler!");
                    }
                    arp_request = new ARPRequest_Ethernet(packetData);

                    ARPCache.Update(arp_request.SenderIP, arp_request.SenderMAC);

                    if (NetworkStack.AddressMap.ContainsKey(arp_request.TargetIP.Hash) == true)
                    {
                        NetworkStack.debugger.Send("ARP Request Recvd from " + arp_request.SenderIP.ToString());
                        NetworkDevice nic = NetworkStack.AddressMap[arp_request.TargetIP.Hash];

                        ARPReply_Ethernet reply =
                            new ARPReply_Ethernet(nic.MACAddress, arp_request.TargetIP, arp_request.SenderMAC, arp_request.SenderIP);

                        nic.QueueBytes(reply.RawData);
                    }
                }
            }
            else if (arp_packet.Operation == 0x02)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    ARPReply_Ethernet arp_reply = new ARPReply_Ethernet(packetData);
                    NetworkStack.debugger.Send("Received ARP Reply");
                    NetworkStack.debugger.Send(arp_reply.ToString());
                    NetworkStack.debugger.Send("ARP Reply Recvd from " + arp_reply.SenderIP.ToString());
                    ARPCache.Update(arp_reply.SenderIP, arp_reply.SenderMAC);

                    OutgoingBuffer.ARPCache_Update(arp_reply);
                }
            }
        }

        /// <summary>
        /// Work around to make VMT scanner include the initFields method
        /// </summary>
        public static void VMTInclude()
        {
            new ARPPacket();
        }

        internal ARPPacket()
            : base()
        { }

        internal ARPPacket(byte[] rawData)
            : base(rawData)
        { }

        protected override void initFields()
        {
            base.initFields();
            aHardwareType = (ushort)((RawData[14] << 8) | RawData[15]);
            aProtocolType = (ushort)((RawData[16] << 8) | RawData[17]);
            aHardwareLen = RawData[18];
            aProtocolLen = RawData[19];
            aOperation = (ushort)((RawData[20] << 8) | RawData[21]);
        }

        protected ARPPacket(MACAddress dest, MACAddress src, ushort hwType, ushort protoType,
            byte hwLen, byte protoLen, ushort operation, int packet_size)
            : base(dest, src, 0x0806, packet_size)
        {
            RawData[14] = (byte)(hwType >> 8);
            RawData[15] = (byte)(hwType >> 0);
            RawData[16] = (byte)(protoType >> 8);
            RawData[17] = (byte)(protoType >> 0);
            RawData[18] = hwLen;
            RawData[19] = protoLen;
            RawData[20] = (byte)(operation >> 8);
            RawData[21] = (byte)(operation >> 0);

            initFields();
        }

        internal ushort Operation => aOperation;
        internal ushort HardwareType => aHardwareType;
        internal ushort ProtocolType => aProtocolType;

        public override string ToString()
        {
            return "ARP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", HWType=" + aHardwareType + ", Protocol=" + aProtocolType +
                ", Operation=" + Operation;
        }
    }
}
