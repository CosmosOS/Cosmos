// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Interfaces.Devices;

/// <summary>
/// A software timer that invokes a callback after a delay, driven by the
/// periodic tick of the timer device it is registered with. Callbacks run
/// in interrupt context: they must not block, and an exception escaping one
/// halts the kernel, because the interrupt dispatch has no handler above it.
/// </summary>
/// <remarks>
/// A kernel obtains one from TimerManager.Schedule or ScheduleRecurring and
/// passes it back to TimerManager.Cancel. Creating one and driving its tick
/// state belongs to the timer device, so those members are internal.
/// </remarks>
public sealed class SoftwareTimer
{
    private readonly Action _callback;
    private ulong _remainingNs;
    private volatile bool _active;

    /// <summary>
    /// Initializes a new software timer with the specified callback and delay.
    /// </summary>
    /// <param name="callback">The method to invoke when the timer fires.</param>
    /// <param name="timeoutNs">The delay before the timer fires, in nanoseconds. For recurring timers, the period between firings.</param>
    /// <param name="recurring">Whether the timer reloads after firing, or fires only once.</param>
    internal SoftwareTimer(Action callback, ulong timeoutNs, bool recurring)
    {
        _callback = callback;
        TimeoutNs = timeoutNs;
        Recurring = recurring;
        _remainingNs = timeoutNs;
    }

    /// <summary>
    /// The delay before the timer fires, in nanoseconds. For recurring timers, the period between firings.
    /// </summary>
    public ulong TimeoutNs { get; }

    /// <summary>
    /// Whether the timer reloads after firing, or fires only once.
    /// </summary>
    public bool Recurring { get; }

    /// <summary>
    /// Whether the timer is registered with a device and pending. One-shot
    /// timers become inactive after firing; unregistering also deactivates.
    /// </summary>
    public bool IsActive => _active;

    /// <summary>
    /// Marks the timer active or inactive. Called by the timer device on
    /// registration and unregistration; activating reloads the full timeout.
    /// </summary>
    internal void SetActive(bool active)
    {
        if (active)
        {
            _remainingNs = TimeoutNs;
        }

        _active = active;
    }

    /// <summary>
    /// Advances the timer by the elapsed tick duration. Called by the timer
    /// device on each hardware tick.
    /// </summary>
    /// <param name="elapsedNs">Nanoseconds elapsed since the previous tick.</param>
    /// <returns>True when the timer is due; recurring timers reload automatically.</returns>
    internal bool Tick(ulong elapsedNs)
    {
        if (_remainingNs > elapsedNs)
        {
            _remainingNs -= elapsedNs;
            return false;
        }

        _remainingNs = TimeoutNs;
        return true;
    }

    /// <summary>
    /// Invokes the timer callback. Called by the timer device when the timer is due.
    /// </summary>
    internal void Invoke()
    {
        _callback();
    }
}
