using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Bridge.Interop;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Bridge function that gives C library code (uACPI) a monotonic clock.
/// </summary>
internal static class TimeNative
{
    /// <summary>
    /// Nanoseconds on the clock behind <see cref="System.Diagnostics.Stopwatch"/>:
    /// the TSC on x64, the generic timer on ARM64.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_get_nanoseconds")]
    public static ulong GetNanoseconds()
    {
        return libSystemNative.GetMonotonicNs();
    }
}
