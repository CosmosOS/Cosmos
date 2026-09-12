using System.Diagnostics;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using SysThread = System.Threading.Thread;

namespace Cosmos.Kernel.Core.Bridge.Interop;

internal static unsafe partial class libSystemNative
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessCpuInformation
    {
        internal ulong _lastRecordedCurrentTime;
        internal ulong _lastRecordedKernelTime;
        internal ulong _lastRecordedUserTime;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_GetCpuUtilization")]
    internal static unsafe double SystemNative_GetCpuUtilization(ProcessCpuInformation* previousCpuInfo)
    {
        if (SchedulerManager.Threads == null)
        {
            return 0.0;
        }

        ulong currentTime = GetMonotonicNs();
        ulong busyTime = SchedulerManager.GetBusyCpuTimeNs();

        ulong lastTime = previousCpuInfo->_lastRecordedCurrentTime;
        ulong lastBusy = previousCpuInfo->_lastRecordedUserTime;

        // First call: seed snapshot only.
        if (lastTime == 0)
        {
            previousCpuInfo->_lastRecordedCurrentTime = currentTime;
            previousCpuInfo->_lastRecordedUserTime = busyTime;
            previousCpuInfo->_lastRecordedKernelTime = 0;
            return 0.0;
        }

        // Window too short for meaningful sample (< 5 ticks at 10 ms quantum).
        // Leave snapshot untouched so the next call sees a longer window.
        if (currentTime - lastTime < 50_000_000UL)
        {
            return 0.0;
        }

        double utilization = 0.0;
        if (busyTime >= lastBusy)
        {
            ulong totalElapsed = (currentTime - lastTime) * SchedulerManager.CpuCount;
            ulong busyElapsed = busyTime - lastBusy;
            if (totalElapsed > 0 && busyElapsed > 0)
            {
                utilization = (double)busyElapsed * 100.0 / (double)totalElapsed;
                if (utilization > 100.0)
                {
                    utilization = 100.0;
                }
            }
        }

        previousCpuInfo->_lastRecordedCurrentTime = currentTime;
        previousCpuInfo->_lastRecordedUserTime = busyTime;
        previousCpuInfo->_lastRecordedKernelTime = 0;
        return utilization;
    }

    [UnmanagedCallersOnly(EntryPoint = "SystemNative_SchedGetCpu")]
    internal static int SystemNative_SchedGetCpu()
    {
        return (int)SchedulerManager.GetCurrentCpuId();
    }

    /// <summary>
    /// Smallest honored thread stack — CoreCLR's arbitrary minimum for
    /// stack-size settings (RhConfig.cpp), standing in for PTHREAD_STACK_MIN
    /// in upstream's pal_threading.c. Requests below CoreLib's 128KB
    /// MinExecutionStackSize are still honored, like upstream on Linux:
    /// EnsureSufficientExecutionStack then throws on that thread.
    /// </summary>
    private const nuint MinStackSize = 64 * 1024;

    /// <summary>Stack sizes are rounded up to whole pages.</summary>
    private const nuint StackSizeAlignment = 4096;

#if ARCH_X64
    [LibraryImport("*", EntryPoint = "_native_x64_get_code_selector")]
    [SuppressGCTransition]
    private static partial ulong GetCurrentCodeSelector();
#endif

    /// <summary>
    /// Backs CoreLib's <c>Interop.Sys.CreateThread</c> P/Invoke. Upstream
    /// <c>Thread.CreateThread</c> resolves the stack size itself — the
    /// constructor's <c>maxStackSize</c>, or <c>RhGetDefaultStackSize</c> when
    /// unset — and passes it here, so honoring
    /// <c>new Thread(start, maxStackSize)</c> needs no access to Thread's
    /// private StartHelper.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "SystemNative_CreateThread")]
    internal static int SystemNative_CreateThread(IntPtr stackSize, delegate* unmanaged<IntPtr, IntPtr> startAddress, IntPtr parameter)
    {
        // startAddress (CoreLib's ThreadEntryPoint) is unused: threads start at
        // ThreadNative.EntryPointStub and the scheduler's InvokeCurrentThreadStart
        // runs CoreLib's StartThread with the GCHandle<Thread> parameter itself.
        _ = startAddress;

        if (!SchedulerManager.IsRunning)
        {
            // Same behavior as before the scheduler existed: report success,
            // the thread simply never runs.
            return 1;
        }

        nuint size = ((nuint)stackSize + (StackSizeAlignment - 1)) & ~(StackSizeAlignment - 1);
        if (size < MinStackSize)
        {
            size = MinStackSize;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            // Create scheduler thread with SchedulerThreadFlags.Managed set.
            // SchedulerManager.InvokeCurrentThreadStart evaluates it to
            // call the managed startup or not.
            SchedulerThread thread = new SchedulerThread
            {
                Id = SchedulerManager.AllocateThreadId(),
                CpuId = 0,
                State = SchedulerThreadState.Created,
                Flags = SchedulerThreadFlags.Managed,
                // CoreLib's handle in `parameter` lives only until the thread
                // reports itself started; the mechanism keeps its own for the
                // thread's whole life, so a kill can reach the managed side.
                ManagedThread = new GCHandle<SysThread>(GCHandle<SysThread>.FromIntPtr(parameter).Target)
            };

            nuint entryPoint = (nuint)(delegate* unmanaged<IntPtr, void>)&ThreadNative.EntryPointStub;
#if ARCH_X64
            ushort cs = (ushort)GetCurrentCodeSelector();
            thread.InitializeStack(entryPoint, cs, (nuint)parameter, size);
#elif ARCH_ARM64
            // ARM64: no code selector needed, use 0.
            thread.InitializeStack(entryPoint, 0, (nuint)parameter, size);
#endif
            SchedulerManager.CreateThread(0, thread);
            SchedulerManager.ReadyThread(0, thread);

            Serial.WriteString("[libSystemNative] Thread ");
            Serial.WriteNumber(thread.Id);
            Serial.WriteString(" scheduled, stack ");
            Serial.WriteNumber((ulong)size);
            Serial.WriteString(" bytes\n");
        }

        return 1;
    }

    private static ulong GetMonotonicNs()
    {
        long ticks = Stopwatch.GetTimestamp();
        long freq = Stopwatch.Frequency;
        if (freq <= 0)
        {
            return 0;
        }
        ulong t = (ulong)ticks;
        ulong f = (ulong)freq;
        // Split mul/div: ticks * 1e9 overflows in ~147 s on a 62.5 MHz ARM64 timer.
        return (t / f) * 1_000_000_000UL + ((t % f) * 1_000_000_000UL) / f;
    }
}
