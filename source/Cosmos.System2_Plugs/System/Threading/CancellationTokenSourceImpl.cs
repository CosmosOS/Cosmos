using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Threading
{
    [Plug(typeof(CancellationTokenSource))]
    public static class CancellationTokenSourceImpl
    {
        public static void NotifyCancellation(CancellationTokenSource aThis, bool aBool)
        {
            throw new NotImplementedException();
        }
        public static void Dispose(CancellationTokenSource aThis)
        {
            throw new NotImplementedException();
        }
    }
}
