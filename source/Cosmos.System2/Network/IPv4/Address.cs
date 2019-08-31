using System;

namespace Cosmos.System.Network.IPv4
{
    /// <summary>
    /// Defines a IPv4 Address
    /// <remarks>Should actually be using System.Net.IPAddress, but gives problems</remarks>
    /// </summary>
    public class Address : IComparable
    {
        /// <summary>
        /// Predefined 0.0.0.0 address
        /// </summary>
        public static Address Zero = new Address(0, 0, 0, 0);
        public static Address Broadcast = new Address(255, 255, 255, 255);

        internal byte[] address = new byte[4];

        public Address(byte aFirst, byte aSecond, byte aThird, byte aFourth)
        {
            address[0] = aFirst;
            address[1] = aSecond;
            address[2] = aThird;
            address[3] = aFourth;
        }

        public Address(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length < (offset + 4))
                throw new ArgumentException("buffer does not contain enough data starting at offset", "buffer");

            address[0] = buffer[offset];
            address[1] = buffer[offset + 1];
            address[2] = buffer[offset + 2];
            address[3] = buffer[offset + 3];
        }

        /// <summary>
        /// Parse a IP Address in string representation
        /// </summary>
        /// <param name="adr">IP Address as string</param>
        /// <returns></returns>
        public static Address Parse(string adr)
        {
            string[] fragments = adr.Split('.');
            if (fragments.Length == 4)
            {
                byte first = byte.Parse(fragments[0]);
                byte second = byte.Parse(fragments[1]);
                byte third = byte.Parse(fragments[2]);
                byte fourth = byte.Parse(fragments[3]);

                return new Address(first, second, third, fourth);
            }
            else
            {
                return null;
            }
        }

        public bool IsLoopbackAddress()
        {
            if (address[0] == 127)
                return true;
            else
                return false;
        }

        public bool IsBroadcastAddress() =>
            address[0] == 0xFF
            && address[1] == 0xFF
            && address[2] == 0xFF
            && address[3] == 0xFF;

        public bool IsAPIPA()
        {
            if ((address[0] == 169) && (address[1] == 254))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts IP Address to String
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

        public byte[] ToByteArray()
        {
            return address;
        }

        private UInt32 to32BitNumber()
        {
            return (UInt32)((address[0] << 24) | (address[1] << 16) | (address[2] << 8) | (address[3] << 0));
        }

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
        /// <param name="obj"></param>
        /// <returns>0 if equal, or non-zero otherwise</returns>
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
