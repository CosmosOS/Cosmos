using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(Socket))]
    public static class SocketImpl
    {
        public static int Send(Socket aThis, ReadOnlySpan<byte> buffer, SocketFlags socketFlags)
        {
            throw new NotImplementedException();
        }

        public static int Receive(Socket aThis, Span<byte> buffer, SocketFlags socketFlags)
        {
            throw new NotImplementedException();
        }

        public static void Close(Socket aThis, int timeout)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(Socket aThis)
        {
            throw new NotImplementedException();
        }
    }
}
