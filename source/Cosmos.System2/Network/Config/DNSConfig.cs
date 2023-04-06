using Cosmos.System.Network.IPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.Config
{
    /// <summary>
    /// Represents DNS configuration.
    /// </summary>
    public class DNSConfig
    {
        /// <summary>
        /// The list of known DNS nameserver addresses.
        /// </summary>
        public readonly static List<Address> DNSNameservers = new();

        /// <summary>
        /// Registers a given DNS server.
        /// </summary>
        /// <param name="nameserver">The IP address of the target DNS server.</param>
        public static void Add(Address nameserver)
        {
            for (int i = 0; i < DNSNameservers.Count; i++)
            {
                if (DNSNameservers[i].Parts.Equals(nameserver))
                {
                    return;
                }
            }
            DNSNameservers.Add(nameserver);
        }

        /// <summary>
        /// Removes the given DNS server from the list of registered nameservers.
        /// </summary>
        /// <param name="nameserver">The IP address of the target DNS server.</param>
        public static void Remove(Address nameserver)
        {
            for (int i = 0; i < DNSNameservers.Count; i++)
            {
                if (DNSNameservers[i].Parts.Equals(nameserver))
                {
                    DNSNameservers.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the address of a known DNS server under the given
        /// registered list index.
        /// </summary>
        /// <param name="index">The index of the server to fetch.</param>
        [Obsolete("Directly index the 'DNSNameservers' dictionary instead.")]
        public static Address Server(int index)
        {
            return DNSNameservers[index];
        }
    }
}
