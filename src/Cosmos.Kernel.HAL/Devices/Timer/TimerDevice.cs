// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Timer;

/// <summary>
/// Abstract base class for all timer devices. Maintains the software timer
/// registry that is advanced on each hardware tick of the device.
/// </summary>
internal abstract class TimerDevice : Device, ITimerDevice
{
    /// <summary>Nanoseconds in one millisecond.</summary>
    protected const ulong NanosecondsPerMillisecond = 1_000_000;

    private readonly List<SoftwareTimer> _timers = new();

    /// <summary>
    /// Timers found due by the current <see cref="HandleTick"/>, so their
    /// callbacks run after the registry walk rather than inside it. Kept at
    /// least as long as the registry by <see cref="RegisterTimer"/>, because
    /// one tick can find every registered timer due and the tick path must not
    /// allocate.
    /// </summary>
    private SoftwareTimer?[] _dueTimers = new SoftwareTimer?[4];

    /// <summary>
    /// Event handler for timer tick events.
    /// </summary>
    public TimerTickHandler? OnTick { get; set; }

    /// <summary>
    /// Initialize the timer device.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Gets the timer frequency in Hz.
    /// </summary>
    public abstract uint Frequency { get; }

    /// <summary>
    /// Sets the timer frequency in Hz.
    /// </summary>
    /// <param name="frequency">Frequency in Hz.</param>
    /// <returns>
    /// True when the device accepted the frequency; false when it is outside
    /// what the device can divide to, in which case the tick is unchanged.
    /// </returns>
    public abstract bool SetFrequency(uint frequency);

    /// <summary>
    /// Registers a software timer driven by this device's periodic tick.
    /// The timer's callback runs in interrupt context and must not block.
    /// </summary>
    /// <param name="timer">Timer to register.</param>
    public virtual void RegisterTimer(SoftwareTimer timer)
    {
        if (timer == null || timer.IsActive)
        {
            return;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            if (_dueTimers.Length <= _timers.Count)
            {
                _dueTimers = new SoftwareTimer?[(_timers.Count + 1) * 2];
            }

            timer.SetActive(true);
            _timers.Add(timer);
        }
    }

    /// <summary>
    /// Unregisters a previously registered software timer.
    /// </summary>
    /// <param name="timer">Timer to unregister.</param>
    /// <returns>
    /// True when the timer was registered and has been removed; false when it
    /// is null, had already fired, or was already unregistered.
    /// </returns>
    public virtual bool UnregisterTimer(SoftwareTimer timer)
    {
        if (timer == null)
        {
            return false;
        }

        using (InternalCpu.DisableInterruptsScope())
        {
            // ReferenceEquals scan (not List.Remove) to match the kernel
            // convention of avoiding EqualityComparer<T>.Default.
            for (int i = 0; i < _timers.Count; i++)
            {
                if (ReferenceEquals(_timers[i], timer))
                {
                    timer.SetActive(false);
                    _timers.RemoveAt(i);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Advances all registered software timers and raises <see cref="OnTick"/>.
    /// Called by the driver's tick interrupt handler with the elapsed tick duration.
    /// </summary>
    /// <param name="elapsedNs">Nanoseconds elapsed since the previous tick.</param>
    protected void HandleTick(ulong elapsedNs)
    {
        // Collect first, invoke second. A callback may call RegisterTimer or
        // UnregisterTimer, and both mutate _timers: re-indexing a list a
        // callback has shrunk ticks one timer twice, and walks off the end
        // outright once a callback cancels two.
        int dueCount = 0;

        for (int i = _timers.Count - 1; i >= 0 && dueCount < _dueTimers.Length; i--)
        {
            SoftwareTimer timer = _timers[i];

            if (!timer.Tick(elapsedNs))
            {
                continue;
            }

            if (!timer.Recurring)
            {
                timer.SetActive(false);
                _timers.RemoveAt(i);
            }

            _dueTimers[dueCount++] = timer;
        }

        for (int i = 0; i < dueCount; i++)
        {
            SoftwareTimer? timer = _dueTimers[i];
            _dueTimers[i] = null;

            // A recurring timer that an earlier callback in this batch
            // cancelled must not fire. A one-shot is already off the registry
            // and committed to this firing.
            if (timer is null || (timer.Recurring && !timer.IsActive))
            {
                continue;
            }

            timer.Invoke();
        }

        OnTick?.Invoke();
    }

    /// <summary>
    /// Blocks for the specified number of milliseconds by waiting for a
    /// one-shot software timer to fire. Requires the device tick to be running.
    /// </summary>
    /// <param name="ms">Milliseconds to wait.</param>
    public virtual void Wait(uint ms)
    {
        SoftwareTimer timer = new(static () => { }, ms * NanosecondsPerMillisecond, recurring: false);
        RegisterTimer(timer);

        while (timer.IsActive)
        {
            PlatformHAL.CpuOps?.Halt();
        }
    }
}
