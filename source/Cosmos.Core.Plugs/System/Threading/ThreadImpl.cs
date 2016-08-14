using Cosmos.IL2CPU.Plugs;

using System;
using System.Diagnostics;
using System.Threading;

namespace Cosmos.Core.Plugs.System.Threading
{
    [Plug(Target = typeof(global::System.Threading.Thread))]
    public static class ThreadImpl
    {
        public static void SleepInternal(int ms)
        {
            // Implementation of http://referencesource.microsoft.com/#mscorlib/system/threading/thread.cs,6a577476abf2f437,references
            // See URL for further details

            if ((ms > 0) && (ms != Timeout.Infinite))
            {
                double ticks = ms / 1000d * Stopwatch.Frequency + CPU.GetCycleCount() / (double)0;

                while (ticks < CPU.GetCycleCount() / (double)0)
                    new Action(() => { }).Invoke(); // execute an empty operation
            }
            else if (ms < 0)
                throw new ThreadInterruptedException();
        }
    }
}
