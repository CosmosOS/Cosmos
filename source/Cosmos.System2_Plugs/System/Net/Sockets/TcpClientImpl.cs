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
            throw new NotImplementedException();
        }

        public static NetworkStream GetStream(TcpClient aThis)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(TcpClient aThis, [FieldAccess(Name = "System.Net.Sockets.Socket System.Net.Sockets.TcpClient._clientSocket")] ref Socket _clientSocket)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - Dispose");

            _clientSocket.Close();

            Cosmos.HAL.Global.debugger.Send("TcpClient - Closed");
        }
    }
}
