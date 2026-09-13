using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Diagnostics;

namespace Cosmos.Kernel.Plugs.System.Net.Sockets;

[Plug(typeof(TcpListener))]
public static class TcpListenerPlug
{
    // Store listener state per instance
    public static readonly Dictionary<int, Socket> _serverSockets = new();
    public static readonly Dictionary<int, IPEndPoint> _serverSocketEPs = new();

    // Use object memory address as unique ID
    public static unsafe int GetId(TcpListener aThis) => (int)*(nint*)Unsafe.AsPointer(ref aThis);

    [PlugMember(".ctor")]
    public static void Ctor(TcpListener aThis, IPEndPoint localEP)
    {
        Log.WriteString("[TcpListenerPlug] Ctor(localEP)\n");

        ArgumentNullException.ThrowIfNull(localEP);

        int id = GetId(aThis);
        _serverSocketEPs[id] = localEP;
        _serverSockets[id] = new Socket(localEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    }

    [PlugMember(".ctor")]
    public static void Ctor(TcpListener aThis, IPAddress localaddr, int port)
    {
        Log.WriteString("[TcpListenerPlug] Ctor(localaddr, port)\n");

        if (localaddr is null)
        {
            Log.WriteString("[TcpListenerPlug] localaddr is null!\n");
            throw new ArgumentNullException(nameof(localaddr));
        }

        Log.WriteString("[TcpListenerPlug] Getting ID\n");
        int id = GetId(aThis);
        Log.WriteString("[TcpListenerPlug] ID=");
        Log.WriteNumber(id);
        Log.WriteString("\n");

        Log.WriteString("[TcpListenerPlug] Creating IPEndPoint\n");
        var ep = new IPEndPoint(localaddr, port);
        Log.WriteString("[TcpListenerPlug] IPEndPoint created\n");

        Log.WriteString("[TcpListenerPlug] Storing endpoint\n");
        _serverSocketEPs[id] = ep;

        Log.WriteString("[TcpListenerPlug] Creating socket\n");
        _serverSockets[id] = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Log.WriteString("[TcpListenerPlug] Socket created\n");
    }

    [PlugMember("get_Server")]
    public static Socket? get_Server(TcpListener aThis)
    {
        int id = GetId(aThis);
        return _serverSockets.TryGetValue(id, out var socket) ? socket : null;
    }

    [PlugMember("get_LocalEndpoint")]
    public static EndPoint? get_LocalEndpoint(TcpListener aThis)
    {
        int id = GetId(aThis);
        if (_serverSockets.TryGetValue(id, out var socket) && socket.LocalEndPoint != null)
        {
            return socket.LocalEndPoint;
        }
        return _serverSocketEPs.TryGetValue(id, out var ep) ? ep : null;
    }

    [PlugMember]
    public static void Start(TcpListener aThis)
    {
        Log.WriteString("[TcpListenerPlug] Start()\n");

        int id = GetId(aThis);
        if (!_serverSockets.TryGetValue(id, out var socket) || !_serverSocketEPs.TryGetValue(id, out var ep))
        {
            throw new InvalidOperationException("TcpListener not initialized");
        }

        socket.Bind(ep);
        socket.Listen(int.MaxValue);

        Log.WriteString("[TcpListenerPlug] Listening on port ");
        Log.WriteNumber((ulong)ep.Port);
        Log.WriteString("\n");
    }

    [PlugMember]
    public static void Start(TcpListener aThis, int backlog)
    {
        Log.WriteString("[TcpListenerPlug] Start(backlog)\n");

        int id = GetId(aThis);
        if (!_serverSockets.TryGetValue(id, out var socket) || !_serverSocketEPs.TryGetValue(id, out var ep))
        {
            throw new InvalidOperationException("TcpListener not initialized");
        }

        socket.Bind(ep);
        socket.Listen(backlog);

        Log.WriteString("[TcpListenerPlug] Listening on port ");
        Log.WriteNumber((ulong)ep.Port);
        Log.WriteString("\n");
    }

    [PlugMember]
    public static void Stop(TcpListener aThis)
    {
        Log.WriteString("[TcpListenerPlug] Stop()\n");

        int id = GetId(aThis);
        if (_serverSockets.TryGetValue(id, out var socket))
        {
            socket.Close();
            _serverSockets.Remove(id);
        }
        _serverSocketEPs.Remove(id);
    }

    [PlugMember]
    public static bool Pending(TcpListener aThis)
    {
        int id = GetId(aThis);
        if (!_serverSockets.TryGetValue(id, out var socket))
        {
            throw new InvalidOperationException("TcpListener not started");
        }

        return socket.Poll(0, SelectMode.SelectRead);
    }

    [PlugMember]
    public static Socket AcceptSocket(TcpListener aThis)
    {
        Log.WriteString("[TcpListenerPlug] AcceptSocket()\n");

        int id = GetId(aThis);
        if (!_serverSockets.TryGetValue(id, out var socket))
        {
            throw new InvalidOperationException("TcpListener not started");
        }

        return socket.Accept();
    }

    [PlugMember]
    public static TcpClient AcceptTcpClient(TcpListener aThis)
    {
        Log.WriteString("[TcpListenerPlug] AcceptTcpClient()\n");

        int id = GetId(aThis);
        if (!_serverSockets.TryGetValue(id, out var socket))
        {
            throw new InvalidOperationException("TcpListener not started");
        }

        TcpClient client = new();
        Socket acceptedSocket = socket.Accept();
        client.Client = acceptedSocket;
        return client;
    }
}
