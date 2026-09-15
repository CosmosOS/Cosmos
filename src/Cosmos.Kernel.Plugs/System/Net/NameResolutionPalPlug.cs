using System.Net;
using System.Net.Sockets;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.DNS;
using Cosmos.Kernel.System.Network.IPv4;
using AddressFamily = System.Net.Sockets.AddressFamily;

namespace Cosmos.Kernel.Plugs.System.Net;

/// <summary>
/// Routes name resolution through the Cosmos resolver. Every entry point on
/// <see cref="Dns"/> reaches the platform through three members of this type,
/// so plugging them covers the blocking overloads, the Begin/End pairs and the
/// Task-returning ones alike: on this platform <c>SupportsGetAddrInfoAsync</c>
/// is a compile-time false, which makes the asynchronous overloads run the
/// blocking path on the thread pool.
/// </summary>
[Plug("System.Net.NameResolutionPal")]
public static class NameResolutionPalPlug
{
    /// <summary>
    /// How long a lookup waits for a reply. <see cref="Dns"/> exposes no
    /// timeout of its own, so this is the only one a caller gets.
    /// </summary>
    private const int QueryTimeoutMs = 5000;

    [PlugMember]
    public static string GetHostName() => DnsConfig.HostName;

    [PlugMember]
    public static SocketError TryGetAddrInfo(string name, bool justAddresses, AddressFamily addressFamily,
        out string? hostName, out string[] aliases, out IPAddress[] addresses, out int nativeErrorCode)
    {
        // The Cosmos resolver reports a CNAME chain only as the name it ends
        // at, which it has already followed, so there is no alias list to hand
        // back and no native error code underneath to report.
        aliases = [];
        nativeErrorCode = 0;
        addresses = [];

        // Documented behaviour: an empty name means the local host.
        hostName = name.Length == 0 ? DnsConfig.HostName : name;

        if (addressFamily is not (AddressFamily.Unspecified or AddressFamily.InterNetwork))
        {
            // A plugged IPAddress stores four bytes, so an IPv6 answer has
            // nothing to be returned in. Refusing the family is honest;
            // answering with an empty success would read as "no such host".
            return SocketError.AddressFamilyNotSupported;
        }

        if (DnsConfig.Nameservers.Count == 0)
        {
            Log.WriteString("[NameResolutionPalPlug] No nameserver configured. Run DHCP or add one to DnsConfig.\n");
            return SocketError.NoRecovery;
        }

        List<Address>? resolved;
        try
        {
            resolved = Resolve(hostName);
        }
        catch (InvalidOperationException)
        {
            // No configured interface can reach the nameserver.
            return SocketError.NetworkUnreachable;
        }

        if (resolved is null)
        {
            return SocketError.HostNotFound;
        }

        addresses = ToIPv4Addresses(resolved);
        return addresses.Length == 0 ? SocketError.HostNotFound : SocketError.Success;
    }

    [PlugMember]
    public static string? TryGetNameInfo(IPAddress addr, out SocketError socketError, out int nativeErrorCode)
    {
        // A reverse lookup is a PTR query against in-addr.arpa, which the
        // Cosmos resolver does not send. Report the failure rather than
        // inventing a name for the address.
        socketError = SocketError.HostNotFound;
        nativeErrorCode = 0;
        return null;
    }

    /// <summary>
    /// Runs one lookup on its own client, so a caller of <see cref="Dns"/> does
    /// not have to own resolver state. The client binds the DNS port for the
    /// duration, which is why it is closed on every path out.
    /// </summary>
    private static List<Address>? Resolve(string name)
    {
        DnsClient client = new();
        try
        {
            client.Connect(DnsConfig.Nameservers[0]);
            client.SendQuery(name);
            return client.ReceiveAll(QueryTimeoutMs);
        }
        finally
        {
            client.Close();
        }
    }

    /// <summary>
    /// Keeps the answers a plugged <see cref="IPAddress"/> can actually carry.
    /// </summary>
    private static IPAddress[] ToIPv4Addresses(List<Address> resolved)
    {
        List<IPAddress> kept = [];
        foreach (Address address in resolved)
        {
            if (address is Address4)
            {
                kept.Add(new IPAddress(address.ToBytes()));
            }
        }

        return [.. kept];
    }
}
