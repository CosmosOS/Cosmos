using IL2CPU.API.Attribs;
using Cosmos.HAL.Drivers.Video;
using Cosmos.System.Graphics;
using Cosmos.System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(TcpListener))]
    public static class TcpListenerImpl
    {
        public static void Ctor(TcpListener aThis, IPEndPoint localEP)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(TcpListener aThis, IPAddress localaddr, int port)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(TcpListener aThis, int port)
        {
            throw new NotImplementedException();
        }

        public static void Start(TcpListener aThis)
        {
            throw new NotImplementedException();
        }

        public static void Stop(TcpListener aThis)
        {
            throw new NotImplementedException();
        }

        public static TcpClient AcceptTcpClient(TcpListener aThis)
        {
            throw new NotImplementedException();
        }
    }
}
