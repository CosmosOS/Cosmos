using System;
using Cosmos.HAL;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System.Threading
{
    [Plug(Target = typeof(global::System.Threading.Thread))]
    public static class ThreadImpl
    {
        public static void Sleep(TimeSpan timeout)
        {
            Global.PIT.Wait((uint)timeout.TotalMilliseconds);
        }

        public static void Sleep(int millisecondsTimeout)
        {
            Global.PIT.Wait((uint)millisecondsTimeout);
        }

        public static bool Yield()
        {
            throw new NotImplementedException("Thread.Yield()");
        }

        public static void SpinWaitInternal(object iterations)
        {
            throw new NotImplementedException("Thread.SpinWaitInternal()");
        }
    }
}
