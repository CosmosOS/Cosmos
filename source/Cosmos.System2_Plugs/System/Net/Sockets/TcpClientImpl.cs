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
            Cosmos.HAL.Global.debugger.Send("TcpClient - ctor.");
        }

        public static void Ctor(TcpClient aThis, Socket acceptedSocket, [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - ctor socket.");

            _clientSocket = acceptedSocket;

            Cosmos.HAL.Global.debugger.Send("TcpClient - ctor socket " + (acceptedSocket.RemoteEndPoint as IPEndPoint).Port + ":" + (acceptedSocket.RemoteEndPoint as IPEndPoint).Port);
        }

        public static int get_ReceiveBufferSize(TcpClient aThis)
        {
            //TODO implement Socket.SetSocketOption Socket.GetSocketOption
            return 8192;
        }

        public static NetworkStream GetStream(TcpClient aThis, [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket,
            [FieldAccess(Name = "System.Net.Sockets.NetworkStream System.Net.Sockets.TcpClient._dataStream")] ref NetworkStream _dataStream)
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

        public static void Dispose(TcpClient aThis, [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose");

            _clientSocket.Close();

            Cosmos.HAL.Global.debugger.Send("TcpClient - Closed");
        }
    }
}
