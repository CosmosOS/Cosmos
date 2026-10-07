// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Timers;

namespace Cosmos.Kernel.System.Timers;

/// <summary>
/// A software timer that invokes a callback after a delay, driven by the
/// periodic tick of the platform timer device. Callbacks run in interrupt
/// context: they must not block, and an exception escaping one halts the
/// kernel, because the interrupt dispatch has no handler above it.
/// </summary>
/// <remarks>
/// A kernel obtains one from <see cref="TimerManager.Schedule"/> or
/// <see cref="TimerManager.ScheduleRecurring"/> and passes it back to
/// <see cref="TimerManager.Cancel"/>. It is a read-only handle:
/// <see cref="TimerManager"/> creates it over an entry in the timer device's
/// registry, and only the device drives that entry's countdown.
/// </remarks>
public sealed class SoftwareTimer
{
    /// <summary>
    /// The timer device's registry entry this handle wraps.
    /// </summary>
    internal TimerEntry Entry { get; }

    /// <summary>
    /// The delay before the timer fires, in nanoseconds. For recurring timers, the period between firings.
    /// </summary>
    public ulong TimeoutNs => Entry.TimeoutNs;

    /// <summary>
    /// Whether the timer reloads after firing, or fires only once.
    /// </summary>
    public bool Recurring => Entry.Recurring;

    /// <summary>
    /// Whether the timer is registered with a device and pending. One-shot
    /// timers become inactive after firing; cancelling also deactivates.
    /// </summary>
    public bool IsActive => Entry.IsActive;

    /// <summary>
    /// Wraps a registry entry in the handle the ring hands out.
    /// </summary>
    /// <param name="entry">The entry the timer device counts down.</param>
    internal SoftwareTimer(TimerEntry entry)
    {
        Entry = entry;
    }
}
