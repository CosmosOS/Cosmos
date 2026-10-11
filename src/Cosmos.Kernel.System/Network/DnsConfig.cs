// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DNS Config
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

namespace Cosmos.Kernel.System.Network;

/// <summary>
/// Represents DNS configuration.
/// </summary>
public static class DnsConfig
{
    private static readonly List<Address> s_nameservers = [];

    /// <summary>
    /// The list of known DNS nameserver addresses. Use <see cref="Add"/> and
    /// <see cref="Remove"/> to change it.
    /// </summary>
    public static IReadOnlyList<Address> Nameservers => s_nameservers;

    /// <summary>
    /// The name this machine answers to. It is what the plugged
    /// <c>System.Net.Dns.GetHostName</c> reports, and what resolving an empty
    /// name resolves instead. Nothing in the kernel or in DHCP sets it, so it
    /// keeps its default until a kernel assigns one.
    /// </summary>
    public static string HostName { get; set; } = "cosmos";

    /// <summary>
    /// Registers a given DNS server. An address already registered is not
    /// added a second time.
    /// </summary>
    /// <param name="nameserver">The IP address of the target DNS server.</param>
    public static void Add(Address nameserver)
    {
        for (int i = 0; i < s_nameservers.Count; i++)
        {
            if (Equals(s_nameservers[i], nameserver))
            {
                return;
            }
        }

        s_nameservers.Add(nameserver);
    }

    /// <summary>
    /// Removes the given DNS server from the list of registered nameservers.
    /// Does nothing when the address is not registered.
    /// </summary>
    /// <param name="nameserver">The IP address of the target DNS server.</param>
    public static void Remove(Address nameserver)
    {
        for (int i = 0; i < s_nameservers.Count; i++)
        {
            if (Equals(s_nameservers[i], nameserver))
            {
                s_nameservers.RemoveAt(i);
                return;
            }
        }
    }
}
