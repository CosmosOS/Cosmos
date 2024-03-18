using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(NetworkStream))]
    public static class NetworkStreamImpl
    {
        public static void Ctor(NetworkStream aThis, Socket socket, FileAccess access, bool ownsSocket,
            [FieldAccess(Name = "System.Boolean System.Net.Sockets.NetworkStream._ownsSocket")] ref bool _ownsSocket,
            [FieldAccess(Name = "System.Boolean System.Net.Sockets.NetworkStream._readable")] ref bool _readable,
            [FieldAccess(Name = "System.Boolean System.Net.Sockets.NetworkStream._writeable")] ref bool _writeable,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket
            )
        {
            if (socket == null)
            {
                Cosmos.HAL.Global.debugger.Send("NetworkStream - socket is null.");

                throw new ArgumentNullException(nameof(socket));
            }
            if (!socket.Connected)
            {
                Cosmos.HAL.Global.debugger.Send("NetworkStream - socket is not connected.");

                throw new IOException("Socket not connected.");
            }

            _streamSocket = socket;
            _ownsSocket = ownsSocket;

            switch (access)
            {
                case FileAccess.Read:
                    _readable = true;
                    break;
                case FileAccess.Write:
                    _writeable = true;
                    break;
                case FileAccess.ReadWrite:
                default: // assume FileAccess.ReadWrite
                    _readable = true;
                    _writeable = true;
                    break;
            }
        }

        public static int Read(NetworkStream aThis, byte[] buffer, int offset, int count,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket)
        {
            return _streamSocket.Receive(buffer, offset, count, 0);
        }

        public static void Write(NetworkStream aThis, ReadOnlySpan<byte> buffer,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket)
        {
            Write(aThis, buffer.ToArray(), 0, buffer.Length, ref _streamSocket);
        }

        public static void Write(NetworkStream aThis, byte[] buffer, int offset, int count,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket)
        {
            _streamSocket.Send(buffer, offset, count, 0);
        }

        public static void Flush(NetworkStream aThis,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket)
        {
            throw new NotImplementedException();
        }

        public static void Close(NetworkStream aThis,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket)
        {
            _streamSocket.Close();
        }

        public static void Dispose(NetworkStream aThis,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket)
        {
            _streamSocket.Close();
        }

        public static void Dispose(NetworkStream aThis, bool disposing,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.NetworkStream._streamSocket")] ref Socket _streamSocket)
        {
            _streamSocket.Close();
        }
    }
}
