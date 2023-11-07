using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Net.Sockets
{
    [Plug(Target = typeof(NetworkStream))]
    public static class NetworkStreamImpl
    {
        public static int Read(NetworkStream aThis, byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public static int Write(NetworkStream aThis, byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public static void Flush(NetworkStream aThis)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(NetworkStream aThis)
        {
            throw new NotImplementedException();
        }
    }
}
