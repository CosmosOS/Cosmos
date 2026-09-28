// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// The kit's clock and its one rule for sleeping: a thread the scheduler
/// owns sleeps through the scheduler; the boot thread, which is the
/// scheduler's idle thread, and any code running before the scheduler exists
/// spin on a <see cref="Stopwatch"/> deadline with interrupts enabled and
/// never halt. Putting the idle thread to sleep would take the scheduler's
/// fallback off its queue, and a halt with no interrupt to end it never ends.
/// </summary>
internal static class KitTime
{
    private const long MillisecondsPerSecond = 1000;
    private const long MicrosecondsPerSecond = 1_000_000;

    /// <summary>The scheduler thread running this code, or null without a scheduler.</summary>
    public static SchedulerThread? CurrentThread => SchedulerManager.IsReady
        ? SchedulerManager.CurrentCpuState?.CurrentThread
        : null;

    /// <summary>True when the caller is a thread the scheduler may put to sleep.</summary>
    public static bool CanSleep
    {
        get
        {
            SchedulerThread? current = CurrentThread;
            return current is not null && (current.Flags & SchedulerThreadFlags.IdleThread) == 0;
        }
    }

    /// <summary>A timestamp <paramref name="milliseconds"/> from now, in <see cref="Stopwatch"/> ticks.</summary>
    /// <param name="milliseconds">How far ahead.</param>
    public static long DeadlineAfter(uint milliseconds) =>
        Stopwatch.GetTimestamp() + Stopwatch.Frequency / MillisecondsPerSecond * milliseconds;

    /// <summary>True once <paramref name="deadline"/> is in the past.</summary>
    /// <param name="deadline">A value from <see cref="DeadlineAfter"/>.</param>
    public static bool HasPassed(long deadline) => Stopwatch.GetTimestamp() >= deadline;

    /// <summary>Busy-waits for <paramref name="microseconds"/> on the timestamp counter, for a caller with no platform delay.</summary>
    /// <param name="microseconds">How long.</param>
    public static void Delay(uint microseconds)
    {
        long deadline = Stopwatch.GetTimestamp() + Stopwatch.Frequency / MicrosecondsPerSecond * microseconds;
        while (Stopwatch.GetTimestamp() < deadline)
        {
        }
    }

    /// <summary>
    /// Gives up the CPU for about <paramref name="milliseconds"/>: a scheduler
    /// sleep on a thread that may sleep, a busy wait with interrupts enabled
    /// otherwise.
    /// </summary>
    /// <param name="milliseconds">How long.</param>
    public static void Sleep(uint milliseconds)
    {
        if (CanSleep)
        {
            SchedulerManager.Sleep(milliseconds);
            return;
        }

        long deadline = DeadlineAfter(milliseconds);
        while (!HasPassed(deadline))
        {
        }
    }
}
