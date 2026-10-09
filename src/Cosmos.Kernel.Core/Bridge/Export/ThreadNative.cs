using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Stable native entry point used as the initial RIP/PC for freshly scheduled
/// threads. The architecture-specific context-switch assembly returns (via iretq
/// on x64 / eret on ARM64) into <see cref="EntryPointStub"/>, which then calls
/// the scheduler's managed entry implementation. Thread-start registration lives
/// in <see cref="SchedulerManager.InvokeCurrentThreadStart"/>.
/// </summary>
internal static class ThreadNative
{
    [UnmanagedCallersOnly]
    public static void EntryPointStub(IntPtr parameter)
    {
        SchedulerManager.InvokeCurrentThreadStart(parameter);
    }
}
