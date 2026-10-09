using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Network;
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

    // How long an answer is kept, and how many: a web page asks for the same few hosts again and
    // again (a connection for every file), and each lookup is a round trip to the nameserver.
    private const int CacheMilliseconds = 60_000;
    private const int CacheSize = 64;

    // The names resolved lately, oldest first. Replaced whole, never changed: any thread reads it
    // without a lock, and a lookup racing another one only loses an entry.
    private static CachedName[] s_cache = [];

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

        if (TryGetCached(hostName, out IPAddress[] cached))
        {
            addresses = cached;
            return SocketError.Success;
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
        if (addresses.Length == 0)
        {
            return SocketError.HostNotFound;
        }

        Remember(hostName, addresses);
        return SocketError.Success;
    }

    /// <summary>The addresses a name resolved to less than <see cref="CacheMilliseconds"/> ago, a copy the caller may change.</summary>
    private static bool TryGetCached(string name, out IPAddress[] addresses)
    {
        CachedName[] cache = Volatile.Read(ref s_cache);
        long now = Stopwatch.GetTimestamp();
        for (int i = cache.Length - 1; i >= 0; i--)
        {
            CachedName entry = cache[i];
            if (now < entry.Expires && string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                addresses = [.. entry.Addresses];
                return true;
            }
        }

        addresses = [];
        return false;
    }

    /// <summary>Keeps a name's addresses for <see cref="CacheMilliseconds"/>, dropping its expired entries and the oldest past <see cref="CacheSize"/>.</summary>
    private static void Remember(string name, IPAddress[] addresses)
    {
        CachedName[] cache = Volatile.Read(ref s_cache);
        long now = Stopwatch.GetTimestamp();
        List<CachedName> kept = new(cache.Length + 1);
        foreach (CachedName entry in cache)
        {
            if (now < entry.Expires && !string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                kept.Add(entry);
            }
        }

        if (kept.Count >= CacheSize)
        {
            kept.RemoveRange(0, kept.Count - CacheSize + 1);
        }

        kept.Add(new CachedName(name, [.. addresses], now + Stopwatch.Frequency * CacheMilliseconds / 1000));
        Volatile.Write(ref s_cache, [.. kept]);
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

    /// <summary>A name resolved, its addresses and when they go stale, as a <see cref="Stopwatch"/> timestamp.</summary>
    private sealed class CachedName(string name, IPAddress[] addresses, long expires)
    {
        public readonly string Name = name;
        public readonly IPAddress[] Addresses = addresses;
        public readonly long Expires = expires;
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
