using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;

namespace Cosmos.Kernel.Plugs.System.Net.Sockets;

[Plug(typeof(TcpClient))]
public static class TcpClientPlug
{
    // Store client socket and data stream per instance
    public static readonly Dictionary<int, Socket> _clientSockets = new();
    public static readonly Dictionary<int, NetworkStream> _dataStreams = new();

    // Use object memory address as unique ID (RuntimeHelpers.GetHashCode not available in bare metal)
    public static unsafe int GetId(TcpClient aThis) => (int)*(nint*)Unsafe.AsPointer(ref aThis);

    [PlugMember(".ctor")]
    public static void Ctor(TcpClient aThis)
    {
        // Default constructor - socket will be created on connect
    }

    [PlugMember(".ctor")]
    public static void Ctor(TcpClient aThis, string hostname, int port)
    {
        Connect(aThis, hostname, port);
    }

    [PlugMember(".ctor")]
    public static void Ctor(TcpClient aThis, AddressFamily family)
    {
        // Create socket with specified address family
        var socket = new Socket(family, SocketType.Stream, ProtocolType.Tcp);
        int id = GetId(aThis);
        _clientSockets[id] = socket;
    }

    [PlugMember("set_Client")]
    public static void set_Client(TcpClient aThis, Socket value)
    {
        int id = GetId(aThis);
        _clientSockets[id] = value;
    }

    [PlugMember("get_Client")]
    public static Socket? get_Client(TcpClient aThis)
    {
        int id = GetId(aThis);
        return _clientSockets.TryGetValue(id, out var socket) ? socket : null;
    }

    [PlugMember("get_Connected")]
    public static bool get_Connected(TcpClient aThis)
    {
        int id = GetId(aThis);
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            return false;
        }

        return socket.Connected;
    }

    [PlugMember("get_ReceiveBufferSize")]
    public static int get_ReceiveBufferSize(TcpClient aThis)
    {
        return Cosmos.Kernel.System.Network.IPv4.TCP.Tcp.TcpWindowSize;
    }

    [PlugMember("set_ReceiveBufferSize")]
    public static void set_ReceiveBufferSize(TcpClient aThis, int value)
    {
        // Not implemented - using fixed window size
    }

    [PlugMember("get_SendBufferSize")]
    public static int get_SendBufferSize(TcpClient aThis)
    {
        return Cosmos.Kernel.System.Network.IPv4.TCP.Tcp.TcpWindowSize;
    }

    [PlugMember("set_SendBufferSize")]
    public static void set_SendBufferSize(TcpClient aThis, int value)
    {
        // Not implemented - using fixed window size
    }

    [PlugMember]
    public static void Connect(TcpClient aThis, string hostname, int port)
    {
        IPAddress address = IPAddress.Parse(hostname);
        int id = GetId(aThis);

        // Create socket if not exists
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSockets[id] = socket;
        }

        socket.Connect(address, port);
    }

    [PlugMember]
    public static void Connect(TcpClient aThis, IPAddress address, int port)
    {
        int id = GetId(aThis);

        // Create socket if not exists
        if (!_clientSockets.TryGetValue(id, out var socket))
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSockets[id] = socket;
        }

        socket.Connect(address, port);
    }

    [PlugMember]
    public static void Connect(TcpClient aThis, IPEndPoint remoteEP)
    {
        Connect(aThis, remoteEP.Address, remoteEP.Port);
    }

    [PlugMember]
    public static NetworkStream GetStream(TcpClient aThis)
    {
        int id = GetId(aThis);

        if (!_clientSockets.TryGetValue(id, out Socket? socket))
        {
            throw new InvalidOperationException("TcpClient is not connected");
        }

        if (_dataStreams.TryGetValue(id, out NetworkStream? stream))
        {
            return stream;
        }

        // Create stream
        stream = new NetworkStream(socket, true);
        _dataStreams[id] = stream;

        return stream;
    }

    [PlugMember]
    public static void Close(TcpClient aThis)
    {
        Dispose(aThis, true);
    }

    [PlugMember]
    public static void Dispose(TcpClient aThis)
    {
        Dispose(aThis, true);
    }

    [PlugMember]
    public static void Dispose(TcpClient aThis, bool disposing)
    {
        int id = GetId(aThis);
        NetworkStream? stream = null;
        Socket? socket = null;

        if (_dataStreams.TryGetValue(id, out stream))
        {
            _dataStreams.Remove(id);
        }
        else if (_clientSockets.TryGetValue(id, out socket))
        {
            // Only get socket if no stream (stream owns socket)
        }
        _clientSockets.Remove(id);

        if (stream is not null)
        {
            stream.Dispose();
        }
        else
        {
            socket?.Close();
        }
    }
}
