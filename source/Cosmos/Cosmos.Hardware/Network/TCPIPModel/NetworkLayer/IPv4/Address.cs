using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4
{
    public class Address
    {
        private byte[] address = new byte[4];

        public Address(byte aFirst, byte aSecond, byte aThird, byte aFourth)
        {
            address[0] = aFirst;
            address[1] = aSecond;
            address[2] = aThird;
            address[3] = aFourth;
        }

        public bool IsBroadcastAddress()
        {
            //TODO: Check if the address is a BroadcastAddress.

            throw new NotImplementedException();
        }

    }
}
