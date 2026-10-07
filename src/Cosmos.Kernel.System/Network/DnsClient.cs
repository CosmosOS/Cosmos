// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

/*
* PROJECT:          Cosmos OS Development
* CONTENT:          DNS Client
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*                   Port of Cosmos Code.
*/

using Cosmos.Kernel.System.Network.Protocols.Dns;
using Cosmos.Kernel.System.Timers;

namespace Cosmos.Kernel.System.Network;

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
    /// Connects to a DNS server on port 53.
    /// </summary>
    /// <param name="address">The DNS server address.</param>
    public void Connect(Address address) => Connect(address, 53);

    /// <summary>
    /// Sends a DNS query for the given domain name string.
    /// </summary>
    /// <param name="url">The domain name string to query the DNS for.</param>
    /// <param name="recordType">The record type to ask for, one of
    /// <see cref="DnsRecordType"/>. Defaults to <see cref="DnsRecordType.A"/>.
    /// Which version carries the query is decided by the server address passed
    /// to <see cref="Connect(Address)"/>, not by this.</param>
    /// <exception cref="InvalidOperationException">No DNS server has been set
    /// with Connect, or no configured interface can reach it.</exception>
    public void SendQuery(string url, ushort recordType = DnsRecordType.A)
    {
        if (_destination is null)
        {
            throw new InvalidOperationException("No network route to DNS server. Run 'netconfig' or 'dhcp' first.");
        }

        Address source = IPConfig.FindNetwork(_destination)
            ?? throw new InvalidOperationException("No network route to DNS server. Run 'netconfig' or 'dhcp' first.");
        _queryUrl = url;
        DnsPacketQuery askPacket = new(source, _destination, url, recordType);

        askPacket.Network.Enqueue();
        NetworkStack.Update();
    }

    /// <summary>
    /// Receives data from the DNS remote host.
    /// </summary>
    /// <param name="timeout">The timeout value - by default 5000ms.</param>
    /// <returns>The first address for the queried name, or
    /// <see langword="null"/>. The null covers every way a lookup can fail
    /// (no reply before the timeout, a reply for a different name, a server
    /// error code, a name that does not exist, a reply carrying no address record),
    /// so a caller that needs to tell them apart must read the reply packet
    /// itself off the seam.</returns>
    public Address? Receive(int timeout = 5000)
    {
        List<Address>? addresses = ReceiveAll(timeout);
        return addresses is { Count: > 0 } ? addresses[0] : null;
    }

    /// <summary>
    /// Resolves any CNAME chain and returns every address record for the final name.
    /// </summary>
    /// <param name="timeout">The timeout value - by default 5000ms.</param>
    /// <returns>All resolved addresses, in server order, or
    /// <see langword="null"/> for any failure or timeout; see
    /// <see cref="Receive"/> for what the null covers.</returns>
    public List<Address>? ReceiveAll(int timeout = 5000)
    {
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
        if (_queryUrl is null || packet.Queries is null || packet.Queries.Count == 0 ||
            !string.Equals(packet.Queries[0].Name, _queryUrl, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (packet.Answers is null || packet.Answers.Count == 0)
        {
            return null;
        }

        return ResolveAddresses(packet.Answers, _queryUrl);
    }

    /// <summary>
    /// Follows a CNAME chain from <paramref name="name"/>, then collects the final address records.
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
                return null;
            }

            current = cname.CanonicalName;
        }

        // Collect the address records for the final name. A reply is free to
        // carry both types, and the record length is what decides which
        // address this is, so a record whose length disagrees with its type is
        // skipped rather than read past.
        List<Address> results = [];
        foreach (DnsAnswer record in answers)
        {
            if (record.ResolvedName is null ||
                !string.Equals(record.ResolvedName, current, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (record.Type == DnsRecordType.A && record.Address is { Length: 4 })
            {
                results.Add(new Address4(record.Address, 0));
            }
            else if (record.Type == DnsRecordType.AAAA && record.Address is { Length: 16 })
            {
                results.Add(new Address6(record.Address, 0));
            }
        }

        return results.Count > 0 ? results : null;
    }
}
