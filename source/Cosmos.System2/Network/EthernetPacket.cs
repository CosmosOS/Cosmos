using System;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network
{
    // for more info, http://standards.ieee.org/about/get/802/802.3.html
    public class EthernetPacket
    {
        protected MACAddress srcMAC;
        protected MACAddress destMAC;

        protected EthernetPacket()
        {
        }

        protected EthernetPacket(byte[] rawData)
        {
            RawData = rawData;
            initFields();
        }

        protected virtual void initFields()
        {
            destMAC = new MACAddress(RawData, 0);
            srcMAC = new MACAddress(RawData, 6);
            EthernetType = (ushort)((RawData[12] << 8) | RawData[13]);
        }

        protected EthernetPacket(ushort type, int packet_size)
            : this(MACAddress.None, MACAddress.None, type, packet_size)
        {
        }

        protected EthernetPacket(MACAddress dest, MACAddress src, ushort type, int packet_size)
        {
            RawData = new byte[packet_size];
            for (int i = 0; i < 6; i++)
            {
                RawData[i] = dest.bytes[i];
                RawData[6 + i] = src.bytes[i];
            }

            RawData[12] = (byte)(type >> 8);
            RawData[13] = (byte)(type >> 0);
            initFields();
        }

        internal byte[] RawData { get; }

        internal MACAddress SourceMAC
        {
            get => srcMAC;
            set
            {
                for (int i = 0; i < 6; i++)
                {
                    RawData[6 + i] = value.bytes[i];
                }
                initFields();
            }
        }
        internal MACAddress DestinationMAC
        {
            get => destMAC;
            set
            {
                for (int i = 0; i < 6; i++)
                {
                    RawData[i] = value.bytes[i];
                }
                initFields();
            }
        }

        internal ushort EthernetType { get; private set; }

        /// <summary>
        /// Calculate any checksums
        /// </summary>
        public virtual void PrepareForSending()
        {

            
        }

        public override string ToString()
        {
            return "Ethernet Packet : Src=" + srcMAC + ", Dest=" + destMAC + ", Type=" + EthernetType;
        }
    }
}
