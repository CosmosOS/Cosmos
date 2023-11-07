using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.IO
{
    [Plug(Target = typeof(Stream))]
    public static class StreamImpl
    {
        public static void Dispose(Stream aThis)
        {
            throw new NotImplementedException();
        }

        public static void Dispose(Stream aThis, bool disposing)
        {
            throw new NotImplementedException();
        }
    }
}
