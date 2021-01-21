/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Ethernet frame
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;
using Cosmos.HAL.Network;

namespace Cosmos.System.Network
{
    // for more info, http://standards.ieee.org/about/get/802/802.3.html
    /// <summary>
    /// EthernetPacket class.
    /// </summary>
    public class EthernetPacket
    {
        /// <summary>
        /// Source MAC address.
        /// </summary>
        protected MACAddress srcMAC;
        /// <summary>
        /// Destination MAC address.
        /// </summary>
        protected MACAddress destMAC;

        /// <summary>
        /// Create new instance of the <see cref="EthernetPacket"/> class.
        /// </summary>
        protected EthernetPacket()
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="EthernetPacket"/> class, with specified raw data.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        public EthernetPacket(byte[] rawData)
        {
            RawData = rawData;
            InitFields();
        }

        /// <summary>
        /// Init EthernetPacket fields.
        /// </summary>
        protected virtual void InitFields()
        {
            destMAC = new MACAddress(RawData, 0);
            srcMAC = new MACAddress(RawData, 6);
            EthernetType = (ushort)((RawData[12] << 8) | RawData[13]);
        }

        /// <summary>
        /// Create new instance of the <see cref="EthernetPacket"/> class, with specified type and size.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="packet_size">Size.</param>
        protected EthernetPacket(ushort type, int packet_size)
            : this(MACAddress.None, MACAddress.None, type, packet_size)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="EthernetPacket"/> class, with specified dsetination, source, type and size.
        /// </summary>
        /// <param name="dest">Destination.</param>
        /// <param name="src">Source.</param>
        /// <param name="type">Type.</param>
        /// <param name="packet_size">Size.</param>
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
            InitFields();
        }

        /// <summary>
        /// Get raw data byte array.
        /// </summary>
        public byte[] RawData { get; }

        /// <summary>
        /// Get and set source MAC address.
        /// </summary>
        internal MACAddress SourceMAC
        {
            get => srcMAC;
            set
            {
                for (int i = 0; i < 6; i++)
                {
                    RawData[6 + i] = value.bytes[i];
                }
                InitFields();
            }
        }

        /// <summary>
        /// Get and set destination MAC address.
        /// </summary>
        internal MACAddress DestinationMAC
        {
            get => destMAC;
            set
            {
                for (int i = 0; i < 6; i++)
                {
                    RawData[i] = value.bytes[i];
                }
                InitFields();
            }
        }

        /// <summary>
        /// Get packet type.
        /// </summary>
        internal ushort EthernetType { get; private set; }

        /// <summary>
        /// Prepare packet for sending.
        /// Not implemented.
        /// </summary>
        public virtual void PrepareForSending()
        {


        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return "Ethernet Packet : Src=" + srcMAC + ", Dest=" + destMAC + ", Type=" + EthernetType;
        }
    }
}
