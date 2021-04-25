using Cosmos.System.Network.IPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Network.Config
{
    /// <summary>
    /// Contains DNS configuration
    /// </summary>
    public class DNSConfig
    {
        /// <summary>
        /// DNS Addresses list.
        /// </summary>
        public static List<Address> DNSNameservers = new List<Address>();

        /// <summary>
        /// Add IPv4 configuration.
        /// </summary>
        /// <param name="config"></param>
        public static void Add(Address nameserver)
        {
            for (int i = 0; i < DNSNameservers.Count; i++)
            {
                if (DNSNameservers[i].address.Equals(nameserver))
                {
                    return;
                }
            }
            DNSNameservers.Add(nameserver);
        }

        /// <summary>
        /// Remove IPv4 configuration.
        /// </summary>
        /// <param name="config"></param>
        public static void Remove(Address nameserver)
        {
            for (int i = 0; i < DNSNameservers.Count; i++)
            {
                if (DNSNameservers[i].address.Equals(nameserver))
                {
                    DNSNameservers.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>
        /// Call this to get your adress to request your DNS server
        /// </summary>
        /// <param name="index">Which server you want to get</param>
        /// <returns>DNS Server</returns>
        public static Address Server(int index)
        {
            return DNSNameservers[index];
        }
    }
}
