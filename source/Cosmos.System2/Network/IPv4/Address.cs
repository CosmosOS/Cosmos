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
    /// Represents a IPv4 address.
    /// </summary>
    // Should actually be using System.Net.IPAddress, but gives problems.
    public class Address : IComparable
    {
        private uint hash;

        /// <summary>
        /// The parts of the address.
        /// </summary>
        internal byte[] Parts = new byte[4];

        public bool IsIpv4 => Parts.Length == 4;
        public bool IsIpv6 => !IsIpv4;

        /// <summary>
        /// The <c>0.0.0.0</c> IP address.
        /// </summary>
        public static readonly Address Zero = new(0, 0, 0, 0);

        /// <summary>
        /// The broadcast address <c>(255.255.255.255)</c>.
        /// </summary>
        public static readonly Address Broadcast = new(255, 255, 255, 255);

        /// <summary>
        /// Create new instance of the <see cref="Address"/> class, with specified IP address.
        /// </summary>
        /// <param name="aFirst">First block of the address.</param>
        /// <param name="aSecond">Second block of the address.</param>
        /// <param name="aThird">Third block of the address.</param>
        /// <param name="aFourth">Fourth block of the address.</param>
        public Address(byte aFirst, byte aSecond, byte aThird, byte aFourth)
        {
            Parts[0] = aFirst;
            Parts[1] = aSecond;
            Parts[2] = aThird;
            Parts[3] = aFourth;
        }

        /// <summary>
        /// Create new instance of the <see cref="Address"/> class, with specified buffer and offset.
        /// </summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="offset">Offset.</param>
        /// <exception cref="ArgumentException">Thrown if buffer is invalid or null.</exception>
        public Address(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length < offset + 4) {
                throw new ArgumentException("The buffer does not contain enough data starting at 'offset'.", nameof(buffer));
            }

            Parts[0] = buffer[offset];
            Parts[1] = buffer[offset + 1];
            Parts[2] = buffer[offset + 2];
            Parts[3] = buffer[offset + 3];
        }

        /// <summary>
        /// Parses a IP address in its string representation.
        /// </summary>
        /// <param name="addr">The IP address as string.</param>
        /// <returns>The parsed address value.</returns>
        /// <exception cref="OverflowException">Thrown if addr is longer than <see cref="Int32.MaxValue"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if addr is null.</exception>
        /// <exception cref="FormatException">Thrown if addr is not in the right format.</exception>
        /// <exception cref="OverflowException">Thrown if addr represents a number less than Byte.MinValue or greater than <see cref="Byte.MaxValue"/>.</exception>
        public static Address Parse(string addr)
        {
            string[] fragments = addr.Split('.');
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
        /// Convert a CIDR number to an IPv4 address.
        /// </summary>
        /// <param name="cidr">The CIDR number.</param>
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
        /// Check if this address is a loopback address.
        /// </summary>
        public bool IsLoopbackAddress() => Parts[0] == 127;

        /// <summary>
        /// Check if this address is a broadcast address.
        /// </summary>
        public bool IsBroadcastAddress() =>
            Parts[0] == 0xFF
            && Parts[1] == 0xFF
            && Parts[2] == 0xFF
            && Parts[3] == 0xFF;

        /// <summary>
        /// Check if this address is an APIPA address.
        /// </summary>
        public bool IsAPIPA() => Parts[0] == 169 && Parts[1] == 254;

        public override string ToString()
        {
            return
                Parts[0] +
                "." +
                Parts[1] +
                "." +
                Parts[2] +
                "." +
                Parts[3];
        }

        /// <summary>
        /// Returns the underlying parts array. Modifying the returned
        /// array will also modify the address.
        /// </summary>
        public byte[] ToByteArray()
        {
            return Parts;
        }

        /// <summary>
        /// Convert this address to a 32-bit number.
        /// </summary>
        private uint ToUInt32()
        {
            return (uint)((Parts[0] << 24) | (Parts[1] << 16) | (Parts[2] << 8) | (Parts[3] << 0));
        }

        /// <summary>
        /// The hash value for this IP. Used to uniquely identify each IP.
        /// </summary>
        public uint Hash
        {
            get
            {
                if (hash == 0)
                {
                    hash = ToUInt32();
                }

                return hash;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj is Address other)
            {
                if (other.hash != hash)
                {
                    return -1;
                }

                return 0;
            }
            else
            {
                throw new ArgumentException("obj is not a IPv4Address", nameof(obj));
            }
        }

        public override bool Equals(object obj)
        {

            if (obj == null && this == null)
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            if (this == null)
            {
                return false;
            }

            if (obj is Address address)
            {
                if (IsIpv4 != address.IsIpv4) // not same ip type so is false
                {
                    return false;
                }

                for (int i = 0; i < Parts.Length; i++)
                {
                    if (Parts[i] != address.Parts[i])
                    {
                        return false; // ips dont match
                    }
                }

                return true; // ip type and value match

            }

            return false; // obj is not an Address

        }

    }
}
