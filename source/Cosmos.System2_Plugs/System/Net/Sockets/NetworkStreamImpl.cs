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
    [Plug(Target = typeof(NetworkStream))]
    internal class NetworkStreamImpl
    {
        public static void Ctor(NetworkStream aThis, Socket socket)
        {
            throw new NotImplementedException();
        }

        public static void Ctor(NetworkStream aThis, Socket socket, bool ownsSocket)
        {
            throw new NotImplementedException();
        }

        public static void Read(NetworkStream aThis, byte[] buffer, int offset, int size)
        {
            throw new NotImplementedException();
        }

        public static void Write(NetworkStream aThis, byte[] buffer, int offset, int size)
        {
            throw new NotImplementedException();
        }
    }
}
