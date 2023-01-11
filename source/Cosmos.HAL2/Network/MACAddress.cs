using System;
using System.Linq;

namespace Cosmos.HAL.Network
{
    public class MACAddress : IComparable
    {
        public static MACAddress Broadcast;
        public static MACAddress None;

        static MACAddress()
        {
            var xBroadcastArray = new byte[6];
            xBroadcastArray[0] = 0xFF;
            xBroadcastArray[1] = 0xFF;
            xBroadcastArray[2] = 0xFF;
            xBroadcastArray[3] = 0xFF;
            xBroadcastArray[4] = 0xFF;
            xBroadcastArray[5] = 0xFF;
            Broadcast = new MACAddress(xBroadcastArray);
            var xNoneArray = new byte[6];
            xNoneArray[0] = 0x00;
            xNoneArray[1] = 0x00;
            xNoneArray[2] = 0x00;
            xNoneArray[3] = 0x00;
            xNoneArray[4] = 0x00;
            xNoneArray[5] = 0x00;
            None = new MACAddress(xNoneArray);
        }

        public byte[] bytes = new byte[6];

        public MACAddress(byte[] address)
        {
            if (address == null || address.Length != 6)
            {
                throw new ArgumentException("MACAddress is null or has wrong length", "address");
            }

            bytes[0] = address[0];
            bytes[1] = address[1];
            bytes[2] = address[2];
            bytes[3] = address[3];
            bytes[4] = address[4];
            bytes[5] = address[5];

        }

        /// <summary>
        /// Create a MAC address from a byte buffer starting at the specified offset
        /// </summary>
        /// <param name="buffer">byte buffer</param>
        /// <param name="offset">offset in buffer to start from</param>
        public MACAddress(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length < offset + 6)
            {
                throw new ArgumentException("buffer does not contain enough data starting at offset", "buffer");
            }

            bytes[0] = buffer[offset];
            bytes[1] = buffer[offset + 1];
            bytes[2] = buffer[offset + 2];
            bytes[3] = buffer[offset + 3];
            bytes[4] = buffer[offset + 4];
            bytes[5] = buffer[offset + 5];
        }

        public MACAddress(MACAddress m)
            : this(m.bytes)
        {
        }


        public bool IsValid()
        {
            return bytes != null && bytes.Length == 6;
        }

        public int CompareTo(object obj)
        {
            if (obj is MACAddress)
            {
                MACAddress other = (MACAddress)obj;
                int i = 0;
                i = bytes[0].CompareTo(other.bytes[0]);
                if (i != 0) return i;
                i = bytes[1].CompareTo(other.bytes[1]);
                if (i != 0) return i;
                i = bytes[2].CompareTo(other.bytes[2]);
                if (i != 0) return i;
                i = bytes[3].CompareTo(other.bytes[3]);
                if (i != 0) return i;
                i = bytes[4].CompareTo(other.bytes[4]);
                if (i != 0) return i;
                i = bytes[5].CompareTo(other.bytes[5]);
                if (i != 0) return i;

                return 0;
            }
            else
            {
                throw new ArgumentException("obj is not a MACAddress", "obj");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is MACAddress)
            {
                MACAddress other = (MACAddress)obj;

                return bytes[0] == other.bytes[0] &&
                    bytes[1] == other.bytes[1] &&
                    bytes[2] == other.bytes[2] &&
                    bytes[3] == other.bytes[3] &&
                    bytes[4] == other.bytes[4] &&
                    bytes[5] == other.bytes[5];
            }
            else
            {
                throw new ArgumentException("obj is not a MACAddress", "obj");
            }
        }

        public override int GetHashCode()
        {
            return (GetType().AssemblyQualifiedName + "|" + ToString()).GetHashCode();
        }

        public ulong ToNumber()
        {
            return (ulong)((bytes[0] << 40) | (bytes[1] << 32) | (bytes[2] << 24) | (bytes[3] << 16) |
                (bytes[4] << 8) | (bytes[5] << 0));
        }

        private static void PutByte(char[] aChars, int aIndex, byte aByte)
        {
            string xChars = "0123456789ABCDEF";
            aChars[aIndex + 0] = xChars[(aByte >> 4) & 0xF];
            aChars[aIndex + 1] = xChars[aByte & 0xF];
        }

        public uint To32BitNumber()
        {
            return (uint)((bytes[0] << 40) | (bytes[1] << 32) | (bytes[2] << 24) | (bytes[3] << 16) |
                (bytes[4] << 8) | (bytes[5] << 0));
        }

        private uint hash;
        /// <summary>
        /// Hash value for this mac. Used to uniquely identify each mac
        /// </summary>
        public uint Hash
        {
            get
            {
                if (hash == 0)
                {
                    hash = To32BitNumber();
                }

                return hash;
            }
        }

        public override string ToString()
        {
            // mac address consists of 6 2chars pairs, delimited by :
            var xChars = new char[17];
            PutByte(xChars, 0, bytes[0]);
            xChars[2] = ':';
            PutByte(xChars, 3, bytes[1]);
            xChars[5] = ':';
            PutByte(xChars, 6, bytes[2]);
            xChars[8] = ':';
            PutByte(xChars, 9, bytes[3]);
            xChars[11] = ':';
            PutByte(xChars, 12, bytes[4]);
            xChars[14] = ':';
            PutByte(xChars, 15, bytes[5]);
            return new string(xChars);
        }
    }
}
