// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Timer;

namespace Cosmos.Kernel.System.Network.IPv6;

/// <summary>
/// Sends ICMPv6 echo requests to one destination and receives its replies:
/// the IPv6 counterpart of <see cref="IcmpClient"/>. The destination must be
/// on the link; there is no IPv6 routing yet.
/// </summary>
public sealed class Icmpv6Client : IDisposable
{
    private static readonly Dictionary<Address6, Icmpv6Client> s_clients = [];

    private readonly Queue<Icmpv6EchoReply> _rxBuffer = new(8);
    private Address6? _destination;
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
    /// Gets the client connected to a destination address.
    /// </summary>
    /// <param name="address">The destination address.</param>
    /// <returns>The client connected to that address, or null when there is none.</returns>
    internal static Icmpv6Client? GetClient(Address6 address)
    {
        return s_clients.TryGetValue(address, out Icmpv6Client? client) ? client : null;
    }

    /// <summary>
    /// Connects to the given destination. Replies from it are delivered to
    /// this client until <see cref="Close"/>.
    /// </summary>
    /// <param name="dest">Destination address.</param>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    public void Connect(Address6 dest)
    {
        ThrowIfDisposed();

        // Reconnecting to a second host has to drop the first registration:
        // s_clients is static, so a stale entry both misroutes replies and
        // roots this client for the life of the kernel.
        Close();

        _destination = dest;
        s_clients[dest] = this;
    }

    /// <summary>
    /// Closes the active connection.
    /// </summary>
    public void Close()
    {
        if (_destination is not null)
        {
            s_clients.Remove(_destination);
        }
    }

    /// <summary>
    /// Sends an ICMPv6 echo request to the connected destination from the
    /// primary device's link-local address.
    /// </summary>
    /// <param name="id">The echo identifier.</param>
    /// <param name="sequence">The echo sequence number.</param>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    /// <exception cref="InvalidOperationException">No destination has been set,
    /// or the primary device has no link-local address yet.</exception>
    public void SendEcho(ushort id = 0x0001, ushort sequence = 0x0001)
    {
        ThrowIfDisposed();

        if (_destination is null)
        {
            throw new InvalidOperationException("Call Connect before using SendEcho.");
        }

        INetworkDevice? device = NetworkManager.PrimaryDevice;
        Address6 source = (device is null ? null : NetworkStack.LinkLocalOf(device))
            ?? throw new InvalidOperationException("The primary network device has no link-local IPv6 address; it comes up with the device's IPv4 configuration.");

        Icmpv6EchoRequest request = new(source, _destination, id, sequence);
        OutgoingBuffer.AddPacket(request);
        NetworkStack.Update();
    }

    /// <summary>
    /// Receives an ICMPv6 echo reply from the remote host.
    /// </summary>
    /// <param name="source">Receives the address the reply came from.</param>
    /// <param name="timeout">The timeout in milliseconds; by default, 5000 ms.</param>
    /// <returns>The elapsed time in milliseconds, or -1 when the timeout was reached.</returns>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
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

        Icmpv6EchoReply packet = _rxBuffer.Dequeue();
        source.Address = packet.SourceIP;

        return waited;
    }

    /// <summary>
    /// Queues a reply for <see cref="Receive"/>.
    /// </summary>
    /// <param name="packet">The reply.</param>
    internal void ReceiveData(Icmpv6EchoReply packet)
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
