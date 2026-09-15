using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Timer;

namespace Cosmos.Kernel.System.Network.UDP;

/// <summary>
/// Used to manage the UDP connection to a client.
/// </summary>
public class UdpClient : IDisposable
{
    private const ushort DynamicPortStart = 49152;

    private static ushort s_nextPort = 49152;

    /// <summary>
    /// Gets a dynamic port (simple incrementing approach for AOT compatibility).
    /// </summary>
    /// <param name="tries">How many consecutive ports to try before giving up.</param>
    /// <returns>A port no live client is bound to, or 0 when
    /// <paramref name="tries"/> consecutive candidates were all taken. Zero is
    /// not a usable port, but it is also what an unbound client reports, so a
    /// caller that keeps the value must not later read it back as a binding.</returns>
    public static ushort GetDynamicPort(int tries = 10)
    {
        for (int i = 0; i < tries; i++)
        {
            ushort port = s_nextPort++;
            if (s_nextPort >= 65535)
            {
                s_nextPort = DynamicPortStart;
            }
            if (!s_clients.ContainsKey(port))
            {
                return port;
            }
        }

        return 0;
    }

    private static readonly Dictionary<uint, UdpClient> s_clients = [];
    private readonly int _localPort;
    private int _destinationPort;

    /// <summary>
    /// Destination address.
    /// </summary>
    internal Address? _destination;

    /// <summary>
    /// The RX buffer queue.
    /// </summary>
    internal Queue<UdpPacket> _rxBuffer;
    private bool _disposed;

    /// <summary>
    /// Throws once <see cref="Dispose"/> has run. <see cref="Close"/> does not
    /// arm this: closing only stops delivery to this client, and the DHCP flow
    /// closes itself mid-exchange and keeps going.
    /// </summary>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// Gets a UDP client running on the given port.
    /// </summary>
    /// <param name="destPort">Destination port.</param>
    /// <returns>If a client is running on the given port, the <see cref="UdpClient"/>; otherwise, <see langword="null"/>.</returns>
    internal static UdpClient? GetClient(ushort destPort)
    {
        if (s_clients.TryGetValue(destPort, out var client))
        {
            return client;
        }
        return null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpClient"/> class.
    /// </summary>
    public UdpClient()
        : this(0)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpClient"/> class.
    /// </summary>
    /// <param name="localPort">Local port.</param>
    public UdpClient(int localPort)
    {
        _rxBuffer = new Queue<UdpPacket>(8);

        _localPort = localPort;
        if (localPort > 0)
        {
            s_clients[(uint)localPort] = this;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpClient"/> class.
    /// </summary>
    /// <param name="dest">Destination address.</param>
    /// <param name="destPort">Destination port.</param>
    public UdpClient(Address dest, int destPort)
        : this(0)
    {
        _destination = dest;
        _destinationPort = destPort;
    }

    /// <summary>
    /// Connects to the given client.
    /// </summary>
    /// <param name="dest">Destination address.</param>
    /// <param name="destPort">Destination port.</param>
    public void Connect(Address dest, int destPort)
    {
        ThrowIfDisposed();

        _destination = dest;
        _destinationPort = destPort;
    }

    /// <summary>
    /// Closes the active connection.
    /// </summary>
    public void Close()
    {
        s_clients.Remove((uint)_localPort);
    }

    /// <summary>
    /// Sends data to the client.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    /// <exception cref="InvalidOperationException">No default remote host has
    /// been set, or no configured interface can reach it.</exception>
    public void Send(byte[] data)
    {
        ThrowIfDisposed();

        if (_destination is null || _destinationPort == 0)
        {
            throw new InvalidOperationException("Call Connect before using the Send overload that takes only the data.");
        }

        Send(data, _destination, _destinationPort);
        NetworkStack.Update();
    }

    /// <summary>
    /// Sends data to a remote host.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="dest">Destination address.</param>
    /// <param name="destPort">Destination port.</param>
    /// <exception cref="ObjectDisposedException">The client has been disposed.</exception>
    /// <exception cref="InvalidOperationException">No configured interface can
    /// reach <paramref name="dest"/>.</exception>
    public void Send(byte[] data, Address dest, int destPort)
    {
        ThrowIfDisposed();

        Serial.WriteString("[UdpClient] Send to ");
        Serial.WriteString(dest.ToString());
        Serial.WriteString(":");
        Serial.WriteNumber((ulong)destPort);
        Serial.WriteString(" _localPort=");
        Serial.WriteNumber((ulong)_localPort);
        Serial.WriteString("\n");

        Address? source = IPConfig.FindNetwork(dest);
        if (source is null)
        {
            Serial.WriteString("[UdpClient] ERROR: IPConfig.FindNetwork returned null!\n");
            throw new InvalidOperationException("No configured interface can reach the destination address.");
        }

        Serial.WriteString("[UdpClient] Source IP: ");
        Serial.WriteString(source.ToString());
        Serial.WriteString("\n");

        UdpPacket packet = new(source, dest, (ushort)_localPort, (ushort)destPort, data);
        Serial.WriteString("[UdpClient] UdpPacket created, adding to outgoing buffer\n");
        packet.Network.Enqueue();
        Serial.WriteString("[UdpClient] Packet added to outgoing buffer\n");
    }

    /// <summary>
    /// Transmits a prebuilt UDP packet through the stack's outgoing queue, including ARP
    /// resolution of the destination MAC address when it is not already known.
    /// </summary>
    /// <param name="packet">The packet to transmit.</param>
    /// <returns><see langword="true"/> when the packet was queued for transmission;
    /// <see langword="false"/> when no configured network interface matches the packet's
    /// source address.</returns>
    [Experimental(Experimentals.PacketSeamDiagId)]
    public bool Send(UdpPacket packet)
    {
        ThrowIfDisposed();

        return NetworkStack.Send(packet.Network);
    }

    /// <summary>
    /// Receives a datagram, waiting up to <paramref name="timeoutMs"/> for one
    /// to arrive.
    /// </summary>
    /// <param name="source">Carries the sender's end point back when a datagram arrives.</param>
    /// <param name="timeoutMs">How long to wait, in milliseconds; 0 polls and returns at once.</param>
    /// <returns>The datagram payload, or <see langword="null"/> when none arrived in time.</returns>
    public byte[]? Receive(ref EndPoint source, int timeoutMs = 5000)
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

        UdpPacket packet = _rxBuffer.Dequeue();
        source.Address = packet.SourceIP;
        source.Port = packet.SourcePort;

        return packet.GetUdpData();
    }

    /// <summary>
    /// Waits for a datagram to arrive on this client's local port and returns the parsed
    /// <see cref="UdpPacket"/> itself, without re-parsing. Unlike
    /// <see cref="Receive"/>, which returns the payload
    /// bytes alone, the returned packet exposes the ports, the source and destination
    /// addresses, and the payload. Polls the receive buffer in 10 millisecond slices.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds; a non-positive value
    /// checks the receive buffer once without waiting.</param>
    /// <returns>The dequeued packet, or <see langword="null"/> when the timeout elapses with
    /// no datagram queued.</returns>
    [Experimental(Experimentals.PacketSeamDiagId)]
    public UdpPacket? ReceivePacket(int timeoutMs = 5000)
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
    /// Receives data from the given packet.
    /// </summary>
    /// <param name="packet">Packet to receive.</param>
    internal void ReceiveData(UdpPacket packet)
    {
        _rxBuffer.Enqueue(packet);
    }

    /// <summary>
    /// Closes the client and retires it. Unlike <see cref="Close"/>, which a
    /// client reopens by connecting again, disposal is final: every other
    /// member throws <see cref="ObjectDisposedException"/> afterwards.
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
