using System.Collections.Generic;
using Cosmos.System.Network.IPv4;

namespace Cosmos.System.Network.Config;

/// <summary>
///     Contains DNS configuration
/// </summary>
public class DNSConfig
{
    /// <summary>
    ///     DNS Addresses list.
    /// </summary>
    public static List<Address> DNSNameservers = new();

    /// <summary>
    ///     Add IPv4 configuration.
    /// </summary>
    /// <param name="config"></param>
    public static void Add(Address nameserver)
    {
        for (var i = 0; i < DNSNameservers.Count; i++)
        {
            if (DNSNameservers[i].address.Equals(nameserver))
            {
                return;
            }
        }

        DNSNameservers.Add(nameserver);
    }

    /// <summary>
    ///     Remove IPv4 configuration.
    /// </summary>
    /// <param name="config"></param>
    public static void Remove(Address nameserver)
    {
        for (var i = 0; i < DNSNameservers.Count; i++)
        {
            if (DNSNameservers[i].address.Equals(nameserver))
            {
                DNSNameservers.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    ///     Call this to get your adress to request your DNS server
    /// </summary>
    /// <param name="index">Which server you want to get</param>
    /// <returns>DNS Server</returns>
    public static Address Server(int index) => DNSNameservers[index];
}
