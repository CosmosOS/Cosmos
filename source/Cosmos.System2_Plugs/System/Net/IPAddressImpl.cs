using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Cosmos.System.Network.IPv4.TCP;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net
{
    [Plug(Target = typeof(IPAddress))]
    public static class IPAddressImpl
    {
        private static uint _addressOrScopeId;
        private const int IPv4AddressBytes = 4;
        private const int IPv6AddressBytes = 16;

        public static uint get_PrivateAddress(IPAddress aThis)
        {
            return _addressOrScopeId;
        }

        public static void set_PrivateAddress(IPAddress aThis, uint address)
        {
            _addressOrScopeId = address;
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
            _addressOrScopeId = (uint)address;
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
                _addressOrScopeId = (uint)((address[0] << 0) | (address[1] << 8) | (address[2] << 16) | (address[3] << 24));
            }
            else if (address.Length == IPv6AddressBytes)
            {
                //do nothing
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
