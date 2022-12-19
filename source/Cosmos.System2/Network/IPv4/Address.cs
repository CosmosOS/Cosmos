/*
* PROJECT:          Aura Operating System Development
* CONTENT:          IP Address
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using System;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Address class, used to define a IPv4 address.
    /// <remarks>Should actually be using System.Net.IPAddress, but gives problems.</remarks>
    /// </summary>
    public class Address : IComparable
    {
        /// <summary>
        /// Predefined 0.0.0.0 address.
        /// </summary>
        public static Address Zero = new Address(0, 0, 0, 0);

        /// <summary>
        /// Broadcast address (255.255.255.255).
        /// </summary>
        public static Address Broadcast = new Address(255, 255, 255, 255);

        /// <summary>
        /// address as byte array.
        /// </summary>
        internal byte[] address = new byte[4];

        /// <summary>
        /// Create new instance of the <see cref="Address"/> class, with specified IP address.
        /// </summary>
        /// <param name="aFirst">First block of the address.</param>
        /// <param name="aSecond">Second block of the address.</param>
        /// <param name="aThird">Third block of the address.</param>
        /// <param name="aFourth">Fourth block of the address.</param>
        public Address(byte aFirst, byte aSecond, byte aThird, byte aFourth)
        {
            address[0] = aFirst;
            address[1] = aSecond;
            address[2] = aThird;
            address[3] = aFourth;
        }

        /// <summary>
        /// Create new instance of the <see cref="Address"/> class, with specified buffer and offset.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <exception cref="ArgumentException">Thrown if buffer is invalid or null.</exception>
        public Address(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length < offset + 4)
                throw new ArgumentException("buffer does not contain enough data starting at offset", "buffer");

            address[0] = buffer[offset];
            address[1] = buffer[offset + 1];
            address[2] = buffer[offset + 2];
            address[3] = buffer[offset + 3];
        }

        /// <summary>
        /// Parse a IP address in string representation.
        /// </summary>
        /// <param name="adr">IP address as string.</param>
        /// <returns>Address value.</returns>
        /// <exception cref="OverflowException">Thrown if adr is longer than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentNullException">Thrown if adr is null.</exception>
        /// <exception cref="FormatException">Thrown if adr is not in the right format.</exception>
        /// <exception cref="OverflowException">Thrown if adr represents a number less than Byte.MinValue or greater than Byte.MaxValue.</exception>
        public static Address Parse(string adr)
        {
            string[] fragments = adr.Split('.');
            if (fragments.Length == 4)
            {
                try
                {
                    byte first = byte.Parse(fragments[0]);
                    byte second = byte.Parse(fragments[1]);
                    byte third = byte.Parse(fragments[2]);
                    byte fourth = byte.Parse(fragments[3]);
                    return new Address(first, second, third, fourth);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Convert CIDR number to IPv4 Address
        /// </summary>
        /// <param name="cidr">CIDR number.</param>
        /// <returns></returns>
        public static Address CIDRToAddress(int cidr)
        {
            try
            {
                uint mask = 0xffffffff << (32 - cidr);
                return new Address((byte)(mask >> 24), (byte)(mask >> 16 & 0xff), (byte)(mask >> 8 & 0xff), (byte)(mask & 0xff));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Check if address is a loopback address.
        /// </summary>
        /// <returns></returns>
        public bool IsLoopbackAddress()
        {
            if (address[0] == 127)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check if address is a broadcast address.
        /// </summary>
        /// <returns></returns>
        public bool IsBroadcastAddress() =>
            address[0] == 0xFF
            && address[1] == 0xFF
            && address[2] == 0xFF
            && address[3] == 0xFF;

        /// <summary>
        /// Check if address is a APIPA address.
        /// </summary>
        /// <returns></returns>
        public bool IsAPIPA()
        {
            if (address[0] == 169 && address[1] == 254)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts IP Address to String.
        /// </summary>
        /// <returns>String with IP Address in dotted notation</returns>
        public override string ToString()
        {
            return
                address[0] +
                "." +
                address[1] +
                "." +
                address[2] +
                "." +
                address[3];
        }

        /// <summary>
        /// Convert address to byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            return address;
        }

        /// <summary>
        /// Convert address to 32 bit number.
        /// </summary>
        /// <returns>UInt32 value.</returns>
        private UInt32 to32BitNumber()
        {
            return (UInt32)((address[0] << 24) | (address[1] << 16) | (address[2] << 8) | (address[3] << 0));
        }

        /// <summary>
        /// Hashed value for the IP.
        /// </summary>
        private UInt32 hash;

        /// <summary>
        /// Hash value for this IP. Used to uniquely identify each IP
        /// </summary>
        public UInt32 Hash
        {
            get
            {
                if (hash == 0)
                {
                    hash = to32BitNumber();
                }

                return hash;
            }
        }

        #region IComparable Members

        /// <summary>
        /// Compare 2 IP Address objects for equality
        /// </summary>
        /// <param name="obj">Other IP to compare with.</param>
        /// <returns>0 if equal, or non-zero otherwise</returns>
        /// <exception cref="ArgumentException">Thrown if obj is not a IPv4Address.</exception>
        public int CompareTo(object obj)
        {
            if (obj is Address)
            {
                Address other = (Address)obj;
                if (other.hash != this.hash)
                {
                    return -1;
                }

                return 0;
            }
            else
                throw new ArgumentException("obj is not a IPv4Address", "obj");
        }

        #endregion
    }
}
