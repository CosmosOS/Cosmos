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

        public static void Ctor(TcpClient aThis, Socket acceptedSocket)
        {
            Cosmos.HAL.Global.debugger.Send("TcpClient - ctor.");
        }

        public static int get_ReceiveBufferSize(TcpClient aThis)
        {
            throw new NotImplementedException();
        }

        public static NetworkStream GetStream(TcpClient aThis)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(TcpClient aThis)
        {
            throw new NotImplementedException();
        }
    }
}
