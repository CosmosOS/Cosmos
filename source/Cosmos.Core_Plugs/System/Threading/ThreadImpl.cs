using System.Threading;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Threading
{
    [Plug("System.Threading.Thread, System.Private.CoreLib")]
    public static class ThreadImpl
    {
        public static Thread GetCurrentThreadNative()
        {
            return null;
        }

        public static void MemoryBarrier()
        {

        }

        //    public static void SleepInternal(int ms)
        //    {
        //        // Implementation of http://referencesource.microsoft.com/#mscorlib/system/threading/thread.cs,6a577476abf2f437,references
        //        // see https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx for more details

        //        if ((ms > 0) && (ms != Timeout.Infinite))
        //        {
        //            double fac = ProcessorInformation.GetCycleRate() / 1000d;
        //            double ticks = ms / 1000d * Stopwatch.Frequency + ProcessorInformation.GetCycleCount() * fac;

        //            while (ticks < ProcessorInformation.GetCycleCount() * fac)
        //                new Action(() => { }).Invoke(); // execute an empty operation
        //        }
        //        else if (ms < 0)
        //            throw new ThreadInterruptedException();
        //    }
    }
}
