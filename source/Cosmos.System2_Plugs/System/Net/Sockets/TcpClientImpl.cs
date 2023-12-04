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
