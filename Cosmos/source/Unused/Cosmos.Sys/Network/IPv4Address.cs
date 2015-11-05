using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Sys.Network
{
    public class IPv4Address : IComparable
    {
        public static IPv4Address Zero = new IPv4Address(0, 0, 0, 0);

        public byte[] address = new byte[4];

        public IPv4Address(byte aFirst, byte aSecond, byte aThird, byte aFourth)
        {
            address[0] = aFirst;
            address[1] = aSecond;
            address[2] = aThird;
            address[3] = aFourth;
        }

        public IPv4Address(byte[] buffer, int offset)
        {
            if (buffer == null || buffer.Length < (offset + 4))
                throw new ArgumentException("buffer does not contain enough data starting at offset", "buffer");

            address[0] = buffer[offset];
            address[1] = buffer[offset + 1];
            address[2] = buffer[offset + 2];
            address[3] = buffer[offset + 3];
        }

        public static IPv4Address Parse(string adr)
        {
            string[] fragments = adr.Split('.');
            if (fragments.Length == 4)
            {
                byte first = byte.Parse(fragments[0]);
                byte second = byte.Parse(fragments[1]);
                byte third = byte.Parse(fragments[2]);
                byte fourth = byte.Parse(fragments[3]);

                return new IPv4Address(first, second, third, fourth);
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

        public bool IsBroadcastAddress()
        {
            if (address[0] == 255)
            {
                return true;
            }

            return false;
        }

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

        public UInt32 To32BitNumber()
        {
            return (UInt32)((address[0] << 24) | (address[1] << 16) | (address[2] << 8) | (address[3] << 0));
        }


        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is IPv4Address)
            {
                IPv4Address other = (IPv4Address)obj;
                int i = 0;
                i = address[0].CompareTo(other.address[0]);
                if (i != 0) return i;
                i = address[1].CompareTo(other.address[1]);
                if (i != 0) return i;
                i = address[2].CompareTo(other.address[2]);
                if (i != 0) return i;
                i = address[3].CompareTo(other.address[3]);
                if (i != 0) return i;

                return 0;
            }
            else
                throw new ArgumentException("obj is not a IPv4Address", "obj");
        }

        #endregion
    }
}
