using System.Runtime;
using System.Runtime.InteropServices.Marshalling;
using Cosmos.Kernel.Core.Bridge;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.Runtime;

internal class Thread
{
    /// <summary>
    /// Thread-static storage of the one thread that exists when the scheduler
    /// is compiled out. Null until CoreLib allocates it through the reference
    /// below, the same way a scheduled thread's slot starts.
    /// </summary>
    private static object[][]? s_threadData;

    [RuntimeExport("RhGetThreadStaticStorage")]
    internal static ref object[][]? RhGetThreadStaticStorage()
    {
        if (CosmosFeatures.SchedulerEnabled)
        {
            var cpuState = SchedulerManager.CurrentCpuState;
            return ref cpuState.CurrentThread!.GetThreadStaticStorage();
        }
        else
        {
            return ref s_threadData;
        }
    }

    [RuntimeExport("RhGetCurrentThreadStackBounds")]
    internal static void RhGetCurrentThreadStackBounds(out IntPtr pStackLow, out IntPtr pStackHigh)
    {
        if (CosmosFeatures.SchedulerEnabled)
        {
            SchedulerThread? current = SchedulerManager.CurrentCpuState?.CurrentThread;
            if (current != null && current.StackBase != 0)
            {
                pStackLow = (nint)current.StackBase;
                pStackHigh = (nint)(current.StackBase + current.StackSize);
                return;
            }
        }

        // Boot/idle thread: runs on the bootloader-provided stack.
        pStackHigh = (nint)BootStack.Top;
        pStackLow = pStackHigh - (nint)BootStack.Size;
    }

    /// <summary>
    /// Default stack size CoreLib's Thread.CreateThread uses when the
    /// constructor's maxStackSize is unset (&lt;= 0).
    /// </summary>
    [RuntimeExport("RhGetDefaultStackSize")]
    internal static IntPtr RhGetDefaultStackSize()
    {
        return (nint)SchedulerThread.DefaultStackSize;
    }

    /// <summary>
    /// Entry-point address CoreLib passes to SystemNative_CreateThread. Unused:
    /// Cosmos threads start at ThreadNative.EntryPointStub and the scheduler's
    /// InvokeCurrentThreadStart runs CoreLib's StartThread with the
    /// GCHandle&lt;Thread&gt; parameter itself.
    /// </summary>
    [RuntimeExport("RhGetThreadEntryPointAddress")]
    internal static IntPtr RhGetThreadEntryPointAddress()
    {
        return IntPtr.Zero;
    }

    [RuntimeExport("RhSetCurrentThreadName")]
    internal static unsafe void RhSetCurrentThreadName(ushort* name)
    {
        // Do nothing, the managed thread holds the string on a field.
        var managedName = Utf8StringMarshaller.ConvertToManaged((byte*)name);

        Serial.WriteString($"[Thread] Setting current thread name to '{managedName}'\n");
    }

    [RuntimeExport("RhSetThreadExitCallback")]
    internal static void RhSetThreadExitCallback(IntPtr callback)
    {
        if (CosmosFeatures.SchedulerEnabled)
        {
            SchedulerManager.OnThreadExitCallback = callback;
        }
    }

    [RuntimeExport("RhYield")]
    internal static int RhYield()
    {
        Serial.WriteString("RhYield Called\n");
        if (CosmosFeatures.SchedulerEnabled)
        {
            SchedulerThread? thread = SchedulerManager.CurrentCpuState?.CurrentThread;
            if (thread != null)
            {
                //TODO: Switch Threads (if possible)
                SchedulerManager.YieldThread(SchedulerManager.GetCurrentCpuId(), thread);
                InternalCpu.Halt();

                return 0;
            }
        }

        return 0;
    }

    [RuntimeExport("RhSpinWait")]
    internal static void RhSpinWait(int iterations)
    {
        // Simple spin wait
        for (int i = 0; i < iterations; i++)
        {
            // Spin
        }
    }
}
