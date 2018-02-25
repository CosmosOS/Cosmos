using System;
using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Threading
{
    [Plug(Target = typeof(global::System.Threading.SpinWait))]
    public static class SpinWaitImpl
    {
        public static void SpinOnce(ref SpinWait aThis)
        {
            throw new NotImplementedException("SpinWait.SpinOnce()");
        }
    }
}
