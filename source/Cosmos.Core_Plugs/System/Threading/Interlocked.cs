using System.Threading;
using Cosmos.IL2CPU.API;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(Target = typeof(Interlocked))]
    public static class InterlockedImpl
    {
        public static int Decrement(ref int aData)
        {
            return aData -= 1;
        }
    }
}
