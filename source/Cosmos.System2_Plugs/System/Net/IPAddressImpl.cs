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

        public static void CCtor(IPAddress aThis)
        {
            Cosmos.HAL.Global.debugger.Send("IPAddress - cctor.");
        }

        public static void Ctor(IPAddress aThis, long address)
        {
            Cosmos.HAL.Global.debugger.Send("IPAddress - ctor long.");

            PrivateAddress = (uint)address;
        }

        public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address)
        {
            Cosmos.HAL.Global.debugger.Send("IPAddress - ctor.");
            
            if (address.Length == IPv4AddressBytes)
            {
                PrivateAddress = (uint)(address[0] << 24 | address[1] << 16 | address[2] << 8 | address[3]);
            }
            else if (address.Length == IPv6AddressBytes)
            {
                throw new NotImplementedException("IPv6 not supported yet!");
            }
            else
            {
                throw new ArgumentException("Bad IP address format", nameof(address));
            }

            Cosmos.HAL.Global.debugger.Send("IPAddress - " + address[0] + "." + address[1] + "." + address[2] + "." + address[3] + " set.");
        }

        public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address, long scopeId)
        {
        }
    }
}
