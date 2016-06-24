using System.Threading;

namespace Cosmos.IL2CPU.Plugs.System.Threading
{
    [Plug(Target = typeof(Thread))]
    public static class ThreadImpl
    {
        public static void MemoryBarrier()
        {

        }
    }
}
