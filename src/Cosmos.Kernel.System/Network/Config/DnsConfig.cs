/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DNS Config
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.Kernel.System.Network.IPv4;

namespace Cosmos.Kernel.System.Network.Config;

/// <summary>
/// Represents DNS configuration.
/// </summary>
public static class DnsConfig
{
    private static readonly List<Address> s_nameservers = new();

    /// <summary>
    /// The list of known DNS nameserver addresses. Use <see cref="Add"/> and
    /// <see cref="Remove"/> to change it.
    /// </summary>
    public static IReadOnlyList<Address> Nameservers => s_nameservers;

    /// <summary>
    /// Registers a given DNS server.
    /// </summary>
    /// <param name="nameserver">The IP address of the target DNS server.</param>
    public static void Add(Address nameserver)
    {
        for (int i = 0; i < Nameservers.Count; i++)
        {
            if (Nameservers[i].Id == nameserver.Id)
            {
                return;
            }
        }
        s_nameservers.Add(nameserver);
    }

    /// <summary>
    /// Removes the given DNS server from the list of registered nameservers.
    /// </summary>
    /// <param name="nameserver">The IP address of the target DNS server.</param>
    public static void Remove(Address nameserver)
    {
        Address? toRemove = null;
        for (int i = 0; i < Nameservers.Count; i++)
        {
            if (Nameservers[i].Id == nameserver.Id)
            {
                toRemove = Nameservers[i];
                break;
            }
        }
        if (toRemove != null)
        {
            s_nameservers.Remove(toRemove);
        }
    }
}
