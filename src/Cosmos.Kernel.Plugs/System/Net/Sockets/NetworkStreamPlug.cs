using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Cosmos.Build.API.Attributes;

namespace Cosmos.Kernel.Plugs.System.Net.Sockets;

/// <summary>
/// Plugs what of .NET's <see cref="NetworkStream"/> reaches past what the
/// socket plug keeps: the constructor's checks, the timeouts, which .NET
/// keeps as socket options, and Dispose, which shuts the socket down through
/// its native handle. The stream keeps its state in its own fields, so the
/// rest is .NET's own code: Read and Write go through the socket plug's
/// Receive and Send, which wait as .NET's do.
/// </summary>
[Plug(typeof(NetworkStream))]
public static class NetworkStreamPlug
{
    // The three other constructors chain to this one.
    [PlugMember(".ctor")]
    public static void Ctor(NetworkStream aThis, Socket socket, FileAccess access, bool ownsSocket)
    {
        Initialize(aThis, socket, access, ownsSocket);
    }

    [PlugMember("get_ReadTimeout")]
    public static int get_ReadTimeout(NetworkStream aThis)
    {
        return ToStreamTimeout(aThis.Socket.ReceiveTimeout);
    }

    [PlugMember("set_ReadTimeout")]
    public static void set_ReadTimeout(NetworkStream aThis, int value)
    {
        aThis.Socket.ReceiveTimeout = CheckStreamTimeout(value);
    }

    [PlugMember("get_WriteTimeout")]
    public static int get_WriteTimeout(NetworkStream aThis)
    {
        return ToStreamTimeout(aThis.Socket.SendTimeout);
    }

    [PlugMember("set_WriteTimeout")]
    public static void set_WriteTimeout(NetworkStream aThis, int value)
    {
        aThis.Socket.SendTimeout = CheckStreamTimeout(value);
    }

    [PlugMember]
    public static void Dispose(NetworkStream aThis, bool disposing)
    {
        Release(aThis, disposing);
    }

    public static void Initialize(NetworkStream stream, Socket socket, FileAccess access, bool ownsSocket)
    {
        ArgumentNullException.ThrowIfNull(socket);

        // .NET checks Blocking, Connected and SocketType. The socket plug
        // keeps neither Blocking nor SocketType, and its Connected turns false
        // once the peer has closed its side, where .NET's stays true: a
        // client's request and its FIN can both arrive before Accept returns,
        // and the request is still to be read.
        if (!SocketPlug.HoldsConnection(socket))
        {
            throw new IOException("The operation is not allowed on non-connected sockets.");
        }

        StreamSocket(stream) = socket;
        OwnsSocket(stream) = ownsSocket;
        Readable(stream) = access != FileAccess.Write;
        Writeable(stream) = access != FileAccess.Read;
    }

    public static void Release(NetworkStream stream, bool disposing)
    {
        if (Disposed(stream))
        {
            return;
        }

        Disposed(stream) = true;

        if (disposing)
        {
            Readable(stream) = false;
            Writeable(stream) = false;

            // .NET shuts the socket down before closing it, through a native
            // handle the socket plug doesn't have: its Close sends the FIN.
            if (OwnsSocket(stream))
            {
                stream.Socket.Close(CloseTimeout(stream));
            }
        }
    }

    // A socket timeout of 0 is none, which a stream says with -1.
    public static int ToStreamTimeout(int socketTimeout)
    {
        return socketTimeout == 0 ? Timeout.Infinite : socketTimeout;
    }

    public static int CheckStreamTimeout(int value)
    {
        if (value <= 0 && value != Timeout.Infinite)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Timeout can be only be set to 'System.Threading.Timeout.Infinite' or a value > 0.");
        }

        return value;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_streamSocket")]
    private static extern ref Socket StreamSocket(NetworkStream stream);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_ownsSocket")]
    private static extern ref bool OwnsSocket(NetworkStream stream);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_readable")]
    private static extern ref bool Readable(NetworkStream stream);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_writeable")]
    private static extern ref bool Writeable(NetworkStream stream);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_disposed")]
    private static extern ref bool Disposed(NetworkStream stream);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_closeTimeout")]
    private static extern ref int CloseTimeout(NetworkStream stream);
}
