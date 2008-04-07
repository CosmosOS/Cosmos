using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware.Network.TCPIPModel.NetworkLayer.IPv4
{
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

    }
}
