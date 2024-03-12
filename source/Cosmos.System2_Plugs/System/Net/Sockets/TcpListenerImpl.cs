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
        public static void Ctor(TcpListener aThis, IPEndPoint localEP,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpListener._serverSocket")] ref Socket _serverSocket,
            [FieldAccess(Name = "System.Net.IPEndPoint System.Net.Sockets.TcpListener._serverSocketEP")] ref IPEndPoint _serverSocketEP)
        {
            if (localEP == null)
            {
                throw new ArgumentNullException(nameof(localEP));
            }
            _serverSocketEP = localEP;
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public static void Ctor(TcpListener aThis, IPAddress localaddr, int port,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpListener._serverSocket")] ref Socket _serverSocket,
            [FieldAccess(Name = "System.Net.IPEndPoint System.Net.Sockets.TcpListener._serverSocketEP")] ref IPEndPoint _serverSocketEP)
        {
            if (localaddr == null)
            {
                Cosmos.HAL.Global.debugger.Send("TcpListener - Ctor localaddr == null");
                throw new ArgumentNullException(nameof(localaddr));
            }

            _serverSocketEP = new IPEndPoint(localaddr, port);
            _serverSocket = new Socket(_serverSocketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public static void Start(TcpListener aThis,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpListener._serverSocket")] ref Socket _serverSocket,
            [FieldAccess(Name = "System.Net.IPEndPoint System.Net.Sockets.TcpListener._serverSocketEP")] ref IPEndPoint _serverSocketEP)
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

        public static TcpClient AcceptTcpClient(TcpListener aThis,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpListener._serverSocket")] ref Socket _serverSocket,
            [FieldAccess(Name = "System.Net.IPEndPoint System.Net.Sockets.TcpListener._serverSocketEP")] ref IPEndPoint _serverSocketEP)
        {
            TcpClient realClient = new();
            Socket acceptedSocket = _serverSocket!.Accept();
            realClient.Client = acceptedSocket;
            return realClient;
        }
    }
}
