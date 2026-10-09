using System.Runtime;

namespace Cosmos.Kernel.Core.Runtime;

internal static class Cpu
{
    /// <summary>
    /// replace this with some thing better
    /// </summary>
    private static long s_tickCount64 = 0;

    [RuntimeExport("RhpGetTickCount64")]
    public static unsafe long RhpGetTickCount64()
    {
        return s_tickCount64++;
    }

}
