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
    [Plug("System.Net.IPAddressParser, System.Net.Primitives")]
    public static class IPAddressParserImpl
    {
        public static IPAddress Parse(ReadOnlySpan<char> ipSpan, bool tryParse)
        {
            if (ipSpan.Contains(':'))
            {
                throw new NotImplementedException("IPv6 not implemented yet!");
            }
            else
            {
                if (TryParseIpv4(ipSpan, out var address))
                {
                    return new IPAddress(address);
                }
            }

            if (tryParse)
            {
                return null;
            }

            throw new FormatException("Adresse IP non valide");
        }

        private static bool TryParseIpv4(ReadOnlySpan<char> ipSpan, out long address)
        {
            var ipv4String = ipSpan.ToString();
            if (IPAddress.TryParse(ipv4String, out var ipAddress) && ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                address = BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0);
                return true;
            }
            else
            {
                address = 0;
                return false;
            }
        }
    }
}
