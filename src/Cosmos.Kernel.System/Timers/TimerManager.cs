// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.HAL.Timers;

namespace Cosmos.Kernel.System.Timers;

/// <summary>
/// Manages system timers.
/// </summary>
public static class TimerManager
{
    /// <summary>Nanoseconds in one <see cref="TimeSpan"/> tick.</summary>
    private const ulong NanosecondsPerTick = 100;

    /// <summary>
    /// Gets the registered tick source.
    /// </summary>
    internal static TickSource? TickSource { get; private set; }

    /// <summary>
    /// Whether timer support is compiled into this kernel
    /// (the <c>CosmosEnableTimer</c> feature switch).
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.TimerEnabled;

    /// <summary>
    /// Gets whether a tick source is registered. False when the timer is
    /// compiled out with CosmosEnableTimer=false, since every member of this
    /// class answers off that tick source.
    /// </summary>
    public static bool IsInitialized => TickSource is not null;

    /// <summary>
    /// Throws when timer support is compiled out. Guards the two members that
    /// would otherwise do nothing and say nothing: a wait that returns at once
    /// and a frequency change that lands nowhere both read as a kernel bug
    /// rather than as a switch left off in a csproj. The reads on this class
    /// answer honestly instead (<see cref="Frequency"/> is 0,
    /// <see cref="IsInitialized"/> is false), and <see cref="Schedule"/>,
    /// <see cref="ScheduleRecurring"/> and <see cref="Cancel"/> already carry
    /// the answer in what they return.
    /// </summary>
    private static void ThrowIfDisabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Timer support is disabled. Set CosmosEnableTimer=true in your csproj to enable it.");
        }
    }

    /// <summary>
    /// Registers a tick source with the manager.
    /// </summary>
    internal static void RegisterTickSource(TickSource? tickSource)
    {
        if (tickSource is null)
        {
            return;
        }

        TickSource = tickSource;
    }

    /// <summary>
    /// Frequency of the tick source in Hz, or 0 when none is registered,
    /// which <see cref="IsInitialized"/> reports and an assignment made in
    /// that state does nothing. Assigning reprograms the tick source, which
    /// divides a fixed input clock, so the value read back is the nearest tick
    /// the divisor can express rather than the value assigned. A frequency the
    /// tick source cannot divide to at all is refused rather than silently
    /// dropped.
    /// </summary>
    /// <exception cref="InvalidOperationException">Timer support is disabled.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The tick source cannot run at this frequency. Each one divides a
    /// fixed counter, so each accepts from that counter divided by its widest
    /// divisor up to the counter itself: 19 Hz to 1193180 Hz on the x64 PIT,
    /// and 1 Hz up to CNTFRQ_EL0 on the ARM64 generic timer.
    /// </exception>
    public static uint Frequency
    {
        get => TickSource?.Frequency ?? 0;
        set
        {
            ThrowIfDisabled();

            if (TickSource is null)
            {
                return;
            }

            if (!TickSource.SetFrequency(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "The tick source cannot run at this frequency.");
            }
        }
    }

    /// <summary>
    /// Blocks for the specified number of milliseconds.
    /// </summary>
    /// <param name="ms">Milliseconds to wait.</param>
    /// <exception cref="InvalidOperationException">Timer support is disabled.</exception>
    public static void Wait(uint ms)
    {
        ThrowIfDisabled();

        TickSource?.Wait(ms);
    }

    /// <summary>
    /// Schedules a callback to run once after the specified delay. The callback
    /// runs in interrupt context, so it must not block, and it must not let an
    /// exception escape: nothing above the interrupt dispatch catches one and
    /// the kernel halts. Use <see cref="AlarmManager.Schedule"/> for callbacks
    /// that need thread context.
    /// </summary>
    /// <param name="callback">Method to invoke when the delay expires.</param>
    /// <param name="delay">
    /// Delay before the timer fires. The tick source's period is the
    /// resolution, so a delay shorter than one tick, and a zero or negative
    /// delay, fire on the next tick.
    /// </param>
    /// <returns>The scheduled timer, or null if no tick source is registered.</returns>
    public static SoftwareTimer? Schedule(Action callback, TimeSpan delay)
    {
        return ScheduleCore(callback, ToNanoseconds(delay), recurring: false);
    }

    /// <summary>
    /// Schedules a callback to run repeatedly with the specified period. The
    /// callback runs in interrupt context under the same rules as
    /// <see cref="Schedule"/>; use <see cref="AlarmManager.ScheduleRecurring"/>
    /// for callbacks that need thread context.
    /// </summary>
    /// <param name="callback">Method to invoke each period.</param>
    /// <param name="period">
    /// Period between firings; must be positive. One tick of the tick source
    /// is the resolution, so a period shorter than one tick fires on every
    /// tick.
    /// </param>
    /// <returns>
    /// The scheduled timer, or null if no tick source is registered or the
    /// period is not positive.
    /// </returns>
    public static SoftwareTimer? ScheduleRecurring(Action callback, TimeSpan period)
    {
        if (period <= TimeSpan.Zero)
        {
            return null;
        }

        return ScheduleCore(callback, ToNanoseconds(period), recurring: true);
    }

    /// <summary>
    /// Cancels a timer returned by <see cref="Schedule"/> or <see cref="ScheduleRecurring"/>.
    /// </summary>
    /// <param name="timer">Timer to cancel.</param>
    /// <returns>
    /// True when the timer was pending and has been cancelled; false when it is
    /// null, had already fired, was already cancelled, or no tick source is
    /// registered.
    /// </returns>
    public static bool Cancel(SoftwareTimer? timer)
    {
        if (timer is null || TickSource is null)
        {
            return false;
        }

        return TickSource.UnregisterTimer(timer.Entry);
    }

    private static SoftwareTimer? ScheduleCore(Action callback, ulong timeoutNs, bool recurring)
    {
        if (TickSource is null || callback is null)
        {
            return null;
        }

        // Both allocations come before the registration, so running out of
        // memory cannot leave a registered entry with no handle to cancel it.
        TimerEntry entry = new(callback, timeoutNs, recurring);
        SoftwareTimer timer = new(entry);
        TickSource.RegisterTimer(entry);
        return timer;
    }

    /// <summary>
    /// Converts a duration to the nanoseconds a <see cref="TimerEntry"/>
    /// counts down. A non-positive duration becomes 0, which fires on the next
    /// tick, and a duration too large to express in nanoseconds saturates
    /// rather than wrapping.
    /// </summary>
    private static ulong ToNanoseconds(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            return 0;
        }

        // ulong.MaxValue nanoseconds is roughly 584 years; past that the
        // multiply below would wrap and produce a near-immediate timer.
        const long MaxTicks = (long)(ulong.MaxValue / NanosecondsPerTick);

        if (value.Ticks >= MaxTicks)
        {
            return ulong.MaxValue;
        }

        return (ulong)value.Ticks * NanosecondsPerTick;
    }
}
