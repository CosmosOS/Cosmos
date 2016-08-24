using Cosmos.IL2CPU.Plugs;

using System;
using System.Diagnostics;

namespace Cosmos.Core.Plugs.System.Diagnostics
{
    [Plug(Target = typeof(global::System.Diagnostics.Stopwatch))]
    public class StopwatchImpl
    {
        public static long GetTimestamp()
        {
            if (Stopwatch.IsHighResolution)
                // see https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx for more details
                return (long)(ProcessorInformation.GetCycleCount() / (double)ProcessorInformation.GetCycleRate() * 1000000d);
            else
                return DateTime.UtcNow.Ticks;
        }
    }
}
