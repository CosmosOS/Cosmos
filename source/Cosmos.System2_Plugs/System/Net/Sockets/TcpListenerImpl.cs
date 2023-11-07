using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(TcpListener))]
    public static class TcpListenerImpl
    {
        public static void Ctor(TcpListener aThis, IPAddress localaddr, int port)
        {
            throw new NotImplementedException();
        }

        public static void Start(TcpListener aThis)
        {
            throw new NotImplementedException();
        }

        public static TcpClient AcceptTcpClient(TcpListener aThis)
        {
            throw new NotImplementedException();
        }
    }
}
