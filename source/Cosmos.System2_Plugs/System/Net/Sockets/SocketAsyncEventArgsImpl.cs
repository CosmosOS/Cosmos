using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(SocketAsyncEventArgs))]
    public static class SocketAsyncEventArgsImpl
    {
        public static void Ctor(SocketAsyncEventArgs aThis)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(SocketAsyncEventArgs aThis, bool unsafeSuppressExecutionContextFlow)
        {
            throw new NotImplementedException();
        }
    }
}
