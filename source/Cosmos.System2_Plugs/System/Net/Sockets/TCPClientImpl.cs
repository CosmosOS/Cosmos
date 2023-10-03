using IL2CPU.API.Attribs;
using Cosmos.HAL.Drivers.Video;
using Cosmos.System.Graphics;
using Cosmos.System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(TcpClient))]
    public static class TcpClientImpl
    {
        public static void Ctor(TcpClient aThis, IPEndPoint localEP)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(TcpClient aThis)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(TcpClient aThis, AddressFamily family)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(TcpClient aThis, string hostname, int port)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(TcpClient aThis, Socket acceptedSocket)
        {
            throw new NotImplementedException();
        }

        public static NetworkStream GetStream(TcpClient aThis)
        {
            throw new NotImplementedException();
        }
    }
}
