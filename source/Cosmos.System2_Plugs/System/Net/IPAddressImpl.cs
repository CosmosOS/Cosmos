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

        public static uint get_PrivateAddress(IPAddress aThis)
        {
            return PrivateAddress;
        }

        public static void set_PrivateAddress(IPAddress aThis, uint address)
        {
            PrivateAddress = address;
        }

        public static AddressFamily get_AddressFamily(IPAddress aThis)
        {
            return AddressFamily.InterNetwork;
        }

        public static void CCtor(IPAddress aThis)
        {
        }

        public static void Ctor(IPAddress aThis, long address)
        {
            PrivateAddress = (uint)address;
        }

        public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address)
        {
            Ctor(aThis, address.ToArray());
        }

        public static void Ctor(IPAddress aThis, ReadOnlySpan<byte> address, long scopeId)
        {
            Ctor(aThis, address.ToArray());
        }

        public static void Ctor(IPAddress aThis, byte[] address)
        {
            if (address.Length == IPv4AddressBytes)
            {
                PrivateAddress = (uint)((address[0] << 0) | (address[1] << 8) | (address[2] << 16) | (address[3] << 24));

                Cosmos.HAL.Global.debugger.Send("Ctor address=" + aThis.ToString());
            }
            else if (address.Length == IPv6AddressBytes)
            {
                Cosmos.HAL.Global.debugger.Send("IPv6 not supported yet!");
            }
            else
            {
                Cosmos.HAL.Global.debugger.Send("Bad IP address format");
                throw new ArgumentException("Bad IP address format");
            }
        }

        public static IPAddress Parse(string address)
        {
            string[] fragments = address.Split('.');
            if (fragments.Length == 4)
            {
                try
                {
                    var addressArray = new byte[4];
                    addressArray[0] = byte.Parse(fragments[0]);
                    addressArray[1] = byte.Parse(fragments[1]);
                    addressArray[2] = byte.Parse(fragments[2]);
                    addressArray[3] = byte.Parse(fragments[3]);

                    return new IPAddress(addressArray);
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
    }
}
