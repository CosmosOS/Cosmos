using System;
using System.Diagnostics;
using Cosmos.Core;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System.Diagnostics;

[Plug(Target = typeof(Stopwatch))]
public class StopwatchImpl
{
    public static long GetTimestamp()
    {
        if (Stopwatch.IsHighResolution)
            // see https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx for more details
        {
            return (long)(CPU.GetCPUUptime() / (double)CPU.GetCPUUptime() * 1000000d);
        }

        return DateTime.UtcNow.Ticks;
    }
}
