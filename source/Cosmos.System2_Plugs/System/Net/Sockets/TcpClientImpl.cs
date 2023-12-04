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

        public static void CCtor(TcpClient aThis)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - cctor.");
        }

        public static void Ctor(TcpClient aThis)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - ctor.");
        }

        public static void Ctor(TcpClient aThis, Socket acceptedSocket)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - ctor socket.");

            _clientSocket = acceptedSocket;

            Cosmos.HAL.Global.debugger.Send("TcpClient - ctor socket " + (acceptedSocket.RemoteEndPoint as IPEndPoint).Port + ":" + (acceptedSocket.RemoteEndPoint as IPEndPoint).Port);
        }

        public static void set_Client(TcpClient aThis, Socket value)
        {
            _clientSocket = value;
        }

        public static Socket get_Client(TcpClient aThis)
        {
            return _clientSocket;
        }

        public static void set_Client(TcpClient aThis, Socket socket)
        {
            _clientSocket = socket;
        }

        public static int get_ReceiveBufferSize(TcpClient aThis)
        {
            //TODO implement Socket.SetSocketOption Socket.GetSocketOption
            return 8192;
        }

        public static NetworkStream GetStream(TcpClient aThis)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - GetStream");

            if (_clientSocket == null)
            {
                Cosmos.HAL.Global.debugger.Send("TcpClient - GetStream _clientSocket null");
                throw new NullReferenceException();
            }
            if (_dataStream == null)
            {
                _dataStream = new NetworkStream(_clientSocket, true);
            }

            Cosmos.HAL.Global.debugger.Send("TcpClient - Created network stream");

            return _dataStream;
        }

        public static void Dispose(TcpClient aThis, bool disposing)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose 2");

            if (_dataStream != null)
            {
                Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose 2.1");

                _dataStream.Dispose();

                Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose 2.2");
            }
            else
            {
                Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose 2.4");
                if (_clientSocket != null)
                {
                    Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose 2.5");
                    _clientSocket.Close();
                }
            }

            Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose 3");
        }
    }
}
