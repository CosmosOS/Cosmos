using System;
using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug(Target = typeof(Thread))]
    public static class ThreadImpl
    {
        public static Thread GetCurrentThreadNative()
        {
            return null;
        }

        public static void MemoryBarrier()
        {

        }
        
        public static void Sleep(int ms)
        {
            if ((ms > 0) && (ms != Timeout.Infinite))
            {
                byte xVoid;
                for (ulong i = 0; i < (ulong)(ms * 155 /* this is not the real amount of microseconds because the loop takes a lot of times too*/); i++)
                {
                    // 10 IOPort Reads = 1 Microsecond
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                    xVoid = Core.Global.BaseIOGroups.ATA1.Status.Byte;
                }
            }
            else if (ms < 0) throw new ArgumentOutOfRangeException("millisecondsTimeout", "millisecondsTimeout should be greater than or equals to 0");
        }
    }
}
