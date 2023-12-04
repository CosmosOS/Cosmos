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
        private static Socket _streamSocket;

        public static void CCtor(NetworkStream aThis)
        {
            Cosmos.HAL.Global.debugger.Send("NetworkStream - cctor.");
        }

        public static void Ctor(NetworkStream aThis, Socket socket, FileAccess access, bool ownsSocket,
            [FieldAccess(Name = "System.Boolean System.Net.Sockets.NetworkStream._ownsSocket")] ref bool _ownsSocket,
            [FieldAccess(Name = "System.Boolean System.Net.Sockets.NetworkStream._readable")] ref bool _readable,
            [FieldAccess(Name = "System.Boolean System.Net.Sockets.NetworkStream._writeable")] ref bool _writeable
            )
        {
            Cosmos.HAL.Global.debugger.Send("NetworkStream - ctor.");

            if (socket == null)
            {
                throw new ArgumentNullException(nameof(socket));
            }
            if (!socket.Connected)
            {
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

        public static int Read(NetworkStream aThis, byte[] buffer, int offset, int count)
        {
            Cosmos.HAL.Global.debugger.Send("NetworkStream - Read.");

            return _streamSocket.Receive(buffer, offset, count, 0);
        }

        public static void Write(NetworkStream aThis, byte[] buffer, int offset, int count)
        {
            Cosmos.HAL.Global.debugger.Send("NetworkStream - Write.");

            _streamSocket.Send(buffer, offset, count, 0);
        }

        public static void Flush(NetworkStream aThis)
        {
            throw new NotImplementedException();
        }

        public static void Close(NetworkStream aThis)
        {
            Cosmos.HAL.Global.debugger.Send("NetworkStream - Close");

            _streamSocket.Close();
        }

        public static void Dispose(NetworkStream aThis)
        {
            Cosmos.HAL.Global.debugger.Send("NetworkStream - Dispose 1.");

            _streamSocket.Close();
        }

        public static void Dispose(NetworkStream aThis, bool disposing)
        {
            Cosmos.HAL.Global.debugger.Send("NetworkStream - Dispose 2.");

            _streamSocket.Close();
        }
    }
}
