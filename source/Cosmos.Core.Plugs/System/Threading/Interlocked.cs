using System.Threading;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.Threading
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
