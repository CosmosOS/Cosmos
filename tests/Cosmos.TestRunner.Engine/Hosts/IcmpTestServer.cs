using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.TestRunner.Engine.Hosts;

/// <summary>
/// Raw-Ethernet ICMP test peer for network testing with QEMU guests.
/// Slirp's hostfwd only forwards TCP/UDP, so host-sourced ICMP cannot reach the
/// guest through the user-mode backend. QEMU instead connects a stream netdev
/// (hubbed with slirp and the guest NIC) to this server, which speaks just
/// enough ARP and ICMP to ping the guest:
/// 1. Resolves the guest MAC via ARP (and answers ARP requests for its own IP)
/// 2. Sends periodic ICMP echo requests with a COSMOS_PING payload
/// 3. After validating an echo reply, switches the payload to HOST_OK so the
///    kernel test can assert the full host-to-guest-to-host round trip
/// The same peer runs the IPv6 twin of that flow from its own link-local
/// address: it resolves the guest's link-local address (derived from the
/// guest MAC) with a Neighbor Solicitation, requires a solicited Neighbor
/// Advertisement carrying the guest MAC, then sends ICMPv6 echo requests
/// (COSMOS_PING6, HOST_OK6 once a reply verified) and answers solicitations
/// for its own address.
/// </summary>
public class IcmpTestServer : IDisposable
{
    private const string ProbePayloadPrefix = "COSMOS_PING";
    private const string AckPayloadPrefix = "HOST_OK";
    private const string Probe6PayloadPrefix = "COSMOS_PING6";
    private const string Ack6PayloadPrefix = "HOST_OK6";
    private const int IcmpPayloadLength = 32;
    private const ushort EchoId = 0x4353; // 'CS'
    private const ushort EchoId6 = 0x4336; // 'C6'

    private const int Ipv6HeaderStart = 14;
    private const int Ipv6PayloadStart = 54;
    private const byte NextHeaderIcmpv6 = 58;
    private const byte NdpHopLimit = 255;

    private static readonly byte[] HostMac = { 0x52, 0x54, 0x00, 0x12, 0x34, 0x99 };
    private static readonly byte[] HostIp = { 10, 0, 2, 99 };
    private static readonly byte[] GuestIp = { 10, 0, 2, 15 }; // slirp's first DHCP address
    private static readonly byte[] BroadcastMac = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
    private static readonly byte[] HostLinkLocal = LinkLocalOf(HostMac);
    private static readonly byte[] HostSolicitedNode = SolicitedNodeOf(HostLinkLocal);

    private readonly int _port;
    private TcpListener? _listener;
    private TcpClient? _qemuClient;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private CancellationTokenSource? _cts;
    private Task? _serverTask;
    private bool _disposed;

    private byte[]? _guestMac;
    private byte[]? _guestLinkLocal;
    private ushort _nextSequence;
    private ushort _nextIpId = 0x4200;
    private bool _replyValidated;
    private readonly Dictionary<ushort, byte[]> _sentPayloads = new();

    private bool _guestNeighborResolved;
    private ushort _nextSequence6;
    private bool _reply6Validated;
    private readonly Dictionary<ushort, byte[]> _sentPayloads6 = new();

    /// <summary>
    /// Number of ICMP echo requests sent to the guest.
    /// </summary>
    public int EchoRequestsSent { get; private set; }

    /// <summary>
    /// Number of echo replies that matched a sent request (id, sequence,
    /// payload and ICMP checksum all verified).
    /// </summary>
    public int ValidEchoRepliesReceived { get; private set; }

    /// <summary>
    /// Whether the guest MAC was resolved (via its ARP reply or a learned frame).
    /// </summary>
    public bool GuestMacResolved => _guestMac != null;

    /// <summary>
    /// Number of Neighbor Solicitations sent for the guest's link-local address.
    /// </summary>
    public int NeighborSolicitationsSent { get; private set; }

    /// <summary>
    /// Number of solicited Neighbor Advertisements from the guest that carried
    /// its MAC in the target link-layer option.
    /// </summary>
    public int NeighborAdvertisementsReceived { get; private set; }

    /// <summary>
    /// Number of ICMPv6 echo requests sent to the guest.
    /// </summary>
    public int EchoRequests6Sent { get; private set; }

    /// <summary>
    /// Number of ICMPv6 echo replies that matched a sent request (id,
    /// sequence, payload and pseudo-header checksum all verified).
    /// </summary>
    public int ValidEchoReplies6Received { get; private set; }

    /// <summary>
    /// Creates a new ICMP test server.
    /// </summary>
    /// <param name="port">TCP port QEMU's stream netdev connects to (default 5560)</param>
    public IcmpTestServer(int port = 5560)
    {
        _port = port;
    }

    /// <summary>
    /// Starts listening. Must be called before QEMU starts: the stream netdev
    /// connects at QEMU startup and aborts the VM if the connection is refused.
    /// </summary>
    public void Start()
    {
        if (_cts != null)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        EchoRequestsSent = 0;
        ValidEchoRepliesReceived = 0;
        NeighborSolicitationsSent = 0;
        NeighborAdvertisementsReceived = 0;
        EchoRequests6Sent = 0;
        ValidEchoReplies6Received = 0;
        _guestMac = null;
        _guestLinkLocal = null;
        _guestNeighborResolved = false;
        _replyValidated = false;
        _reply6Validated = false;
        _sentPayloads.Clear();
        _sentPayloads6.Clear();

        try
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
            _serverTask = RunAsync(_cts.Token);
            Console.WriteLine($"[IcmpTestServer] Listening for QEMU stream netdev on port {_port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IcmpTestServer] Failed to start listener: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops the ICMP server.
    /// </summary>
    public async Task StopAsync()
    {
        if (_cts == null)
        {
            return;
        }

        _cts.Cancel();
        _qemuClient?.Close();
        _listener?.Stop();

        try
        {
            if (_serverTask != null)
            {
                await _serverTask.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }

        _cts.Dispose();
        _cts = null;
        _qemuClient = null;
        _stream = null;
        _listener = null;

        Console.WriteLine($"[IcmpTestServer] Stopped. Requests sent: {EchoRequestsSent}, valid replies: {ValidEchoRepliesReceived}");
        Console.WriteLine($"[IcmpTestServer] IPv6: solicitations sent: {NeighborSolicitationsSent}, advertisements: {NeighborAdvertisementsReceived}, requests sent: {EchoRequests6Sent}, valid replies: {ValidEchoReplies6Received}");
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            _qemuClient = await _listener!.AcceptTcpClientAsync(cancellationToken);
            _stream = _qemuClient.GetStream();
            Console.WriteLine("[IcmpTestServer] QEMU connected");
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[IcmpTestServer] Accept failed: {ex.Message}");
            return;
        }

        var readTask = ReadFramesAsync(cancellationToken);
        var pingTask = PingLoopAsync(cancellationToken);

        try
        {
            await Task.WhenAll(readTask, pingTask);
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
        catch (IOException) { }
    }

    /// <summary>
    /// Sends an ARP request until the guest MAC is known, then periodic ICMP
    /// echo requests. Payload starts as COSMOS_PING and switches to HOST_OK
    /// once a reply validated, acknowledging the round trip to the kernel.
    /// The IPv6 flow rides the same tick: a Neighbor Solicitation until the
    /// guest advertised its link-local address, then ICMPv6 echo requests.
    /// </summary>
    private async Task PingLoopAsync(CancellationToken cancellationToken)
    {
        // Wait a bit for kernel to initialize network stack
        await Task.Delay(3000, cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_guestMac == null)
                {
                    await SendFrameAsync(BuildArpRequest(), cancellationToken);
                }
                else
                {
                    ushort sequence = _nextSequence++;
                    byte[] payload = BuildPayload(_replyValidated ? AckPayloadPrefix : ProbePayloadPrefix);
                    lock (_sentPayloads)
                    {
                        _sentPayloads[sequence] = payload;
                    }

                    await SendFrameAsync(BuildEchoRequest(_guestMac, sequence, payload), cancellationToken);
                    EchoRequestsSent++;
                    Console.WriteLine($"[IcmpTestServer] Sent echo request seq={sequence} ({(_replyValidated ? AckPayloadPrefix : ProbePayloadPrefix)})");

                    await SendIpv6ProbeAsync(_guestMac, cancellationToken);
                }

                await Task.Delay(500, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (IOException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IcmpTestServer] Ping error: {ex.Message}");
                await Task.Delay(1000, cancellationToken);
            }
        }
    }

    /// <summary>
    /// One tick of the IPv6 flow: a Neighbor Solicitation for the guest's
    /// link-local address until it is advertised, then an ICMPv6 echo request
    /// whose payload switches from COSMOS_PING6 to HOST_OK6 once a reply
    /// verified.
    /// </summary>
    private async Task SendIpv6ProbeAsync(byte[] guestMac, CancellationToken cancellationToken)
    {
        if (!_guestNeighborResolved)
        {
            await SendFrameAsync(BuildNeighborSolicitation(), cancellationToken);
            NeighborSolicitationsSent++;
            Console.WriteLine($"[IcmpTestServer] Sent neighbor solicitation for {new IPAddress(_guestLinkLocal!)}");
            return;
        }

        ushort sequence = _nextSequence6++;
        byte[] payload = BuildPayload(_reply6Validated ? Ack6PayloadPrefix : Probe6PayloadPrefix);
        lock (_sentPayloads6)
        {
            _sentPayloads6[sequence] = payload;
        }

        await SendFrameAsync(BuildEchoRequest6(guestMac, sequence, payload), cancellationToken);
        EchoRequests6Sent++;
        Console.WriteLine($"[IcmpTestServer] Sent ICMPv6 echo request seq={sequence} ({(_reply6Validated ? Ack6PayloadPrefix : Probe6PayloadPrefix)})");
    }

    /// <summary>
    /// Reads length-prefixed Ethernet frames from QEMU and handles ARP, ICMP
    /// and ICMPv6 addressed to the host peer; everything else on the hub is ignored.
    /// </summary>
    private async Task ReadFramesAsync(CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[4];

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await ReadExactAsync(lengthBuffer, 4, cancellationToken);
                int frameLength = (lengthBuffer[0] << 24) | (lengthBuffer[1] << 16) | (lengthBuffer[2] << 8) | lengthBuffer[3];
                if (frameLength <= 0 || frameLength > 65535)
                {
                    Console.WriteLine($"[IcmpTestServer] Bad frame length {frameLength}, closing");
                    break;
                }

                var frame = new byte[frameLength];
                await ReadExactAsync(frame, frameLength, cancellationToken);
                HandleFrame(frame);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (IOException)
            {
                break;
            }
        }
    }

    private async Task ReadExactAsync(byte[] buffer, int count, CancellationToken cancellationToken)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = await _stream!.ReadAsync(buffer.AsMemory(offset, count - offset), cancellationToken);
            if (read == 0)
            {
                throw new IOException("QEMU stream closed");
            }
            offset += read;
        }
    }

    private void HandleFrame(byte[] frame)
    {
        if (frame.Length < 14)
        {
            return;
        }

        ushort etherType = (ushort)((frame[12] << 8) | frame[13]);
        if (etherType == 0x0806)
        {
            HandleArp(frame);
        }
        else if (etherType == 0x0800)
        {
            HandleIpv4(frame);
        }
        else if (etherType == 0x86DD)
        {
            HandleIpv6(frame);
        }
    }

    private void HandleArp(byte[] frame)
    {
        // Ethernet(14) + ARP: htype(2) ptype(2) hlen(1) plen(1) op(2)
        //                     sha(6) spa(4) tha(6) tpa(4)
        if (frame.Length < 42)
        {
            return;
        }

        ushort operation = (ushort)((frame[20] << 8) | frame[21]);
        var senderMac = frame.AsSpan(22, 6);
        var senderIp = frame.AsSpan(28, 4);
        var targetIp = frame.AsSpan(38, 4);

        // Learn the guest MAC from any ARP frame it authored
        if (senderIp.SequenceEqual(GuestIp) && _guestMac == null)
        {
            _guestMac = senderMac.ToArray();
            _guestLinkLocal = LinkLocalOf(_guestMac);
            Console.WriteLine($"[IcmpTestServer] Guest MAC resolved: {FormatMac(_guestMac)}, link-local {new IPAddress(_guestLinkLocal)}");
        }

        // Answer requests for the host peer's IP so the guest can send us replies
        if (operation == 1 && targetIp.SequenceEqual(HostIp))
        {
            byte[] reply = BuildArpReply(senderMac.ToArray(), senderIp.ToArray());
            _ = SendFrameAsync(reply, _cts?.Token ?? CancellationToken.None);
            Console.WriteLine("[IcmpTestServer] Answered ARP request for host peer IP");
        }
    }

    private void HandleIpv4(byte[] frame)
    {
        if (frame.Length < 34)
        {
            return;
        }

        int ipStart = 14;
        int ipHeaderLength = (frame[ipStart] & 0x0F) * 4;
        byte protocol = frame[ipStart + 9];
        var destIp = frame.AsSpan(ipStart + 16, 4);

        if (protocol != 1 || !destIp.SequenceEqual(HostIp))
        {
            return;
        }

        int icmpStart = ipStart + ipHeaderLength;
        int totalLength = (frame[ipStart + 2] << 8) | frame[ipStart + 3];
        int icmpLength = totalLength - ipHeaderLength;
        if (icmpLength < 8 || icmpStart + icmpLength > frame.Length)
        {
            return;
        }

        byte icmpType = frame[icmpStart];
        if (icmpType != 0)
        {
            return;
        }

        ushort id = (ushort)((frame[icmpStart + 4] << 8) | frame[icmpStart + 5]);
        ushort sequence = (ushort)((frame[icmpStart + 6] << 8) | frame[icmpStart + 7]);

        if (id != EchoId)
        {
            return;
        }

        if (ComputeChecksum(frame, icmpStart, icmpLength) != 0)
        {
            Console.WriteLine($"[IcmpTestServer] Echo reply seq={sequence} has a bad ICMP checksum");
            return;
        }

        byte[]? sentPayload;
        lock (_sentPayloads)
        {
            _sentPayloads.TryGetValue(sequence, out sentPayload);
        }

        if (sentPayload == null || icmpLength - 8 != sentPayload.Length ||
            !frame.AsSpan(icmpStart + 8, sentPayload.Length).SequenceEqual(sentPayload))
        {
            Console.WriteLine($"[IcmpTestServer] Echo reply seq={sequence} payload mismatch");
            return;
        }

        ValidEchoRepliesReceived++;
        if (!_replyValidated)
        {
            _replyValidated = true;
            Console.WriteLine($"[IcmpTestServer] Valid echo reply seq={sequence} — switching payload to {AckPayloadPrefix}");
        }
        else
        {
            Console.WriteLine($"[IcmpTestServer] Valid echo reply seq={sequence}");
        }
    }

    /// <summary>
    /// Handles ICMPv6 addressed to the host peer's link-local address or its
    /// solicited-node group: answers Neighbor Solicitations for the address,
    /// records the guest's solicited Neighbor Advertisement, and verifies echo
    /// replies. Anything whose pseudo-header checksum does not verify is dropped.
    /// </summary>
    private void HandleIpv6(byte[] frame)
    {
        if (frame.Length < Ipv6PayloadStart)
        {
            return;
        }

        int payloadLength = (frame[Ipv6HeaderStart + 4] << 8) | frame[Ipv6HeaderStart + 5];
        byte nextHeader = frame[Ipv6HeaderStart + 6];
        byte hopLimit = frame[Ipv6HeaderStart + 7];
        Span<byte> sourceIp = frame.AsSpan(Ipv6HeaderStart + 8, 16);
        Span<byte> destIp = frame.AsSpan(Ipv6HeaderStart + 24, 16);

        if (nextHeader != NextHeaderIcmpv6 || payloadLength < 8 || Ipv6PayloadStart + payloadLength > frame.Length)
        {
            return;
        }

        bool toHost = destIp.SequenceEqual(HostLinkLocal);
        if (!toHost && !destIp.SequenceEqual(HostSolicitedNode))
        {
            return;
        }

        byte icmpType = frame[Ipv6PayloadStart];
        if (ComputeIcmpv6Checksum(frame, payloadLength) != 0)
        {
            Console.WriteLine($"[IcmpTestServer] ICMPv6 type {icmpType} from {new IPAddress(sourceIp)} has a bad checksum");
            return;
        }

        switch (icmpType)
        {
            case 135:
                HandleNeighborSolicitation(frame, payloadLength, hopLimit, sourceIp);
                break;
            case 136 when toHost:
                HandleNeighborAdvertisement(frame, payloadLength, hopLimit, sourceIp);
                break;
            case 129 when toHost:
                HandleEchoReply6(frame, payloadLength);
                break;
        }
    }

    private void HandleNeighborSolicitation(byte[] frame, int payloadLength, byte hopLimit, ReadOnlySpan<byte> sourceIp)
    {
        if (payloadLength < 24 || hopLimit != NdpHopLimit)
        {
            return;
        }

        Span<byte> target = frame.AsSpan(Ipv6PayloadStart + 8, 16);
        if (!target.SequenceEqual(HostLinkLocal))
        {
            return;
        }

        byte[] reply = BuildNeighborAdvertisement(frame.AsSpan(6, 6).ToArray(), sourceIp.ToArray());
        _ = SendFrameAsync(reply, _cts?.Token ?? CancellationToken.None);
        Console.WriteLine($"[IcmpTestServer] Answered neighbor solicitation from {new IPAddress(sourceIp)}");
    }

    private void HandleNeighborAdvertisement(byte[] frame, int payloadLength, byte hopLimit, ReadOnlySpan<byte> sourceIp)
    {
        if (payloadLength < 24 || hopLimit != NdpHopLimit || _guestLinkLocal == null || _guestMac == null)
        {
            return;
        }

        Span<byte> target = frame.AsSpan(Ipv6PayloadStart + 8, 16);
        if (!target.SequenceEqual(_guestLinkLocal))
        {
            return;
        }

        bool solicited = (frame[Ipv6PayloadStart + 4] & 0x40) != 0;
        if (!solicited)
        {
            Console.WriteLine("[IcmpTestServer] Neighbor advertisement from guest is not marked solicited");
            return;
        }

        byte[]? targetMac = FindLinkLayerOption(frame, Ipv6PayloadStart + 24, Ipv6PayloadStart + payloadLength, 2);
        if (targetMac == null || !targetMac.AsSpan().SequenceEqual(_guestMac))
        {
            Console.WriteLine("[IcmpTestServer] Neighbor advertisement from guest lacks a matching target link-layer option");
            return;
        }

        NeighborAdvertisementsReceived++;
        if (!_guestNeighborResolved)
        {
            _guestNeighborResolved = true;
            Console.WriteLine($"[IcmpTestServer] Guest link-local {new IPAddress(sourceIp)} resolved to {FormatMac(targetMac)}");
        }
    }

    private void HandleEchoReply6(byte[] frame, int payloadLength)
    {
        int icmpStart = Ipv6PayloadStart;
        ushort id = (ushort)((frame[icmpStart + 4] << 8) | frame[icmpStart + 5]);
        ushort sequence = (ushort)((frame[icmpStart + 6] << 8) | frame[icmpStart + 7]);

        if (id != EchoId6)
        {
            return;
        }

        byte[]? sentPayload;
        lock (_sentPayloads6)
        {
            _sentPayloads6.TryGetValue(sequence, out sentPayload);
        }

        if (sentPayload == null || payloadLength - 8 != sentPayload.Length ||
            !frame.AsSpan(icmpStart + 8, sentPayload.Length).SequenceEqual(sentPayload))
        {
            Console.WriteLine($"[IcmpTestServer] ICMPv6 echo reply seq={sequence} payload mismatch");
            return;
        }

        ValidEchoReplies6Received++;
        if (!_reply6Validated)
        {
            _reply6Validated = true;
            Console.WriteLine($"[IcmpTestServer] Valid ICMPv6 echo reply seq={sequence}: switching payload to {Ack6PayloadPrefix}");
        }
        else
        {
            Console.WriteLine($"[IcmpTestServer] Valid ICMPv6 echo reply seq={sequence}");
        }
    }

    private async Task SendFrameAsync(byte[] frame, CancellationToken cancellationToken)
    {
        // Pad to the Ethernet minimum; real NIC models may drop runt frames
        if (frame.Length < 60)
        {
            Array.Resize(ref frame, 60);
        }

        var lengthPrefix = new byte[4]
        {
            (byte)(frame.Length >> 24),
            (byte)(frame.Length >> 16),
            (byte)(frame.Length >> 8),
            (byte)frame.Length
        };

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await _stream!.WriteAsync(lengthPrefix, cancellationToken);
            await _stream.WriteAsync(frame, cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private static byte[] BuildPayload(string prefix)
    {
        var payload = new byte[IcmpPayloadLength];
        byte[] prefixBytes = Encoding.ASCII.GetBytes(prefix);
        Array.Copy(prefixBytes, payload, prefixBytes.Length);
        for (int i = prefixBytes.Length; i < payload.Length; i++)
        {
            payload[i] = (byte)i;
        }

        return payload;
    }

    private static byte[] BuildArpRequest()
    {
        var frame = new byte[42];
        WriteEthernetHeader(frame, BroadcastMac, 0x0806);
        WriteArpBody(frame, 1, HostMac, HostIp, new byte[6], GuestIp);
        return frame;
    }

    private static byte[] BuildArpReply(byte[] targetMac, byte[] targetIp)
    {
        var frame = new byte[42];
        WriteEthernetHeader(frame, targetMac, 0x0806);
        WriteArpBody(frame, 2, HostMac, HostIp, targetMac, targetIp);
        return frame;
    }

    private static void WriteEthernetHeader(byte[] frame, byte[] destMac, ushort etherType)
    {
        Array.Copy(destMac, 0, frame, 0, 6);
        Array.Copy(HostMac, 0, frame, 6, 6);
        frame[12] = (byte)(etherType >> 8);
        frame[13] = (byte)etherType;
    }

    private static void WriteArpBody(byte[] frame, ushort operation, byte[] senderMac, byte[] senderIp, byte[] targetMac, byte[] targetIp)
    {
        frame[14] = 0x00;
        frame[15] = 0x01; // Ethernet
        frame[16] = 0x08;
        frame[17] = 0x00; // IPv4
        frame[18] = 6;
        frame[19] = 4;
        frame[20] = (byte)(operation >> 8);
        frame[21] = (byte)operation;
        Array.Copy(senderMac, 0, frame, 22, 6);
        Array.Copy(senderIp, 0, frame, 28, 4);
        Array.Copy(targetMac, 0, frame, 32, 6);
        Array.Copy(targetIp, 0, frame, 38, 4);
    }

    private byte[] BuildEchoRequest(byte[] guestMac, ushort sequence, byte[] payload)
    {
        int icmpLength = 8 + payload.Length;
        int ipLength = 20 + icmpLength;
        var frame = new byte[14 + ipLength];

        WriteEthernetHeader(frame, guestMac, 0x0800);

        // IPv4 header
        frame[14] = 0x45;
        frame[15] = 0x00;
        frame[16] = (byte)(ipLength >> 8);
        frame[17] = (byte)ipLength;
        ushort ipId = _nextIpId++;
        frame[18] = (byte)(ipId >> 8);
        frame[19] = (byte)ipId;
        frame[20] = 0x00;
        frame[21] = 0x00;
        frame[22] = 64; // TTL
        frame[23] = 1;  // ICMP
        frame[24] = 0x00;
        frame[25] = 0x00;
        Array.Copy(HostIp, 0, frame, 26, 4);
        Array.Copy(GuestIp, 0, frame, 30, 4);
        ushort ipChecksum = ComputeChecksum(frame, 14, 20);
        frame[24] = (byte)(ipChecksum >> 8);
        frame[25] = (byte)ipChecksum;

        // ICMP echo request
        frame[34] = 8;
        frame[35] = 0;
        frame[36] = 0x00;
        frame[37] = 0x00;
        frame[38] = (byte)(EchoId >> 8);
        frame[39] = (byte)(EchoId & 0xFF);
        frame[40] = (byte)(sequence >> 8);
        frame[41] = (byte)sequence;
        Array.Copy(payload, 0, frame, 42, payload.Length);
        ushort icmpChecksum = ComputeChecksum(frame, 34, icmpLength);
        frame[36] = (byte)(icmpChecksum >> 8);
        frame[37] = (byte)icmpChecksum;

        return frame;
    }

    /// <summary>
    /// Neighbor Solicitation for the guest's link-local address, sent to its
    /// solicited-node group with a source link-layer option, as RFC 4861
    /// section 4.3 requires of a unicast-sourced solicitation.
    /// </summary>
    private byte[] BuildNeighborSolicitation()
    {
        byte[] group = SolicitedNodeOf(_guestLinkLocal!);
        byte[] icmp = new byte[32];
        icmp[0] = 135;
        Array.Copy(_guestLinkLocal!, 0, icmp, 8, 16);
        icmp[24] = 1; // source link-layer address option
        icmp[25] = 1; // one 8-byte unit
        Array.Copy(HostMac, 0, icmp, 26, 6);
        return BuildIpv6Frame(MulticastMacOf(group), HostLinkLocal, group, NdpHopLimit, icmp);
    }

    /// <summary>
    /// Solicited Neighbor Advertisement for the host peer's own address, with
    /// the override flag and a target link-layer option.
    /// </summary>
    private static byte[] BuildNeighborAdvertisement(byte[] destMac, byte[] destIp)
    {
        byte[] icmp = new byte[32];
        icmp[0] = 136;
        icmp[4] = 0x60; // solicited, override
        Array.Copy(HostLinkLocal, 0, icmp, 8, 16);
        icmp[24] = 2; // target link-layer address option
        icmp[25] = 1;
        Array.Copy(HostMac, 0, icmp, 26, 6);
        return BuildIpv6Frame(destMac, HostLinkLocal, destIp, NdpHopLimit, icmp);
    }

    private byte[] BuildEchoRequest6(byte[] guestMac, ushort sequence, byte[] payload)
    {
        byte[] icmp = new byte[8 + payload.Length];
        icmp[0] = 128;
        icmp[4] = (byte)(EchoId6 >> 8);
        icmp[5] = (byte)(EchoId6 & 0xFF);
        icmp[6] = (byte)(sequence >> 8);
        icmp[7] = (byte)sequence;
        Array.Copy(payload, 0, icmp, 8, payload.Length);
        return BuildIpv6Frame(guestMac, HostLinkLocal, _guestLinkLocal!, 64, icmp);
    }

    /// <summary>
    /// Wraps an ICMPv6 section in an IPv6 header and Ethernet frame and
    /// fills in the pseudo-header checksum.
    /// </summary>
    private static byte[] BuildIpv6Frame(byte[] destMac, byte[] sourceIp, byte[] destIp, byte hopLimit, byte[] icmp)
    {
        byte[] frame = new byte[Ipv6PayloadStart + icmp.Length];
        WriteEthernetHeader(frame, destMac, 0x86DD);
        frame[Ipv6HeaderStart] = 0x60;
        frame[Ipv6HeaderStart + 4] = (byte)(icmp.Length >> 8);
        frame[Ipv6HeaderStart + 5] = (byte)icmp.Length;
        frame[Ipv6HeaderStart + 6] = NextHeaderIcmpv6;
        frame[Ipv6HeaderStart + 7] = hopLimit;
        Array.Copy(sourceIp, 0, frame, Ipv6HeaderStart + 8, 16);
        Array.Copy(destIp, 0, frame, Ipv6HeaderStart + 24, 16);
        Array.Copy(icmp, 0, frame, Ipv6PayloadStart, icmp.Length);
        frame[Ipv6PayloadStart + 2] = 0;
        frame[Ipv6PayloadStart + 3] = 0;
        ushort checksum = ComputeIcmpv6Checksum(frame, icmp.Length);
        frame[Ipv6PayloadStart + 2] = (byte)(checksum >> 8);
        frame[Ipv6PayloadStart + 3] = (byte)checksum;
        return frame;
    }

    /// <summary>
    /// RFC 8200 section 8.1 upper-layer checksum: the pseudo-header (source,
    /// destination, payload length, next header) and the ICMPv6 section.
    /// Returns 0 when run over a section whose stored checksum is valid.
    /// </summary>
    private static ushort ComputeIcmpv6Checksum(byte[] frame, int payloadLength)
    {
        uint sum = 0;
        for (int i = Ipv6HeaderStart + 8; i < Ipv6PayloadStart; i += 2)
        {
            sum += (uint)((frame[i] << 8) | frame[i + 1]);
        }

        sum += (uint)payloadLength;
        sum += NextHeaderIcmpv6;

        int end = Ipv6PayloadStart + (payloadLength & ~1);
        for (int i = Ipv6PayloadStart; i < end; i += 2)
        {
            sum += (uint)((frame[i] << 8) | frame[i + 1]);
        }

        if ((payloadLength & 1) != 0)
        {
            sum += (uint)(frame[end] << 8);
        }

        while ((sum >> 16) != 0)
        {
            sum = (sum & 0xFFFF) + (sum >> 16);
        }

        return (ushort)~sum;
    }

    /// <summary>
    /// RFC 1071 ones'-complement checksum. Returns 0 when run over a section
    /// whose stored checksum is valid.
    /// </summary>
    private static ushort ComputeChecksum(byte[] buffer, int offset, int length)
    {
        uint sum = 0;
        int i = offset;
        int end = offset + (length & ~1);

        while (i < end)
        {
            sum += (uint)((buffer[i] << 8) | buffer[i + 1]);
            i += 2;
        }

        if ((length & 1) != 0)
        {
            sum += (uint)(buffer[end] << 8);
        }

        while ((sum >> 16) != 0)
        {
            sum = (sum & 0xFFFF) + (sum >> 16);
        }

        return (ushort)~sum;
    }

    /// <summary>
    /// The link-layer address carried by the first option of the given type
    /// between <paramref name="start"/> and <paramref name="end"/>, or null.
    /// </summary>
    private static byte[]? FindLinkLayerOption(byte[] frame, int start, int end, byte optionType)
    {
        int offset = start;
        while (offset + 2 <= end)
        {
            int length = frame[offset + 1] * 8;
            if (length == 0)
            {
                return null;
            }

            if (frame[offset] == optionType && length == 8 && offset + 8 <= end)
            {
                return frame.AsSpan(offset + 2, 6).ToArray();
            }

            offset += length;
        }

        return null;
    }

    /// <summary>
    /// RFC 4291 Appendix A: fe80::/64 with the modified EUI-64 of the MAC.
    /// </summary>
    private static byte[] LinkLocalOf(byte[] mac)
    {
        return new byte[]
        {
            0xFE, 0x80, 0, 0, 0, 0, 0, 0,
            (byte)(mac[0] ^ 0x02), mac[1], mac[2], 0xFF, 0xFE, mac[3], mac[4], mac[5]
        };
    }

    /// <summary>
    /// RFC 4291 section 2.7.1: ff02::1:ff00:0/104 with the low 24 bits of the address.
    /// </summary>
    private static byte[] SolicitedNodeOf(byte[] address)
    {
        return new byte[]
        {
            0xFF, 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x01, 0xFF, address[13], address[14], address[15]
        };
    }

    /// <summary>
    /// RFC 2464 section 7: 33:33 followed by the low 32 bits of the group.
    /// </summary>
    private static byte[] MulticastMacOf(byte[] group)
    {
        return new byte[] { 0x33, 0x33, group[12], group[13], group[14], group[15] };
    }

    private static string FormatMac(byte[] mac) =>
        $"{mac[0]:X2}:{mac[1]:X2}:{mac[2]:X2}:{mac[3]:X2}:{mac[4]:X2}:{mac[5]:X2}";

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cts?.Cancel();
        _qemuClient?.Dispose();
        _listener?.Stop();
        _cts?.Dispose();
        _writeLock.Dispose();
    }
}
