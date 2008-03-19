using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Network
{
    public class MACAddress : IComparable
    {
        public byte[] bytes= new byte[6];

        public MACAddress(byte[] address)
        {
            if (address == null || address.Length != 6)
                throw new ArgumentException("MACAddress is null or has wrong length", "address");

            bytes[0] = address[0];
            bytes[1] = address[1];
            bytes[2] = address[2];
            bytes[3] = address[3];
            bytes[4] = address[4];
            bytes[5] = address[5];

        }

        public MACAddress(MACAddress m) : this(m.bytes)
        {
        }

        
        public bool IsValid()
        {
            return bytes != null && bytes.Length ==6;
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
                throw new ArgumentException("obj is not a MACAddress", "obj");
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
                throw new ArgumentException("obj is not a MACAddress", "obj");
        }

        public override string ToString()
        {
            string address = string.Empty;

            foreach (byte i in bytes)
                address = address + ToHex(i, 2) + ":";
            
            address = address.TrimEnd(':');

            return address;
        }

        //COPIED FROM Cosmos.Hardware.PC.Bus.PCIBus
        private static string ToHex(UInt32 num, int length)
        {
            char[] ret = new char[length];
            UInt32 cpy = num;

            for (int index = length - 1; index >= 0; index--)
            {
                ret[index] = hex(cpy & 0xf);
                cpy = cpy / 16;
            }

            return new string(ret);
        }

        private static char hex(uint p)
        {
            switch (p)
            {
                case 0:
                    return '0';
                case 1:
                    return '1';
                case 2:
                    return '2';
                case 3:
                    return '3';
                case 4:
                    return '4';
                case 5:
                    return '5';
                case 6:
                    return '6';
                case 7:
                    return '7';
                case 8:
                    return '8';
                case 9:
                    return '9';
                case 10:
                    return 'a';
                case 11:
                    return 'b';
                case 12:
                    return 'c';
                case 13:
                    return 'd';
                case 14:
                    return 'e';
                case 15:
                    return 'f';
            }
            return ' ';

        }
    }
}
