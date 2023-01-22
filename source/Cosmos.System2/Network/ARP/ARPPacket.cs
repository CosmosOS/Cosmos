/*
* PROJECT:          Aura Operating System Development
* CONTENT:          ARP Packet
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Cosmos.HAL;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.ARP
{
    /// <summary>
    /// ARPPacket class. See also: <seealso cref="EthernetPacket"/>.
    /// </summary>
    public class ARPPacket : EthernetPacket
    {
        /// <summary>
        /// Hardware type.
        /// </summary>
        protected ushort aHardwareType;
        /// <summary>
        /// Protocol type.
        /// </summary>
        protected ushort aProtocolType;
        /// <summary>
        /// Hardware address length.
        /// </summary>
        protected byte aHardwareLen;
        /// <summary>
        /// Protocol address length.
        /// </summary>
        protected byte aProtocolLen;
        /// <summary>
        /// Operation code.
        /// </summary>
        protected ushort aOperation;

        /// <summary>
        /// ARP handler.
        /// </summary>
        /// <param name="packetData">Packet data.</param>
        /// <exception cref="sys.ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="sys.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="sys.ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="sys.OverflowException">Thrown on fatal error (contact support).</exception>
        internal static void ARPHandler(byte[] packetData)
        {
            ARPPacket arp_packet = new ARPPacket(packetData);
            //Sys.Console.WriteLine("Received ARP Packet");
            //Sys.Console.WriteLine(arp_packet.ToString());
            if (arp_packet.Operation == 0x01)
            {
                if (arp_packet.HardwareType == 1 && arp_packet.ProtocolType == 0x0800)
                {
                    ARPRequest_Ethernet arp_request = new ARPRequest_Ethernet(packetData);
                    if (arp_request.SenderIP == null)
                    {
                        Global.mDebugger.Send("SenderIP null in ARPHandler!");
                    }
                    arp_request = new ARPRequest_Ethernet(packetData);

                    ARPCache.Update(arp_request.SenderIP, arp_request.SenderMAC);

                    if (NetworkStack.AddressMap.ContainsKey(arp_request.TargetIP.Hash) == true)
                    {
                        Global.mDebugger.Send("ARP Request Recvd from " + arp_request.SenderIP.ToString());
                        NetworkDevice nic = NetworkStack.AddressMap[arp_request.TargetIP.Hash];

                        ARPReply_Ethernet reply =
                            new ARPReply_Ethernet(nic.MACAddress, arp_request.TargetIP, arp_request.SenderMAC, arp_request.SenderIP);

                        nic.QueueBytes(reply.RawData);
                    }
                }
            }
            else if (arp_packet.Operation == 0x02)
            {
                if (arp_packet.HardwareType == 1 && arp_packet.ProtocolType == 0x0800)
                {
                    ARPReply_Ethernet arp_reply = new ARPReply_Ethernet(packetData);
                    Global.mDebugger.Send("Received ARP Reply");
                    Global.mDebugger.Send(arp_reply.ToString());
                    Global.mDebugger.Send("ARP Reply Recvd from " + arp_reply.SenderIP.ToString());
                    ARPCache.Update(arp_reply.SenderIP, arp_reply.SenderMAC);

                    OutgoingBuffer.ARPCache_Update(arp_reply);
                }
            }
        }

        /// <summary>
        /// Create new instance of the <see cref="ARPPacket"/> class.
        /// </summary>
        internal ARPPacket()
            : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="ARPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public ARPPacket(byte[] rawData)
            : base(rawData)
        { }

        /// <summary>
        /// Init ARPPacket fields.
        /// </summary>
        protected override void InitFields()
        {
            base.InitFields();
            aHardwareType = (ushort)((RawData[14] << 8) | RawData[15]);
            aProtocolType = (ushort)((RawData[16] << 8) | RawData[17]);
            aHardwareLen = RawData[18];
            aProtocolLen = RawData[19];
            aOperation = (ushort)((RawData[20] << 8) | RawData[21]);
        }

        /// <summary>
        /// Create new instance of the <see cref="ARPPacket"/> class.
        /// </summary>
        /// <param name="dest">Destination MAC address.</param>
        /// <param name="src">Source MAC address.</param>
        /// <param name="hwType">Hardware type.</param>
        /// <param name="protoType">Protocol type.</param>
        /// <param name="hwLen">Hardware address length.</param>
        /// <param name="protoLen">Protocol length.</param>
        /// <param name="operation">Operation.</param>
        /// <param name="packet_size">Packet size.</param>
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

            InitFields();
        }

        /// <summary>
        /// Get operation.
        /// </summary>
        internal ushort Operation => aOperation;
        /// <summary>
        /// Get hardware type.
        /// </summary>
        internal ushort HardwareType => aHardwareType;
        /// <summary>
        /// Get protocol type.
        /// </summary>
        internal ushort ProtocolType => aProtocolType;

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "ARP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", HWType=" + aHardwareType + ", Protocol=" + aProtocolType +
                ", Operation=" + Operation;
        }
    }
}
