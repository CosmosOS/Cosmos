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

        private static Cosmos.System.Network.IPv4.TCP.TcpListener tcpListener;

        public static void Ctor(TcpListener aThis, IPEndPoint localEP)
        {
            if (localEP == null)
            {
                throw new ArgumentNullException(nameof(localEP));
            }
            _serverSocketEP = localEP;
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcpListener = new Cosmos.System.Network.IPv4.TCP.TcpListener((ushort)localEP.Port);
        }

        public static void Ctor(TcpListener aThis, IPAddress localaddr, int port)
        {
            Cosmos.HAL.Global.debugger.Send("TcpListener - ctor.");

            if (localaddr == null)
            {
                throw new ArgumentNullException(nameof(localaddr));
            }

            Cosmos.HAL.Global.debugger.Send("TcpListener - localaddr ok.");

            _serverSocketEP = new IPEndPoint(localaddr, port);

            Cosmos.HAL.Global.debugger.Send("TcpListener - _serverSocketEP ok.");

            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            tcpListener = new Cosmos.System.Network.IPv4.TCP.TcpListener((ushort)port);

            Cosmos.HAL.Global.debugger.Send("TcpListener - _serverSocket ok.");
        }

        public static void Start(TcpListener aThis)
        {
            tcpListener.Start();
        }

        public static TcpClient AcceptTcpClient(TcpListener aThis)
        {
            if (!tcpListener.IsListening())
            {
                throw new InvalidOperationException("TcpListener not active.");
            }

            Cosmos.HAL.Global.debugger.Send("TcpListener - accepting client.");

            var client = tcpListener.AcceptTcpClient();

            Cosmos.HAL.Global.debugger.Send("TcpListener - AcceptTcpClient ok.");

            var realClient = new TcpClient();
            realClient.Client = _serverSocket;
            return realClient;
        }
    }
}
