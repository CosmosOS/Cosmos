namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// Represents a DHCP acknowledge (ACK) packet.
    /// </summary>
    internal class DHCPAck : DHCPPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPAck"/> class.
        /// </summary>
        internal DHCPAck() : base()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DHCPAck"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPAck(byte[] rawData) : base(rawData)
        { }

        protected override void InitializeFields()
        {
            base.InitializeFields();

            foreach (var option in Options)
            {
                if (option.Type == 1) // Mask
                {
                    Subnet = new Address(option.Data, 0);
                }
                else if (option.Type == 3) // Router
                {
                    Server = new Address(option.Data, 0);
                }
                else if (option.Type == 6) // DNS
                {
                    DNS = new Address(option.Data, 0);
                }
            }
        }

        /// <summary>
        /// Gets the subnet IPv4 address.
        /// </summary>
        internal Address Subnet { get; private set; }

        /// <summary>
        /// Gets the DNS IPv4 address.
        /// </summary>
        internal Address DNS { get; private set; }

        /// <summary>
        /// Gets the DHCP server IPv4 address.
        /// </summary>
        internal Address Server { get; private set; }
    }
}
