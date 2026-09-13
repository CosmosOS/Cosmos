using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Diagnostics;


namespace Cosmos.Kernel.Plugs.System.Net.Sockets;

[Plug(typeof(UdpClient))]
public static class UdpClientPlug
{
    // Store client socket per instance
    public static readonly Dictionary<int, Socket> _clientSockets = new();
    public static readonly Dictionary<int, bool> _active = new();

    // Use object memory address as unique ID (RuntimeHelpers.GetHashCode not available in bare metal)
    public static unsafe int GetId(UdpClient aThis) => (int)*(nint*)Unsafe.AsPointer(ref aThis);

    // NOTE: UdpClient() and UdpClient(int port) are NOT plugged because they chain to
    // UdpClient(AddressFamily) and UdpClient(int port, AddressFamily) respectively.
    // The terminal constructors handle all the work.

    [PlugMember(".ctor")]
    public static void Ctor(UdpClient aThis, IPEndPoint localEP)
    {
        Log.WriteString("[UdpClientPlug] Ctor(localEP)\n");

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        int id = GetId(aThis);
        _clientSockets[id] = socket;
        _active[id] = false;

        socket.Bind(localEP);
    }

    [PlugMember(".ctor")]
    public static void Ctor(UdpClient aThis, AddressFamily family)
    {
        Log.WriteString("[UdpClientPlug] Ctor(family)\n");

        var socket = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
        int id = GetId(aThis);
        _clientSockets[id] = socket;
        _active[id] = false;
    }

    [PlugMember(".ctor")]
    public static void Ctor(UdpClient aThis, int port, AddressFamily family)
    {
        Log.WriteString("[UdpClientPlug] Ctor(port, family)\n");

        var socket = new Socket(family, SocketType.Dgram, ProtocolType.Udp);
        int id = GetId(aThis);
        _clientSockets[id] = socket;
        _active[id] = false;

        // Bind to the specified port
        var localEP = family == AddressFamily.InterNetwork
            ? new IPEndPoint(IPAddress.Any, port)
            : new IPEndPoint(IPAddress.IPv6Any, port);
        socket.Bind(localEP);
    }

    [PlugMember(".ctor")]
    public static void Ctor(UdpClient aThis, string hostname, int port)
    {
        Log.WriteString("[UdpClientPlug] Ctor(hostname, port)\n");

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        int id = GetId(aThis);
        _clientSockets[id] = socket;
        _active[id] = false;

        // Connect to remote endpoint
        IPAddress address = IPAddress.Parse(hostname);
        socket.Connect(address, port);
        _active[id] = true;
    }

    [PlugMember("get_Client")]
    public static Socket? get_Client(UdpClient aThis)
    {
        int id = GetId(aThis);
        return _clientSockets.TryGetValue(id, out var socket) ? socket : null;
    }

    [PlugMember("set_Client")]
    public static void set_Client(UdpClient aThis, Socket value)
    {
        int id = GetId(aThis);
        _clientSockets[id] = value;
    }

    [PlugMember("get_Available")]
    public static int get_Available(UdpClient aThis)
    {
        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            return 0;
        }

        return socket.Available;
    }

    [PlugMember]
    public static void Connect(UdpClient aThis, string hostname, int port)
    {
        IPAddress address = IPAddress.Parse(hostname);
        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            return;
        }

        socket.Connect(address, port);
        _active[id] = true;
    }

    [PlugMember]
    public static void Connect(UdpClient aThis, IPAddress addr, int port)
    {
        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            return;
        }

        socket.Connect(addr, port);
        _active[id] = true;
    }

    [PlugMember]
    public static void Connect(UdpClient aThis, IPEndPoint endPoint)
    {
        Connect(aThis, endPoint.Address, endPoint.Port);
    }

    [PlugMember]
    public static int Send(UdpClient aThis, byte[] dgram, int bytes)
    {
        Log.WriteString("[UdpClientPlug] Send(dgram, bytes)\n");

        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            throw new InvalidOperationException("UdpClient socket not initialized");
        }
        if (!_active.TryGetValue(id, out bool active) || !active)
        {
            throw new InvalidOperationException("UdpClient not connected - use Send with endpoint");
        }

        return socket.Send(dgram, 0, bytes, SocketFlags.None);
    }

    [PlugMember]
    public static int Send(UdpClient aThis, byte[] dgram, int bytes, IPEndPoint endPoint)
    {
        Log.WriteString("[UdpClientPlug] Send(dgram, bytes, endPoint)\n");

        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            throw new InvalidOperationException("UdpClient socket not initialized");
        }

        return socket.SendTo(dgram, 0, bytes, SocketFlags.None, endPoint);
    }

    [PlugMember]
    public static int Send(UdpClient aThis, byte[] dgram, int bytes, string hostname, int port)
    {
        Log.WriteString("[UdpClientPlug] Send(dgram, bytes, hostname, port)\n");

        IPAddress address = IPAddress.Parse(hostname);
        var endPoint = new IPEndPoint(address, port);

        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            throw new InvalidOperationException("UdpClient socket not initialized");
        }

        return socket.SendTo(dgram, 0, bytes, SocketFlags.None, endPoint);
    }

    [PlugMember]
    public static byte[] Receive(UdpClient aThis, ref IPEndPoint remoteEP)
    {
        Log.WriteString("[UdpClientPlug] Receive()\n");

        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            throw new InvalidOperationException("UdpClient socket not initialized");
        }

        // Create buffer for receiving
        byte[] buffer = new byte[65536];
        EndPoint ep = remoteEP ?? new IPEndPoint(IPAddress.Any, 0);

        int received = socket.ReceiveFrom(buffer, ref ep);

        remoteEP = (IPEndPoint)ep;

        // Return only the received data
        byte[] result = new byte[received];
        for (int i = 0; i < received; i++)
        {
            result[i] = buffer[i];
        }

        return result;
    }

    [PlugMember]
    public static void Close(UdpClient aThis)
    {
        Dispose(aThis, true);
    }

    [PlugMember]
    public static void Dispose(UdpClient aThis)
    {
        Dispose(aThis, true);
    }

    [PlugMember]
    public static void Dispose(UdpClient aThis, bool disposing)
    {
        int id = GetId(aThis);
        if (_clientSockets.TryGetValue(id, out var socket))
        {
            _clientSockets.Remove(id);
            socket.Close();
        }
        _active.Remove(id);
    }
}
