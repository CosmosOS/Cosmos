using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.IPv4.UDP.DHCP
{
    /// <summary>
    /// DHCPAck class.
    /// </summary>
    internal class DHCPAck : DHCPPacket
    {
        /// <summary>
        /// Create new instance of the <see cref="DHCPAck"/> class.
        /// </summary>
        internal DHCPAck() : base()
        { }

        /// <summary>
        /// Create new instance of the <see cref="DHCPAck"/> class.
        /// </summary>
        /// <param name="rawData">Raw data.</param>
        internal DHCPAck(byte[] rawData) : base(rawData)
        { }

        /// <summary>
        /// Init DHCPAck fields.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if RawData is invalid or null.</exception>
        protected override void InitFields()
        {
            base.InitFields();

            foreach (var option in Options)
            {
                if (option.Type == 1) //Mask
                {
                    Subnet = new Address(option.Data, 0);
                }
                else if (option.Type == 6) //DNS
                {
                    DNS = new Address(option.Data, 0);
                }
            }
        }

        /// <summary>
        /// Get Subnet IPv4 Address
        /// </summary>
        internal Address Subnet { get; private set; }

        /// <summary>
        /// Get DNS IPv4 Address
        /// </summary>
        internal Address DNS { get; private set; }

    }
}
