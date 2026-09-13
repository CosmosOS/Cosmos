/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DNS Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Timer;

namespace Cosmos.Kernel.System.Network.IPv4.UDP.DNS;

/// <summary>
/// Used to manage a DNS connection to a server.
/// </summary>
public sealed class DnsClient : UdpClient
{
    private string? _queryUrl;

    /// <summary>
    /// Create new instance of the <see cref="DnsClient"/> class.
    /// </summary>
    public DnsClient() : base(53)
    {
    }

    /// <summary>
    /// Connects to a client.
    /// </summary>
    /// <param name="address">Destination address.</param>
    public void Connect(Address address)
    {
        Connect(address, 53);
    }

    /// <summary>
    /// Sends a DNS query for the given domain name string.
    /// </summary>
    /// <param name="url">The domain name string to query the DNS for.</param>
    /// <exception cref="InvalidOperationException">No DNS server has been set
    /// with Connect, or no configured interface can reach it.</exception>
    public void SendQuery(string url)
    {
        if (_destination is null)
        {
            throw new InvalidOperationException("No network route to DNS server. Run 'netconfig' or 'dhcp' first.");
        }

        Address source = IPConfig.FindNetwork(_destination)
            ?? throw new InvalidOperationException("No network route to DNS server. Run 'netconfig' or 'dhcp' first.");
        _queryUrl = url;
        DnsPacketQuery askpacket = new(source, _destination!, url);

        OutgoingBuffer.AddPacket(askpacket);
        NetworkStack.Update();
    }

    /// <summary>
    /// Receives data from the DNS remote host.
    /// </summary>
    /// <param name="timeout">The timeout value - by default 5000ms.</param>
    /// <returns>The first address for the queried name, or
    /// <see langword="null"/>. The null covers every way a lookup can fail
    /// (no reply before the timeout, a reply for a different name, a server
    /// error code, a name that does not exist, a reply carrying no A record),
    /// so a caller that needs to tell them apart must read the reply packet
    /// itself off the seam.</returns>
    public Address? Receive(int timeout = 5000)
    {
        List<Address>? addresses = ReceiveAll(timeout);
        return addresses is { Count: > 0 } ? addresses[0] : null;
    }

    /// <summary>
    /// Resolves any CNAME chain and returns every A record for the final name.
    /// </summary>
    /// <param name="timeout">The timeout value - by default 5000ms.</param>
    /// <returns>All resolved addresses, in server order, or
    /// <see langword="null"/> for any failure or timeout; see
    /// <see cref="Receive"/> for what the null covers.</returns>
    public List<Address>? ReceiveAll(int timeout = 5000)
    {
        // Wait in 100ms intervals, checking for data each time
        int waited = 0;
        while (_rxBuffer.Count < 1 && waited < timeout)
        {
            TimerManager.Wait(100);
            waited += 100;
        }

        if (_rxBuffer.Count < 1)
        {
            return null;
        }

        DnsPacketAnswer packet = new(_rxBuffer.Dequeue().RawData);

        if ((ushort)(packet.DnsFlags & 0x0F) != (ushort)ReplyCode.OK)
        {
            return null;
        }

        // Reject mismatched or unsolicited replies (e.g. spoofed/stray packets).
        if (packet.Queries is null || packet.Queries.Count == 0 ||
            !string.Equals(packet.Queries[0].Name, _queryUrl, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (packet.Answers is null || packet.Answers.Count == 0)
        {
            return null;
        }

        ArgumentNullException.ThrowIfNull(_queryUrl);
        return ResolveAddresses(packet.Answers, _queryUrl);
    }

    /// <summary>
    /// Follows a CNAME chain from <paramref name="name"/>, then collects the final A records.
    /// </summary>
    private static List<Address>? ResolveAddresses(List<DnsAnswer> answers, string name)
    {
        string current = name;

        // Guards against a CNAME loop.
        HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);

        // At most one CNAME hop per answer record.
        for (int i = 0; i < answers.Count; i++)
        {
            DnsAnswer? cname = answers.Find(a =>
                a.Type == DnsRecordType.CNAME &&
                a.ResolvedName is not null &&
                string.Equals(a.ResolvedName, current, StringComparison.OrdinalIgnoreCase));

            if (cname?.CanonicalName is null)
            {
                break;
            }

            if (!visited.Add(current))
            {
                // CNAME loop detected.
                return null;
            }

            current = cname.CanonicalName;
        }

        // Collect the A records for the final name.
        List<Address> results = [];
        foreach (DnsAnswer record in answers)
        {
            if (record.Type == DnsRecordType.A &&
                record.Address is { Length: 4 } &&
                record.ResolvedName is not null &&
                string.Equals(record.ResolvedName, current, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new Address(record.Address, 0));
            }
        }

        return results.Count > 0 ? results : null;
    }
}
