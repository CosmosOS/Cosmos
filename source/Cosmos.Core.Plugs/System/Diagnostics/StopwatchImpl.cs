using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.IL2CPU.Plugs;

using System;
using System.Diagnostics;

namespace Cosmos.Core.Plugs.System.Diagnostics
{
    [Plug(Target = typeof(global::System.Diagnostics.Stopwatch))]
    public static class StopwatchImpl
    {
        public static long GetTimestamp()
        {
            if (Stopwatch.IsHighResolution)
                return (long)(CPU.GetCycleCount() / (double)CPU.GetCycleRate() * 1000000d); // see https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx
            else
                return DateTime.UtcNow.Ticks;
        }
    }
}
