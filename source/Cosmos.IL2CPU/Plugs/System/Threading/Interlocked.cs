using System.Threading;

namespace Cosmos.IL2CPU.Plugs.System.Threading
{
    [Plug(Target=typeof(Interlocked))]
    public static class InterlockedImpl
    {
        public static int Decrement(ref int aData) {
            return aData -= 1;
        }
    }
}
