using System;
using System.Threading;
using Cosmos.HAL;
using IL2CPU.API.Attribs;

namespace Cosmos.System2_Plugs.System.Threading
{
    [Plug(Target = typeof(Thread))]
    public static class ThreadImpl
    {

        public static void Sleep(TimeSpan timeout)
        {
            Global.PIT.Wait((uint) timeout.TotalMilliseconds);
        }

        public static void Sleep(int millisecondsTimeout)
        {
            Global.PIT.Wait((uint) millisecondsTimeout);
        }

    }
}
