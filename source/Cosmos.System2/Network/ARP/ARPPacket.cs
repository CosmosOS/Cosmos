using System;
using Cosmos.HAL.Network;
using Sys = System;
using Cosmos.HAL;

namespace Cosmos.System.Network.ARP
{
    internal class ARPPacket : EthernetPacket
    {
        protected UInt16 aHardwareType;
        protected UInt16 aProtocolType;
        protected byte aHardwareLen;
        protected byte aProtocolLen;
        protected UInt16 aOperation;

        internal static void ARPHandler(byte[] packetData)
        {
            ARPPacket arp_packet = new ARPPacket(packetData);
            //Sys.Console.WriteLine("Received ARP Packet");
            //Sys.Console.WriteLine(arp_packet.ToString());
            if (arp_packet.Operation == 0x01)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    IPv4.ARPRequest_Ethernet arp_request = new IPv4.ARPRequest_Ethernet(packetData);
                    if (arp_request.SenderIP == null)
                    {
                        global::System.Console.WriteLine("SenderIP null in ARPHandler!");
                    }
                    arp_request = new IPv4.ARPRequest_Ethernet(packetData);
                    
                    ARPCache.Update(arp_request.SenderIP, arp_request.SenderMAC);

                    if (NetworkStack.AddressMap.ContainsKey(arp_request.TargetIP.Hash) == true)
                    {
                        //Sys.Console.WriteLine("ARP Request Recvd from " + arp_request.SenderIP.ToString());
                        NetworkDevice nic = NetworkStack.AddressMap[arp_request.TargetIP.Hash];

                        IPv4.ARPReply_Ethernet reply =
                            new IPv4.ARPReply_Ethernet(nic.MACAddress, arp_request.TargetIP, arp_request.SenderMAC, arp_request.SenderIP);

                        nic.QueueBytes(reply.RawData);
                    }
                }
            }
            else if (arp_packet.Operation == 0x02)
            {
                if ((arp_packet.HardwareType == 1) && (arp_packet.ProtocolType == 0x0800))
                {
                    IPv4.ARPReply_Ethernet arp_reply = new IPv4.ARPReply_Ethernet(packetData);
                    //Sys.Console.WriteLine("Received ARP Reply");
                    //Sys.Console.WriteLine(arp_reply.ToString());
                    //Sys.Console.WriteLine("ARP Reply Recvd from " + arp_reply.SenderIP.ToString());
                    ARPCache.Update(arp_reply.SenderIP, arp_reply.SenderMAC);

                    IPv4.OutgoingBuffer.ARPCache_Update(arp_reply);
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
            aHardwareType = (UInt16)((mRawData[14] << 8) | mRawData[15]);
            aProtocolType = (UInt16)((mRawData[16] << 8) | mRawData[17]);
            aHardwareLen = mRawData[18];
            aProtocolLen = mRawData[19];
            aOperation = (UInt16)((mRawData[20] << 8) | mRawData[21]);
        }

        protected ARPPacket(MACAddress dest, MACAddress src, UInt16 hwType, UInt16 protoType,
            byte hwLen, byte protoLen, UInt16 operation, int packet_size)
            : base(dest, src, 0x0806, packet_size)
        {
            mRawData[14] = (byte)(hwType >> 8);
            mRawData[15] = (byte)(hwType >> 0);
            mRawData[16] = (byte)(protoType >> 8);
            mRawData[17] = (byte)(protoType >> 0);
            mRawData[18] = hwLen;
            mRawData[19] = protoLen;
            mRawData[20] = (byte)(operation >> 8);
            mRawData[21] = (byte)(operation >> 0);

            initFields();
        }

        internal UInt16 Operation
        {
            get { return this.aOperation; }
        }
        internal UInt16 HardwareType
        {
            get { return this.aHardwareType; }
        }
        internal UInt16 ProtocolType
        {
            get { return this.aProtocolType; }
        }

        public override string ToString()
        {
            return "ARP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", HWType=" + aHardwareType + ", Protocol=" + aProtocolType +
                ", Operation=" + Operation;
        }
    }
}
