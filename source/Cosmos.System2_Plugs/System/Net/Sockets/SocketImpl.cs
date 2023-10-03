using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(Socket))]
    public static class SocketImpl
    {
        public static void Ctor(Socket aThis, SocketType socketType, ProtocolType protocolType)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(Socket aThis, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(Socket aThis, SafeSocketHandle handle)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(Socket aThis)
        {
            throw new NotImplementedException();
        }
    }
}
