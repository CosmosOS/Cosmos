using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Timer;

namespace Cosmos.Kernel.System.Network.IPv4;

/// <summary>
/// Used to manage the ICMP connection to a client.
/// </summary>
public sealed class IcmpClient : IDisposable
{
    private static readonly Dictionary<uint, IcmpClient> s_clients = new();

    /// <summary>
    /// Destination address.
    /// </summary>
    internal Address? _destination;

    /// <summary>
    /// The RX buffer queue.
    /// </summary>
    internal Queue<IcmpPacket> _rxBuffer;
    private bool _disposed;

    /// <summary>
    /// Throws once <see cref="Dispose"/> has run. <see cref="Close"/> does not
    /// arm this: closing only stops delivery to this client, and
    /// <see cref="Connect"/> reopens it.
    /// </summary>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// Gets a client by its IP address hash.
    /// </summary>
    /// <param name="iphash">The IP address hash.</param>
    /// <returns>If a client is connected to the given address, the <see cref="IcmpClient"/>; otherwise, <see langword="null"/>.</returns>
    internal static IcmpClient? GetClient(uint iphash)
    {
        if (s_clients.TryGetValue(iphash, out IcmpClient? client))
        {
            return client;
        }
        return null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IcmpClient"/> class.
    /// </summary>
    public IcmpClient()
    {
        _rxBuffer = new Queue<IcmpPacket>(8);
    }

    /// <summary>
    /// Connects to the given client.
    /// </summary>
    /// <param name="dest">Destination address.</param>
    public void Connect(Address dest)
    {
        ThrowIfDisposed();

        // Reconnecting to a second host has to drop the first registration:
        // s_clients is static, so a stale entry both misroutes replies and
        // roots this client for the life of the kernel.
        Close();

        _destination = dest;
        s_clients[dest.Id] = this;
    }

    /// <summary>
    /// Closes the active connection.
    /// </summary>
    public void Close()
    {
        if (_destination is not null)
        {
            s_clients.Remove(_destination.Id);
        }
    }

    /// <summary>
    /// Sends an ICMP echo request to the connected destination.
    /// </summary>
    /// <param name="id">The echo identifier.</param>
    /// <param name="sequence">The echo sequence number.</param>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    /// <exception cref="InvalidOperationException">No destination has been set,
    /// or no configured interface can reach it.</exception>
    public void SendEcho(ushort id = 0x0001, ushort sequence = 0x0001)
    {
        ThrowIfDisposed();

        if (_destination is null)
        {
            throw new InvalidOperationException("Call Connect before using SendEcho.");
        }

        Address source = IPConfig.FindNetwork(_destination) ?? throw new InvalidOperationException("No configured interface can reach the destination address.");
        IcmpEchoRequest request = new(source, _destination, id, sequence);
        OutgoingBuffer.AddPacket(request);
        NetworkStack.Update();
    }

    /// <summary>
    /// Transmits a prebuilt ICMP packet through the stack's outgoing queue,
    /// including ARP resolution of the destination.
    /// </summary>
    /// <param name="packet">The packet to transmit; its headers and checksum must already be final.</param>
    /// <returns><see langword="false"/> when no configured interface matches the packet's source address; otherwise, <see langword="true"/>.</returns>
    [Experimental(Experimentals.PacketSeamDiagId)]
    public bool Send(IcmpPacket packet)
    {
        ThrowIfDisposed();

        return Cosmos.Kernel.System.Network.NetworkStack.Send(packet);
    }

    /// <summary>
    /// Receives one ICMP packet from this client's receive queue. Unlike
    /// <see cref="Receive"/>, this hands back the parsed packet object so the
    /// caller can read the ICMP identifier, sequence number, and payload.
    /// </summary>
    /// <param name="timeoutMs">The timeout in milliseconds; a non-positive value checks the queue once without waiting.</param>
    /// <returns>The dequeued <see cref="IcmpPacket"/>, or <see langword="null"/> if none arrived before the timeout.</returns>
    [Experimental(Experimentals.PacketSeamDiagId)]
    public IcmpPacket? ReceivePacket(int timeoutMs = 5000)
    {
        ThrowIfDisposed();

        int waited = 0;
        while (_rxBuffer.Count < 1 && waited < timeoutMs)
        {
            TimerManager.Wait(10);
            waited += 10;
        }

        if (_rxBuffer.Count < 1)
        {
            return null;
        }

        return _rxBuffer.Dequeue();
    }

    /// <summary>
    /// Receives an ICMP echo reply from the remote host.
    /// </summary>
    /// <param name="source">The source end point.</param>
    /// <param name="timeout">The timeout value in milliseconds; by default, 5000ms.</param>
    /// <returns>The elapsed time in milliseconds, or -1 if a timeout has been reached.</returns>
    public int Receive(ref EndPoint source, int timeout = 5000)
    {
        ThrowIfDisposed();

        int waited = 0;
        while (_rxBuffer.Count < 1 && waited < timeout)
        {
            TimerManager.Wait(10);
            waited += 10;
        }

        if (_rxBuffer.Count < 1)
        {
            return -1;
        }

        IcmpEchoReply packet = new(_rxBuffer.Dequeue().RawData);
        source.Address = packet.SourceIP;

        return waited;
    }

    /// <summary>
    /// Receives data from the given packet.
    /// </summary>
    /// <param name="packet">The packet to receive.</param>
    internal void ReceiveData(IcmpPacket packet)
    {
        _rxBuffer.Enqueue(packet);
    }

    /// <summary>
    /// Closes the client and retires it. Unlike <see cref="Close"/>, which a
    /// client reopens by calling <see cref="Connect"/> again, disposal is
    /// final: every other member throws
    /// <see cref="ObjectDisposedException"/> afterwards.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Close();
    }
}
