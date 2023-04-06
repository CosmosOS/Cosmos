using Cosmos.HAL.Network;

namespace Cosmos.System.Network
{
    /// <summary>
    /// Represents an Ethernet packet.
    /// </summary>
    // For more info, refer to http://standards.ieee.org/about/get/802/802.3.html
    public class EthernetPacket
    {
        protected MACAddress srcMAC;
        protected MACAddress destMAC;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthernetPacket"/> class.
        /// </summary>
        protected EthernetPacket()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EthernetPacket"/> class, with specified raw data.
        /// </summary>
        /// <param name="rawData">The raw data of the packet.</param>
        public EthernetPacket(byte[] rawData)
        {
            RawData = rawData;
            InitializeFields();
        }

        /// <summary>
        /// Initializes all internal fields.
        /// </summary>
        protected virtual void InitializeFields()
        {
            destMAC = new MACAddress(RawData, 0);
            srcMAC = new MACAddress(RawData, 6);
            EthernetType = (ushort)((RawData[12] << 8) | RawData[13]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EthernetPacket"/> class, with specified type and size.
        /// </summary>
        protected EthernetPacket(ushort type, int packetSize)
            : this(MACAddress.None, MACAddress.None, type, packetSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EthernetPacket"/> class, with specified dsetination, source, type and size.
        /// </summary>
        protected EthernetPacket(MACAddress dest, MACAddress src, ushort type, int packetSize)
        {
            RawData = new byte[packetSize];
            for (int i = 0; i < 6; i++)
            {
                RawData[i] = dest.bytes[i];
                RawData[6 + i] = src.bytes[i];
            }

            RawData[12] = (byte)(type >> 8);
            RawData[13] = (byte)(type >> 0);
            InitializeFields();
        }

        /// <summary>
        /// Gets the raw data of the packet in the form of a byte array.
        /// </summary>
        public byte[] RawData { get; }

        /// <summary>
        /// The source MAC address.
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
                InitializeFields();
            }
        }

        /// <summary>
        /// The destination MAC address.
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

                InitializeFields();
            }
        }

        /// <summary>
        /// The type of the packet.
        /// </summary>
        internal ushort EthernetType { get; private set; }

        /// <summary>
        /// Prepares the packet for sending. Not implemented.
        /// </summary>
        internal virtual void PrepareForSending() { }

        public override string ToString()
        {
            return "Ethernet Packet : Src=" + srcMAC + ", Dest=" + destMAC + ", Type=" + EthernetType;
        }
    }
}
