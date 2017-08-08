using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace Cosmos.Hardware2.Network.TCPIPModel.NetworkLayer.IPv4
{
    //Kudzu - Frode I planned to use the existing stuff in System.Net for parsing URLS, but 
    // the problem is it crashes right now.
    [Obsolete("Soon to be replaced by System.Net.IPAddress")]
    public class IPv4Address
    {
        private byte[] address = new byte[4];

        public IPv4Address(byte aFirst, byte aSecond, byte aThird, byte aFourth)
        {
            address[0] = aFirst;
            address[1] = aSecond;
            address[2] = aThird;
            address[3] = aFourth;
        }

        //TODO: Uncomment - doesn't work in Cosmos yet. FL, 08.apr'08.
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
            //TODO: Check if the address is a BroadcastAddress.

            throw new NotImplementedException();
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
            return (UInt32)((address[0] << 0) | (address[1] << 4) | (address[2] << 8) | (address[3] << 16));
        }

    }
}
