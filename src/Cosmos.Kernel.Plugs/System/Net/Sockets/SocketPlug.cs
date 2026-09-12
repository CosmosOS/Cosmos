using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Diagnostics;
using Cosmos.Kernel.System.Network;
using Cosmos.Kernel.System.Network.Config;
using Cosmos.Kernel.System.Network.IPv4;
using Cosmos.Kernel.System.Network.IPv4.TCP;
using KernelEndPoint = Cosmos.Kernel.System.Network.IPv4.EndPoint;
using KernelUdpClient = Cosmos.Kernel.System.Network.IPv4.UDP.UdpClient;

namespace Cosmos.Kernel.Plugs.System.Net.Sockets;

[Plug(typeof(Socket))]
public static class SocketPlug
{
    // Receive timeout for the UDP socket paths. Zero because SO_RCVTIMEO is not
    // plumbed through yet: these poll once and leave the waiting to the caller,
    // which is what the counter spin they replaced effectively did, without
    // burning the cycles.
    private const int UdpPollTimeoutMs = 0;

    // Store protocol type per socket (public for cross-assembly access when patched)
    public static readonly Dictionary<int, ProtocolType> _protocolTypes = new();
    // Store TCP state machine per socket instance
    internal static readonly Dictionary<int, Tcp> s_tcpStateMachines = new();
    // Store UDP client per socket instance
    public static readonly Dictionary<int, KernelUdpClient> _udpClients = new();
    // Store bound endpoint per socket instance
    public static readonly Dictionary<int, IPEndPoint> _endpoints = new();
    // Store local endpoint per socket instance
    public static readonly Dictionary<int, IPEndPoint> _localEndPoints = new();
    // Store remote endpoint per socket instance
    public static readonly Dictionary<int, IPEndPoint> _remoteEndPoints = new();

    // Use object memory address as unique ID (RuntimeHelpers.GetHashCode not available in bare metal)
    public static unsafe int GetId(Socket aThis) => (int)*(nint*)Unsafe.AsPointer(ref aThis);

    [PlugMember(".ctor")]
    public static void Ctor(Socket aThis, SocketType socketType, ProtocolType protocolType)
    {
        CheckSocket(aThis, socketType, protocolType);
    }

    [PlugMember(".ctor")]
    public static void Ctor(Socket aThis, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
    {
        CheckSocket(aThis, socketType, protocolType);
    }

    public static void CheckSocket(Socket aThis, SocketType socketType, ProtocolType protocolType)
    {
        Log.WriteString("[SocketPlug] CheckSocket called\n");
        int id = GetId(aThis);
        Log.WriteString("[SocketPlug] GetId returned: ");
        Log.WriteNumber(id);
        Log.WriteString("\n");

        if (protocolType == ProtocolType.Udp)
        {
            if (socketType != SocketType.Dgram)
            {
                Log.WriteString("[SocketPlug] UDP requires Dgram socket type.\n");
                throw new NotSupportedException("UDP requires Dgram socket type.");
            }
            _protocolTypes[id] = ProtocolType.Udp;
            Log.WriteString("[SocketPlug] Created UDP socket\n");
        }
        else if (protocolType == ProtocolType.Tcp)
        {
            if (socketType != SocketType.Stream)
            {
                Log.WriteString("[SocketPlug] TCP requires Stream socket type.\n");
                throw new NotSupportedException("TCP requires Stream socket type.");
            }
            _protocolTypes[id] = ProtocolType.Tcp;
            Log.WriteString("[SocketPlug] Created TCP socket\n");
        }
        else
        {
            Log.WriteString("[SocketPlug] Only TCP and UDP sockets supported.\n");
            throw new NotImplementedException("Only TCP and UDP sockets supported.");
        }
    }

    [PlugMember("get_Connected")]
    public static bool get_Connected(Socket aThis)
    {
        int id = GetId(aThis);
        if (_protocolTypes.TryGetValue(id, out var proto))
        {
            if (proto == ProtocolType.Tcp)
            {
                if (s_tcpStateMachines.TryGetValue(id, out var sm))
                {
                    return sm.Status == Status.ESTABLISHED;
                }
            }
            else if (proto == ProtocolType.Udp)
            {
                // UDP is connectionless, return true if we have a destination
                return _remoteEndPoints.ContainsKey(id);
            }
        }
        return false;
    }

    [PlugMember("get_Available")]
    public static int get_Available(Socket aThis)
    {
        int id = GetId(aThis);
        if (!_protocolTypes.TryGetValue(id, out var proto))
        {
            return 0;
        }

        if (proto == ProtocolType.Tcp)
        {
            if (s_tcpStateMachines.TryGetValue(id, out var sm))
            {
                return sm.Data.Length;
            }
        }
        else if (proto == ProtocolType.Udp)
        {
            if (_udpClients.TryGetValue(id, out var client))
            {
                // Return approximate bytes available (count of packets in buffer)
                return client._rxBuffer.Count > 0 ? client._rxBuffer.Count : 0;
            }
        }

        return 0;
    }

    [PlugMember("get_LocalEndPoint")]
    public static global::System.Net.EndPoint? get_LocalEndPoint(Socket aThis)
    {
        int id = GetId(aThis);
        if (_localEndPoints.TryGetValue(id, out var ep))
        {
            return ep;
        }

        return null;
    }

    [PlugMember("get_RemoteEndPoint")]
    public static global::System.Net.EndPoint? get_RemoteEndPoint(Socket aThis)
    {
        int id = GetId(aThis);
        if (_remoteEndPoints.TryGetValue(id, out var ep))
        {
            return ep;
        }

        return null;
    }

    [PlugMember]
    public static bool Poll(Socket aThis, int microSeconds, SelectMode mode)
    {
        int id = GetId(aThis);
        if (_protocolTypes.TryGetValue(id, out var proto))
        {
            if (proto == ProtocolType.Tcp)
            {
                if (s_tcpStateMachines.TryGetValue(id, out var sm))
                {
                    return sm.Status == Status.ESTABLISHED;
                }
            }
            else if (proto == ProtocolType.Udp)
            {
                if (_udpClients.TryGetValue(id, out var client))
                {
                    return client._rxBuffer.Count > 0;
                }
            }
        }
        return false;
    }

    [PlugMember]
    public static void Bind(Socket aThis, global::System.Net.EndPoint localEP)
    {
        int id = GetId(aThis);
        var ipep = (IPEndPoint)localEP;
        _endpoints[id] = ipep;
        _localEndPoints[id] = ipep;

        if (_protocolTypes.TryGetValue(id, out var proto) && proto == ProtocolType.Udp)
        {
            // Create UDP client with the bound port
            var client = new KernelUdpClient(ipep.Port);
            _udpClients[id] = client;
            Log.WriteString("[SocketPlug] UDP socket bound to port ");
            Log.WriteNumber((ulong)ipep.Port);
            Log.WriteString("\n");
        }
    }

    [PlugMember]
    public static void Listen(Socket aThis, int backlog)
    {
        int id = GetId(aThis);
        if (_protocolTypes.TryGetValue(id, out var proto) && proto == ProtocolType.Tcp)
        {
            StartTcp(aThis);
        }
    }

    public static void StartTcp(Socket aThis)
    {
        int id = GetId(aThis);
        if (!_endpoints.TryGetValue(id, out var ep))
        {
            Log.WriteString("[SocketPlug] Socket not bound\n");
            throw new InvalidOperationException("Socket not bound");
        }

        var sm = Tcp.CreateConnection((ushort)ep.Port, 0, Address.Zero, Address.Zero);
        sm.LocalEndPoint.Port = (ushort)ep.Port;
        sm.Status = Status.LISTEN;

        s_tcpStateMachines[id] = sm;
    }

    [PlugMember]
    public static Socket Accept(Socket aThis)
    {
        int id = GetId(aThis);

        if (!s_tcpStateMachines.TryGetValue(id, out var sm))
        {
            Log.WriteString("[SocketPlug] TcpListener not started, starting...\n");
            StartTcp(aThis);
            sm = s_tcpStateMachines[id];
        }

        if (sm.Status == Status.CLOSED)
        {
            Tcp.RemoveConnection(sm.LocalEndPoint.Port, sm.RemoteEndPoint.Port, sm.LocalEndPoint.Address, sm.RemoteEndPoint.Address);
            StartTcp(aThis);
            sm = s_tcpStateMachines[id];
        }

        while (sm.WaitStatus(Status.ESTABLISHED) != true)
        {
            ;
        }

        _remoteEndPoints[id] = new IPEndPoint(new IPAddress(sm.RemoteEndPoint.Address.ToSpan()), sm.RemoteEndPoint.Port);
        _localEndPoints[id] = new IPEndPoint(new IPAddress(sm.LocalEndPoint.Address.ToSpan()), sm.LocalEndPoint.Port);

        return aThis;
    }

    [PlugMember]
    public static void Connect(Socket aThis, IPAddress address, int port)
    {
        int id = GetId(aThis);
        if (!_protocolTypes.TryGetValue(id, out var proto))
        {
            throw new InvalidOperationException("Socket not initialized");
        }

        if (proto == ProtocolType.Udp)
        {
            ConnectUdp(aThis, address, port);
        }
        else
        {
            ConnectTcp(aThis, address, port);
        }
    }

    public static void ConnectUdp(Socket aThis, IPAddress address, int port)
    {
        int id = GetId(aThis);

        // Create UDP client if not exists
        if (!_udpClients.TryGetValue(id, out var client))
        {
            int localPort = KernelUdpClient.GetDynamicPort();
            client = new KernelUdpClient(localPort);
            _udpClients[id] = client;
            _localEndPoints[id] = new IPEndPoint(IPAddress.Any, localPort);
        }

        // Use GetAddressBytes directly to avoid string parsing (byte.Parse can trigger resource loading)
        byte[] destBytes = address.GetAddressBytes();
        var destAddr = new Address(destBytes[0], destBytes[1], destBytes[2], destBytes[3]);
        client.Connect(destAddr, port);

        _remoteEndPoints[id] = new IPEndPoint(address, port);
        Log.WriteString("[SocketPlug] UDP connected to ");
        Log.WriteString(destAddr.ToString());
        Log.WriteString(":");
        Log.WriteNumber((ulong)port);
        Log.WriteString("\n");
    }

    public static void ConnectTcp(Socket aThis, IPAddress address, int port)
    {
        int id = GetId(aThis);

        // Create endpoint if not bound
        if (!_endpoints.ContainsKey(id))
        {
            _endpoints[id] = new IPEndPoint(address, port);
        }

        StartTcp(aThis);
        var sm = s_tcpStateMachines[id];

        if (sm.Status == Status.ESTABLISHED)
        {
            Log.WriteString("[SocketPlug] Client must be closed before setting a new connection.\n");
            throw new Exception("Client must be closed before setting a new connection.");
        }

        // Use GetAddressBytes directly to avoid string parsing (byte.Parse can trigger resource loading)
        byte[] remoteBytes = address.GetAddressBytes();
        sm.RemoteEndPoint.Address = new Address(remoteBytes[0], remoteBytes[1], remoteBytes[2], remoteBytes[3]);
        sm.RemoteEndPoint.Port = (ushort)port;
        sm.LocalEndPoint.Address = NetworkManager.Primary.IPConfig?.Address ?? throw new Exception("No IPv4 configuration on the primary network device");
        sm.LocalEndPoint.Port = Tcp.GetDynamicPort();

        _remoteEndPoints[id] = new IPEndPoint(address, sm.RemoteEndPoint.Port);
        _localEndPoints[id] = new IPEndPoint(new IPAddress(sm.LocalEndPoint.Address.ToSpan()), sm.LocalEndPoint.Port);

        // Simple sequence number generation
        uint sequenceNumber = (uint)(1000 + id);

        // Fill TCB
        sm.TCB.SndUna = sequenceNumber;
        sm.TCB.SndNxt = sequenceNumber;
        sm.TCB.SndWnd = Tcp.TcpWindowSize;
        sm.TCB.SndUp = 0;
        sm.TCB.SndWl1 = 0;
        sm.TCB.SndWl2 = 0;
        sm.TCB.ISS = sequenceNumber;

        sm.TCB.RcvNxt = 0;
        sm.TCB.RcvWnd = Tcp.TcpWindowSize;
        sm.TCB.RcvUp = 0;
        sm.TCB.IRS = 0;

        // Set status BEFORE sending packet to avoid race condition
        // (SendEmptyPacket calls NetworkStack.Update which can process incoming packets)
        sm.Status = Status.SYN_SENT;
        sm.SendEmptyPacket(TcpFlags.SYN);

        if (sm.WaitStatus(Status.ESTABLISHED, 5000) == false)
        {
            throw new Exception("Failed to open TCP connection!");
        }
    }

    [PlugMember]
    public static void Connect(Socket aThis, global::System.Net.EndPoint remoteEP)
    {
        if (remoteEP is IPEndPoint ipep)
        {
            Connect(aThis, ipep.Address, ipep.Port);
        }
        else
        {
            throw new NotSupportedException("Only IPEndPoint supported");
        }
    }

    [PlugMember]
    public static int Send(Socket aThis, ReadOnlySpan<byte> buffer, SocketFlags socketFlags)
    {
        return Send(aThis, buffer.ToArray(), 0, buffer.Length, socketFlags);
    }

    [PlugMember]
    public static int Send(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags)
    {
        int id = GetId(aThis);
        if (!_protocolTypes.TryGetValue(id, out var proto))
        {
            throw new InvalidOperationException("Socket not initialized");
        }

        if (proto == ProtocolType.Udp)
        {
            return SendUdp(aThis, buffer, offset, size);
        }
        else
        {
            return SendTcp(aThis, buffer, offset, size);
        }
    }

    public static int SendUdp(Socket aThis, byte[] buffer, int offset, int size)
    {
        int id = GetId(aThis);
        if (!_udpClients.TryGetValue(id, out var client))
        {
            throw new InvalidOperationException("UDP socket not connected");
        }

        if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid offset or size");
        }

        byte[] data = new byte[size];
        Buffer.BlockCopy(buffer, offset, data, 0, size);

        client.Send(data);
        return size;
    }

    public static int SendTcp(Socket aThis, byte[] buffer, int offset, int size)
    {
        Log.WriteString("[SocketPlug] SendTcp: entering\n");
        int id = GetId(aThis);
        if (!s_tcpStateMachines.TryGetValue(id, out var sm))
        {
            Log.WriteString("[SocketPlug] Must establish a connection before sending data.\n");
            throw new InvalidOperationException("Must establish a connection before sending data.");
        }

        if (sm.RemoteEndPoint.Address == null || sm.RemoteEndPoint.Port == 0)
        {
            Log.WriteString("[SocketPlug] Must establish a default remote host by calling Connect().\n");
            throw new InvalidOperationException("Must establish a default remote host by calling Connect() before using this Send() overload");
        }
        if (sm.Status != Status.ESTABLISHED)
        {
            Log.WriteString("[SocketPlug] Client must be connected before sending data.\n");
            throw new Exception("Client must be connected before sending data.");
        }

        if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
        {
            Log.WriteString("[SocketPlug] Invalid offset or size\n");
            throw new ArgumentOutOfRangeException("Invalid offset or size");
        }

        int bytesSent = 0;

        if (size > 536)
        {
            byte[] data = new byte[size];
            Buffer.BlockCopy(buffer, offset, data, 0, size);

            byte[][] chunks = ArraySplit(data, 536);

            for (int i = 0; i < chunks.Length; i++)
            {
                var packet = new TcpPacket(sm.LocalEndPoint.Address, sm.RemoteEndPoint.Address, sm.LocalEndPoint.Port, sm.RemoteEndPoint.Port, sm.TCB.SndNxt, sm.TCB.RcvNxt, 20, i == chunks.Length - 1 ? (byte)(TcpFlags.PSH | TcpFlags.ACK) : (byte)TcpFlags.ACK, sm.TCB.SndWnd, 0, chunks[i]);
                OutgoingBuffer.AddPacket(packet);

                // Increment SndNxt BEFORE NetworkStack.Update() so incoming packets see the correct value
                sm.TCB.SndNxt += (uint)chunks[i].Length;
                bytesSent += chunks[i].Length;

                NetworkStack.Update();

                WaitAck(sm);
            }

            bytesSent = size;
        }
        else
        {
            Log.WriteString("[SocketPlug] SendTcp: preparing packet\n");
            byte[] data = new byte[size];
            Buffer.BlockCopy(buffer, offset, data, 0, size);

            var packet = new TcpPacket(sm.LocalEndPoint.Address, sm.RemoteEndPoint.Address, sm.LocalEndPoint.Port, sm.RemoteEndPoint.Port, sm.TCB.SndNxt, sm.TCB.RcvNxt, 20, (byte)(TcpFlags.PSH | TcpFlags.ACK), sm.TCB.SndWnd, 0, data);
            Log.WriteString("[SocketPlug] SendTcp: adding to outgoing buffer\n");
            OutgoingBuffer.AddPacket(packet);

            // Increment SndNxt BEFORE NetworkStack.Update() so incoming packets see the correct value
            sm.TCB.SndNxt += (uint)size;
            bytesSent = size;

            Log.WriteString("[SocketPlug] SendTcp: calling NetworkStack.Update\n");
            NetworkStack.Update();
            Log.WriteString("[SocketPlug] SendTcp: NetworkStack.Update returned\n");

            // Check if connection was closed during Update (e.g., by FIN from server)
            if (sm.Status == Status.CLOSED || sm.Status == Status.TIME_WAIT)
            {
                Log.WriteString("[SocketPlug] SendTcp: connection closed during send, returning early\n");
                return bytesSent;
            }

            Log.WriteString("[SocketPlug] SendTcp: calling WaitAck\n");
            WaitAck(sm);
            Log.WriteString("[SocketPlug] SendTcp: WaitAck returned, status=");
            Log.WriteNumber((ulong)sm.Status);
            Log.WriteString("\n");
        }

        Log.WriteString("[SocketPlug] SendTcp: returning bytesSent=");
        Log.WriteNumber((ulong)bytesSent);
        Log.WriteString("\n");
        return bytesSent;
    }

    internal static void WaitAck(Tcp sm)
    {
        bool ackReceived = false;
        uint expectedAckNumber = sm.TCB.SndNxt;
        int timeout = 0;

        while (!ackReceived && timeout < 100000)
        {
            if (sm.TCB.SndUna >= expectedAckNumber)
            {
                ackReceived = true;
            }
            timeout++;
        }
    }

    [PlugMember]
    public static int SendTo(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags, global::System.Net.EndPoint remoteEP)
    {
        int id = GetId(aThis);
        if (!_protocolTypes.TryGetValue(id, out var proto) || proto != ProtocolType.Udp)
        {
            throw new InvalidOperationException("SendTo only supported for UDP sockets");
        }

        if (!_udpClients.TryGetValue(id, out var client))
        {
            // Create a UDP client if not exists
            int localPort = KernelUdpClient.GetDynamicPort();
            client = new KernelUdpClient(localPort);
            _udpClients[id] = client;
            _localEndPoints[id] = new IPEndPoint(IPAddress.Any, localPort);
        }

        if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid offset or size");
        }

        if (remoteEP is not IPEndPoint ipep)
        {
            throw new NotSupportedException("Only IPEndPoint supported");
        }

        byte[] data = new byte[size];
        Buffer.BlockCopy(buffer, offset, data, 0, size);

        // Use GetAddressBytes directly to avoid string parsing (byte.Parse can trigger resource loading)
        byte[] addrBytes = ipep.Address.GetAddressBytes();
        var destAddr = new Address(addrBytes[0], addrBytes[1], addrBytes[2], addrBytes[3]);

        Log.WriteString("[SocketPlug] SendTo destAddr=");
        Log.WriteNumber(addrBytes[0]); Log.WriteString(".");
        Log.WriteNumber(addrBytes[1]); Log.WriteString(".");
        Log.WriteNumber(addrBytes[2]); Log.WriteString(".");
        Log.WriteNumber(addrBytes[3]); Log.WriteString(" port=");
        Log.WriteNumber(ipep.Port);
        Log.WriteString("\n");

        client.Send(data, destAddr, ipep.Port);
        NetworkStack.Update();

        return size;
    }

    [PlugMember]
    public static int Receive(Socket aThis, Span<byte> buffer, SocketFlags socketFlags)
    {
        return Receive(aThis, buffer.ToArray(), 0, buffer.Length, socketFlags);
    }

    [PlugMember]
    public static int Receive(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags)
    {
        int id = GetId(aThis);
        if (!_protocolTypes.TryGetValue(id, out var proto))
        {
            throw new InvalidOperationException("Socket not initialized");
        }

        if (proto == ProtocolType.Udp)
        {
            return ReceiveUdp(aThis, buffer, offset, size);
        }
        else
        {
            return ReceiveTcp(aThis, buffer, offset, size);
        }
    }

    public static int ReceiveUdp(Socket aThis, byte[] buffer, int offset, int size)
    {
        int id = GetId(aThis);
        if (!_udpClients.TryGetValue(id, out var client))
        {
            throw new InvalidOperationException("UDP socket not initialized");
        }

        if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid offset or size");
        }

        KernelEndPoint ep = new(Address.Zero, 0);
        byte[]? data = client.Receive(ref ep, UdpPollTimeoutMs);

        if (data == null)
        {
            return 0;
        }

        int bytesToCopy = Math.Min(data.Length, size);
        Buffer.BlockCopy(data, 0, buffer, offset, bytesToCopy);

        return bytesToCopy;
    }

    public static int ReceiveTcp(Socket aThis, byte[] buffer, int offset, int size)
    {
        int id = GetId(aThis);
        if (!s_tcpStateMachines.TryGetValue(id, out var sm))
        {
            Log.WriteString("[SocketPlug] Must establish a connection before receiving data.\n");
            throw new InvalidOperationException("Must establish a connection before receiving data.");
        }

        if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
        {
            Log.WriteString("[SocketPlug] Receive Invalid offset or size\n");
            throw new ArgumentOutOfRangeException("Invalid offset or size");
        }

        // If data is already available, return it immediately (even if connection closed)
        if (sm.Data.Length > 0)
        {
            int bytesToCopy = Math.Min(sm.Data.Length, size);
            var target = buffer.AsSpan(offset, bytesToCopy);
            sm.Data.Slice(0, bytesToCopy).CopyTo(target);

            sm.AdvanceDataOffset(bytesToCopy);

            return bytesToCopy;
        }

        // Wait for data only if connection is still active
        int timeout = 0;
        while (sm.Data.Length == 0 && timeout < 100000)
        {
            // Allow reading data in ESTABLISHED, CLOSE_WAIT, or FIN_WAIT states
            if (sm.Status != Status.ESTABLISHED &&
                sm.Status != Status.CLOSE_WAIT &&
                sm.Status != Status.FIN_WAIT1 &&
                sm.Status != Status.FIN_WAIT2)
            {
                break;
            }
            timeout++;
        }

        if (sm.Data.Length == 0)
        {
            return 0;
        }

        int bytes = Math.Min(sm.Data.Length, size);
        var finalTarget = buffer.AsSpan(offset, bytes);
        sm.Data.Slice(0, bytes).CopyTo(finalTarget);

        sm.AdvanceDataOffset(bytes);

        return bytes;
    }

    [PlugMember]
    public static int ReceiveFrom(Socket aThis, byte[] buffer, int offset, int size, SocketFlags socketFlags, ref global::System.Net.EndPoint remoteEP)
    {
        int id = GetId(aThis);
        if (!_protocolTypes.TryGetValue(id, out var proto) || proto != ProtocolType.Udp)
        {
            throw new InvalidOperationException("ReceiveFrom only supported for UDP sockets");
        }

        if (!_udpClients.TryGetValue(id, out var client))
        {
            throw new InvalidOperationException("UDP socket not initialized");
        }

        if (offset < 0 || size < 0 || (offset + size) > buffer.Length)
        {
            throw new ArgumentOutOfRangeException("Invalid offset or size");
        }

        KernelEndPoint ep = new(Address.Zero, 0);
        byte[]? data = client.Receive(ref ep, UdpPollTimeoutMs);

        if (data == null)
        {
            return 0;
        }

        // Update the remote endpoint (use byte array to avoid endianness issues)
        remoteEP = new IPEndPoint(new IPAddress(ep.Address.ToSpan()), ep.Port);

        int bytesToCopy = Math.Min(data.Length, size);
        Buffer.BlockCopy(data, 0, buffer, offset, bytesToCopy);

        return bytesToCopy;
    }

    [PlugMember]
    public static void Close(Socket aThis)
    {
        Close(aThis, 5000);
    }

    [PlugMember]
    public static void Close(Socket aThis, int timeout)
    {
        int id = GetId(aThis);

        if (_protocolTypes.TryGetValue(id, out var proto))
        {
            if (proto == ProtocolType.Udp)
            {
                CloseUdp(aThis);
            }
            else
            {
                CloseTcp(aThis, timeout);
            }
            _protocolTypes.Remove(id);
        }
    }

    public static void CloseUdp(Socket aThis)
    {
        int id = GetId(aThis);
        if (_udpClients.TryGetValue(id, out var client))
        {
            client.Close();
            _udpClients.Remove(id);
        }
        _endpoints.Remove(id);
        _localEndPoints.Remove(id);
        _remoteEndPoints.Remove(id);
    }

    public static void CloseTcp(Socket aThis, int timeout)
    {
        Log.WriteString("[SocketPlug] CloseTcp: entering\n");
        int id = GetId(aThis);
        if (!s_tcpStateMachines.TryGetValue(id, out var sm))
        {
            Log.WriteString("[SocketPlug] CloseTcp: no state machine found, returning\n");
            return;
        }

        Log.WriteString("[SocketPlug] CloseTcp: status=");
        Log.WriteNumber((ulong)sm.Status);
        Log.WriteString("\n");

        if (sm.Status == Status.CLOSED)
        {
            Log.WriteString("[SocketPlug] CloseTcp: already closed, cleaning up\n");
            Tcp.RemoveConnection(sm);
            s_tcpStateMachines.Remove(id);
            _endpoints.Remove(id);
            _localEndPoints.Remove(id);
            _remoteEndPoints.Remove(id);
            Log.WriteString("[SocketPlug] CloseTcp: cleanup done\n");
            return;
        }
        else if (sm.Status == Status.CLOSING || sm.Status == Status.CLOSE_WAIT)
        {
            if (sm.WaitStatus(Status.CLOSED, timeout))
            {
                Tcp.RemoveConnection(sm);
            }
            else
            {
                // The final ACK never arrived — detach instead of blocking
                // forever; Tcp reaps the connection once it reaches CLOSED.
                Log.WriteString("[SocketPlug] CloseTcp: passive close pending, detaching connection\n");
                sm.Detached = true;
            }

            s_tcpStateMachines.Remove(id);
            _endpoints.Remove(id);
            _localEndPoints.Remove(id);
            _remoteEndPoints.Remove(id);
            return;
        }

        if (sm.Status == Status.LISTEN)
        {
            Tcp.RemoveConnection(sm);
            s_tcpStateMachines.Remove(id);
        }
        else if (sm.Status == Status.ESTABLISHED)
        {
            // Set status BEFORE sending the FIN — same race as ConnectTcp:
            // SendEmptyPacket pumps NetworkStack.Update, which can process the
            // peer's immediate FIN|ACK inline. Under ESTABLISHED that runs the
            // passive close all the way to CLOSED, and an assignment after the
            // send would overwrite CLOSED with FIN_WAIT1 and hang the close.
            sm.Status = Status.FIN_WAIT1;
            sm.SendEmptyPacket(TcpFlags.FIN | TcpFlags.ACK);

            // Wait for the peer to ACK our FIN. Once it is ACKed the close has
            // succeeded from the caller's point of view — the peer may hold
            // its half of the connection open for as long as it wants
            // (issue #369), so Close() must not block on or throw for the
            // peer's own FIN.
            sm.WaitLeaveStatus(Status.FIN_WAIT1, timeout);

            if (sm.Status == Status.CLOSED)
            {
                Tcp.RemoveConnection(sm);
            }
            else
            {
                // Half-close: detach the state machine so it finishes the
                // handshake in the background; Tcp reaps it on CLOSED.
                Log.WriteString("[SocketPlug] CloseTcp: peer FIN pending, detaching connection\n");
                sm.Detached = true;
            }

            s_tcpStateMachines.Remove(id);
        }

        _endpoints.Remove(id);
        _localEndPoints.Remove(id);
        _remoteEndPoints.Remove(id);
    }

    [PlugMember]
    public static void Dispose(Socket aThis)
    {
        Close(aThis, 5000);
    }

    /// <summary>
    /// Splits an array into chunks of the specified size.
    /// </summary>
    public static byte[][] ArraySplit(byte[] data, int chunkSize)
    {
        int numChunks = (data.Length + chunkSize - 1) / chunkSize;
        byte[][] result = new byte[numChunks][];

        for (int i = 0; i < numChunks; i++)
        {
            int start = i * chunkSize;
            int length = Math.Min(chunkSize, data.Length - start);
            result[i] = new byte[length];
            Buffer.BlockCopy(data, start, result[i], 0, length);
        }

        return result;
    }
}
