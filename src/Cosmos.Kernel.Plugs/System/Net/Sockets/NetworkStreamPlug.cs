using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;
using Cosmos.Kernel.System.Diagnostics;

namespace Cosmos.Kernel.Plugs.System.Net.Sockets;

[Plug(typeof(NetworkStream))]
public static class NetworkStreamPlug
{
    // Store stream state per instance
    public static readonly Dictionary<int, Socket> _streamSockets = new();
    public static readonly Dictionary<int, bool> _ownsSocket = new();
    public static readonly Dictionary<int, bool> _readable = new();
    public static readonly Dictionary<int, bool> _writeable = new();

    // Use object memory address as unique ID (RuntimeHelpers.GetHashCode not available in bare metal)
    public static unsafe int GetId(NetworkStream aThis) => (int)*(nint*)Unsafe.AsPointer(ref aThis);

    [PlugMember(".ctor")]
    public static void Ctor(NetworkStream aThis, Socket socket)
    {
        Ctor(aThis, socket, FileAccess.ReadWrite, false);
    }

    [PlugMember(".ctor")]
    public static void Ctor(NetworkStream aThis, Socket socket, bool ownsSocket)
    {
        Ctor(aThis, socket, FileAccess.ReadWrite, ownsSocket);
    }

    [PlugMember(".ctor")]
    public static void Ctor(NetworkStream aThis, Socket socket, FileAccess access)
    {
        Ctor(aThis, socket, access, false);
    }

    [PlugMember(".ctor")]
    public static void Ctor(NetworkStream aThis, Socket socket, FileAccess access, bool ownsSocket)
    {
        Log.WriteString("[NetworkStreamPlug] Ctor(socket, access, ownsSocket)\n");

        if (socket == null)
        {
            Log.WriteString("[NetworkStreamPlug] socket is null\n");
            throw new ArgumentNullException(nameof(socket));
        }

        if (!socket.Connected)
        {
            Log.WriteString("[NetworkStreamPlug] socket is not connected\n");
            throw new IOException("Socket not connected.");
        }

        int id = GetId(aThis);
        _streamSockets[id] = socket;
        _ownsSocket[id] = ownsSocket;

        switch (access)
        {
            case FileAccess.Read:
                _readable[id] = true;
                _writeable[id] = false;
                break;
            case FileAccess.Write:
                _readable[id] = false;
                _writeable[id] = true;
                break;
            case FileAccess.ReadWrite:
            default:
                _readable[id] = true;
                _writeable[id] = true;
                break;
        }
    }

    [PlugMember("get_CanRead")]
    public static bool get_CanRead(NetworkStream aThis)
    {
        int id = GetId(aThis);
        return _readable.TryGetValue(id, out bool readable) && readable;
    }

    [PlugMember("get_CanWrite")]
    public static bool get_CanWrite(NetworkStream aThis)
    {
        int id = GetId(aThis);
        return _writeable.TryGetValue(id, out bool writeable) && writeable;
    }

    [PlugMember("get_CanSeek")]
    public static bool get_CanSeek(NetworkStream aThis)
    {
        return false;
    }

    [PlugMember("get_DataAvailable")]
    public static bool get_DataAvailable(NetworkStream aThis)
    {
        int id = GetId(aThis);
        if (!_streamSockets.TryGetValue(id, out var socket))
        {
            return false;
        }

        return socket.Available > 0;
    }

    [PlugMember("get_Length")]
    public static long get_Length(NetworkStream aThis)
    {
        throw new NotSupportedException("NetworkStream does not support Length");
    }

    [PlugMember("get_Position")]
    public static long get_Position(NetworkStream aThis)
    {
        throw new NotSupportedException("NetworkStream does not support Position");
    }

    [PlugMember("set_Position")]
    public static void set_Position(NetworkStream aThis, long value)
    {
        throw new NotSupportedException("NetworkStream does not support Position");
    }

    [PlugMember("get_Socket")]
    public static Socket? get_Socket(NetworkStream aThis)
    {
        int id = GetId(aThis);
        return _streamSockets.TryGetValue(id, out var socket) ? socket : null;
    }

    [PlugMember]
    public static int Read(NetworkStream aThis, byte[] buffer, int offset, int count)
    {
        int id = GetId(aThis);
        if (!_streamSockets.TryGetValue(id, out var socket))
        {
            throw new ObjectDisposedException(nameof(NetworkStream));
        }

        return socket.Receive(buffer, offset, count, SocketFlags.None);
    }

    [PlugMember]
    public static int ReadByte(NetworkStream aThis)
    {
        int id = GetId(aThis);
        if (!_streamSockets.TryGetValue(id, out var socket))
        {
            throw new ObjectDisposedException(nameof(NetworkStream));
        }

        byte[] buffer = new byte[1];
        int read = socket.Receive(buffer, 0, 1, SocketFlags.None);
        return read == 0 ? -1 : buffer[0];
    }

    [PlugMember]
    public static void Write(NetworkStream aThis, byte[] buffer, int offset, int count)
    {
        Log.WriteString("[NetworkStreamPlug] Write: entering, count=");
        Log.WriteNumber((ulong)count);
        Log.WriteString("\n");

        int id = GetId(aThis);
        if (!_streamSockets.TryGetValue(id, out var socket))
        {
            Log.WriteString("[NetworkStreamPlug] Write: socket disposed\n");
            throw new ObjectDisposedException(nameof(NetworkStream));
        }

        Log.WriteString("[NetworkStreamPlug] Write: calling socket.Send\n");
        socket.Send(buffer, offset, count, SocketFlags.None);
        Log.WriteString("[NetworkStreamPlug] Write: socket.Send returned\n");
    }

    [PlugMember]
    public static void Write(NetworkStream aThis, ReadOnlySpan<byte> buffer)
    {
        Write(aThis, buffer.ToArray(), 0, buffer.Length);
    }

    [PlugMember]
    public static void WriteByte(NetworkStream aThis, byte value)
    {
        Write(aThis, new byte[] { value }, 0, 1);
    }

    [PlugMember]
    public static void Flush(NetworkStream aThis)
    {
        // No-op for network streams
    }

    [PlugMember]
    public static long Seek(NetworkStream aThis, long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("NetworkStream does not support Seek");
    }

    [PlugMember]
    public static void SetLength(NetworkStream aThis, long value)
    {
        throw new NotSupportedException("NetworkStream does not support SetLength");
    }

    [PlugMember]
    public static void Close(NetworkStream aThis)
    {
        Dispose(aThis, true);
    }

    [PlugMember]
    public static void Dispose(NetworkStream aThis)
    {
        Dispose(aThis, true);
    }

    [PlugMember]
    public static void Dispose(NetworkStream aThis, bool disposing)
    {
        int id = GetId(aThis);
        Socket? socket = null;
        bool owns = false;

        if (_streamSockets.TryGetValue(id, out socket))
        {
            _ownsSocket.TryGetValue(id, out owns);
        }

        _streamSockets.Remove(id);
        _ownsSocket.Remove(id);
        _readable.Remove(id);
        _writeable.Remove(id);

        if (socket != null && owns)
        {
            socket.Close();
        }
    }
}
