using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Driver.RTL8139
{
    /// <summary>
    /// The packethead consists of two bytes (i.e. 16 bits).
    /// A PacketHead contains information about a network Packet, and its transfer.
    /// </summary>
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
            return CheckBit(PacketHeadBit.ROK);
        }

        public bool IsFrameAlignmentError()
        {
            return CheckBit(PacketHeadBit.FAE);
        }

        public bool IsCRCError()
        {
            return CheckBit(PacketHeadBit.CRC);
        }

        public bool IsLongPacket()
        {
            return CheckBit(PacketHeadBit.LONG);
        }

        public bool IsRuntPacket()
        {
            return CheckBit(PacketHeadBit.RUNT);
        }

        public bool IsInvalidSymbolError()
        {
            return CheckBit(PacketHeadBit.ISE);
        }

        public bool IsBroadcastAddress()
        {
            return CheckBit(PacketHeadBit.BAR);
        }

        public bool IsPhysicalAddressMatch()
        {
            return CheckBit(PacketHeadBit.PAM);
        }

        public bool IsMulticastAddress()
        {
            return CheckBit(PacketHeadBit.MAR);
        }

        /// <summary>
        /// Bitwise checks it the given bit is set in the PacketHeader.
        /// </summary>
        /// <param name="bit"></param>
        /// <returns>Returns TRUE if bit is set</returns>
        private bool CheckBit(PacketHeadBit bit)
        {
            return ((head & (byte)bit) == (byte)bit);
        }

        private enum PacketHeadBit : byte
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