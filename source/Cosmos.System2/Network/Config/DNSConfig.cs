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
    }
}
