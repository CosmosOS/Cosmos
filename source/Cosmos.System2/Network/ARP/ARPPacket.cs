using System;
using Cosmos.HAL;
using Cosmos.HAL.Network;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.ARP
{
    /// <summary>
    /// Represents an ARP (Address Resolution Protocol) packet.
    /// </summary>
    /// <remarks>
    /// See also: <seealso cref="EthernetPacket"/>.
    /// </remarks>
    public class ARPPacket : EthernetPacket
    {
        protected ushort hardwareType;
        protected ushort protocolType;
        protected byte hardwareAddrLength;
        protected byte protocolAddrLength;
        protected ushort opCode;

        /// <summary>
        /// Handles ARP packets.
        /// </summary>
        /// <param name="packetData">Packet data.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="global::System.IO.IOException">Thrown on IO error.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="OverflowException">Thrown on fatal error.</exception>
        internal static void ARPHandler(byte[] packetData)
        {
            var arpPacket = new ARPPacket(packetData);
            //Sys.Console.WriteLine("Received ARP Packet");
            //Sys.Console.WriteLine(arp_packet.ToString());
            if (arpPacket.Operation == 0x01)
            {
                if (arpPacket.HardwareType == 1 && arpPacket.ProtocolType == 0x0800)
                {
                    var arpRequest = new ARPRequestEthernet(packetData);
                    if (arpRequest.SenderIP == null)
                    {
                        NetworkStack.Debugger.Send("SenderIP null in ARPHandler!");
                    }

                    arpRequest = new ARPRequestEthernet(packetData);

                    ARPCache.Update(arpRequest.SenderIP, arpRequest.SenderMAC);

                    if (NetworkStack.AddressMap.ContainsKey(arpRequest.TargetIP.Hash) == true)
                    {
                        NetworkStack.Debugger.Send("ARP request received from " + arpRequest.SenderIP.ToString());
                        NetworkDevice nic = NetworkStack.AddressMap[arpRequest.TargetIP.Hash];

                        var reply = new ARPReplyEthernet(
                            nic.MACAddress,
                            arpRequest.TargetIP,
                            arpRequest.SenderMAC,
                            arpRequest.SenderIP
                        );

                        nic.QueueBytes(reply.RawData);
                    }
                }
            }
            else if (arpPacket.Operation == 0x02)
            {
                if (arpPacket.HardwareType == 1 && arpPacket.ProtocolType == 0x0800)
                {
                    var arpReply = new ARPReplyEthernet(packetData);
                    NetworkStack.Debugger.Send("Received ARP reply:");
                    NetworkStack.Debugger.Send(arpReply.ToString());
                    NetworkStack.Debugger.Send("ARP reply received from " + arpReply.SenderIP.ToString());
                    ARPCache.Update(arpReply.SenderIP, arpReply.SenderMAC);

                    OutgoingBuffer.UpdateARPCache(arpReply);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ARPPacket"/> class.
        /// </summary>
        internal ARPPacket()
            : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ARPPacket"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public ARPPacket(byte[] rawData)
            : base(rawData)
        { }

        protected override void InitializeFields()
        {
            base.InitializeFields();
            hardwareType = (ushort)((RawData[14] << 8) | RawData[15]);
            protocolType = (ushort)((RawData[16] << 8) | RawData[17]);
            hardwareAddrLength = RawData[18];
            protocolAddrLength = RawData[19];
            opCode = (ushort)((RawData[20] << 8) | RawData[21]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ARPPacket"/> class.
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

            InitializeFields();
        }

        /// <summary>
        /// Gets the operation code.
        /// </summary>
        internal ushort Operation => opCode;

        /// <summary>
        /// Get the hardware type.
        /// </summary>
        internal ushort HardwareType => hardwareType;

        /// <summary>
        /// Gets the protocol type.
        /// </summary>
        internal ushort ProtocolType => protocolType;

        public override string ToString()
        {
            return "ARP Packet Src=" + srcMAC + ", Dest=" + destMAC + ", HWType=" + hardwareType + ", Protocol=" + protocolType +
                ", Operation=" + Operation;
        }
    }
}
