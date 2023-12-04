using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net
{
    [Plug(Target = typeof(IPAddress))]
    public static class IPAddressImpl
    {
        private const int IPv4AddressBytes = 4;
        private const int IPv6AddressBytes = 16;
        private static uint PrivateAddress;

        public static uint get_PrivateAddress()
        {
            return PrivateAddress;
        }

        public static void set_PrivateAddress(uint address)
        {
            PrivateAddress = address;
        }

        public static void Ctor(IPAddress aThis, long address)
        {
            PrivateAddress = (uint)address;
        }

        public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address)
        {
            if (address.Length == IPv4AddressBytes)
            {
                PrivateAddress = (uint)(address[0] << 24 | address[1] << 16 | address[2] << 8 | address[3]);
            }
            else if (address.Length == IPv6AddressBytes)
            {
                Cosmos.HAL.Global.debugger.Send("IPv6 not supported yet!");
                throw new NotImplementedException("IPv6 not supported yet!");
            }
            else
            {
                Cosmos.HAL.Global.debugger.Send("Bad IP address format");
                throw new ArgumentException("Bad IP address format");
            }
        }

        public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address, long scopeId)
        {
        }
    }
}
