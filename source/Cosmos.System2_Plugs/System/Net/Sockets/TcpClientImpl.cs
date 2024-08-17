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
    [Plug(Target = typeof(TcpClient))]
    public static class TcpClientImpl
    {
        public static void Ctor(TcpClient aThis)
        {
        }

        public static void Ctor(TcpClient aThis, string hostname, int port,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            Connect(aThis, hostname, port, ref _clientSocket);
        }

        public static void Ctor(TcpClient aThis, Socket acceptedSocket,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            _clientSocket = acceptedSocket;
        }

        public static void set_Client(TcpClient aThis, Socket value,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            _clientSocket = value;
        }

        public static Socket get_Client(TcpClient aThis,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            return _clientSocket;
        }

        public static int get_ReceiveBufferSize(TcpClient aThis)
        {
            return Cosmos.System.Network.IPv4.TCP.Tcp.TcpWindowSize;
        }

        public static void Connect(TcpClient aThis, string hostname, int port,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            IPAddress address = IPAddress.Parse(hostname);
            IPEndPoint endpoint = new(address, port);

            Connect(aThis, endpoint, ref _clientSocket);
        }

        public static void Connect(TcpClient aThis, IPAddress address, int port,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            IPEndPoint endpoint = new(address, port);

            Connect(aThis, endpoint, ref _clientSocket);
        }

        private static void Connect(TcpClient aThis, IPEndPoint endpoint,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Bind(endpoint);
            _clientSocket.Connect(endpoint.Address, endpoint.Port);
        }

        public static void Connect(TcpClient aThis, IPAddress[] ipAddresses, int port)
        {
            throw new NotImplementedException(); 
        }

        public static NetworkStream GetStream(TcpClient aThis,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket,
            [FieldAccess(Name = "System.Net.Sockets.NetworkStream System.Net.Sockets.TcpClient._dataStream")] ref NetworkStream _dataStream)
        {
            if (_clientSocket == null)
            {
                Cosmos.HAL.Global.debugger.Send("TcpClient - GetStream _clientSocket null");
                throw new NullReferenceException();
            }
            if (_dataStream == null)
            {
                _dataStream = new NetworkStream(_clientSocket, true);
            }

            return _dataStream;
        }

        public static void Dispose(TcpClient aThis, bool disposing,
            [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket,
            [FieldAccess(Name = "System.Net.Sockets.NetworkStream System.Net.Sockets.TcpClient._dataStream")] ref NetworkStream _dataStream)
        {
            if (_dataStream != null)
            {
                _dataStream.Dispose();
            }
            else
            {
                if (_clientSocket != null)
                {
                    _clientSocket.Close();
                }
            }
        }
    }
}
