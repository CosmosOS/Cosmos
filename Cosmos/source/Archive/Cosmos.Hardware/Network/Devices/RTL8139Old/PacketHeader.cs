using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware2.Network.Devices.RTL8139
{
    /// <summary>
    /// The packethead consists of two bytes (i.e. 16 bits).
    /// A PacketHead contains information about a network Packet, and its transfer.
    /// </summary>
    [Obsolete("Use Ethernet2Frame instead.")]
    public class PacketHeader
    {
        private UInt16 head;

        public PacketHeader(UInt16 data)
        {
            head = data;
        }

        public ushort PacketLength 
        { 
            get {return 2048;} //TODO: Get from packet?
            //private set; 
        }

        public bool IsReceiveOk()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.ROK);
        }

        public bool IsFrameAlignmentError()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.FAE);
        }

        public bool IsCRCError()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.CRC);
        }

        public bool IsLongPacket()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.LONG);
        }

        public bool IsRuntPacket()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.RUNT);
        }

        public bool IsInvalidSymbolError()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.ISE);
        }

        public bool IsBroadcastAddress()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.BAR);
        }

        public bool IsPhysicalAddressMatch()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.PAM);
        }

        public bool IsMulticastAddress()
        {
            return BinaryHelper.CheckBit(head, (ushort)PacketHeadBit.MAR);
        }

        private enum PacketHeadBit : ushort
        {
            ROK = 0x00, //Receive OK
            FAE = 0x01, //Frame Alignment Error
            CRC = 0x02, //CRC Error
            LONG = 0x03,//Long packet - set to 1 when packet over 4k bytes
            RUNT = 0x04,//Runt packet received (smaller than 64 bytes)
            ISE = 0x05, //Invalid Symbol Error (Only 100BASE-TX).
            BAR = 0x0D, //Broadcast Address Received
            PAM = 0x0E, //Physical Address Matched
            MAR = 0x0F  //Multicast Address Received
        }
    }
}