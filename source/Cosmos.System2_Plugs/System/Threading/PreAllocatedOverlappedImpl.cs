using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Threading
{
    [Plug(Target = typeof(PreAllocatedOverlapped))]
    public static class PreAllocatedOverlappedImpl
    {
        [PlugMethod(Signature = "System_Void__System_Threading_PreAllocatedOverlapped_System_Threading_IDeferredDisposable_OnFinalRelease_System_Boolean_")]
        public static void OnFinalRelease(SocketAsyncEventArgs aThis, bool disposed)
        {
            throw new NotImplementedException();
        }
    }
}
