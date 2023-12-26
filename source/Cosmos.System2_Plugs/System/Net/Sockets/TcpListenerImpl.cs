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
        private static Socket? _serverSocket;
        private static IPEndPoint _serverSocketEP;

        public static void Ctor(TcpListener aThis, IPEndPoint localEP)
        {
            if (localEP == null)
            {
                throw new ArgumentNullException(nameof(localEP));
            }
            _serverSocketEP = localEP;
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public static void Ctor(TcpListener aThis, IPAddress localaddr, int port)
        {
            if (localaddr == null)
            {
                Cosmos.HAL.Global.debugger.Send("TcpListener - Ctor localaddr == null");
                throw new ArgumentNullException(nameof(localaddr));
            }

            _serverSocketEP = new IPEndPoint(localaddr, port);
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public static void Start(TcpListener aThis)
        {
            _serverSocket!.Bind(_serverSocketEP);

            try
            {
                _serverSocket.Listen();
            }
            catch (SocketException)
            {
                // aThis.Stop();
                throw;
            }
        }

        public static TcpClient AcceptTcpClient(TcpListener aThis)
        {
            Socket acceptedSocket = _serverSocket!.Accept();
            var realClient = new TcpClient();
            realClient.Client = acceptedSocket;
            return realClient;
        }
    }
}
