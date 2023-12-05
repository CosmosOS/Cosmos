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
        private static Socket _clientSocket;
        private static NetworkStream _dataStream;

        public static void Ctor(TcpClient aThis)
        {
        }

        public static void Ctor(TcpClient aThis, Socket acceptedSocket)
        {
            _clientSocket = acceptedSocket;
        }

        public static void set_Client(TcpClient aThis, Socket value)
        {
            _clientSocket = value;
        }

        public static Socket get_Client(TcpClient aThis)
        {
            return _clientSocket;
        }

        public static int get_ReceiveBufferSize(TcpClient aThis)
        {
            //TODO implement Socket.SetSocketOption Socket.GetSocketOption
            return 8192;
        }

        public static void Connect(TcpClient aThis, string hostname, int port, [FieldAccess(Name = "System.Boolean System.Net.Sockets.TcpClient._active")] ref bool _active)
        {
            var address = IPAddress.Parse(hostname);
            var endpoint = new IPEndPoint(address, port);

            Cosmos.HAL.Global.debugger.Send("address - " + address.ToString());
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _clientSocket.Bind(endpoint);
            _clientSocket.Connect(address, port);
            _active = true;
        }

        public static void Connect(TcpClient aThis, IPAddress address, int port, [FieldAccess(Name = "System.Boolean System.Net.Sockets.TcpClient._active")] ref bool _active)
        {
            _clientSocket.Connect(address, port);
            _active = true;
        }

        public static void Connect(TcpClient aThis, IPEndPoint remoteEP, [FieldAccess(Name = "System.Boolean System.Net.Sockets.TcpClient._active")] ref bool _active)
        {
            throw new NotImplementedException();
        }

        public static void Connect(TcpClient aThis, IPAddress[] ipAddresses, int port)
        {
            throw new NotImplementedException(); 
        }

        public static NetworkStream GetStream(TcpClient aThis)
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

        public static void Dispose(TcpClient aThis, bool disposing)
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
