// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Boot;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Timers;

/// <summary>
/// The platform's tick source: the hardware timer whose periodic interrupt
/// advances the software timer registry it keeps (the PIT on x64, the
/// generic timer on ARM64). The machine description creates the one
/// instance at boot, before the driver kit runs, which is why it is
/// platform code rather than a device the kit publishes.
/// </summary>
internal abstract class TickSource
{
    /// <summary>Nanoseconds in one millisecond.</summary>
    protected const ulong NanosecondsPerMillisecond = 1_000_000;

    /// <summary>Slots the registry and the due array start with; both double when the registry is full.</summary>
    private const int InitialCapacity = 4;

    /// <summary>
    /// The registry's lock, IRQ-safe: the tick walks the registry from its
    /// interrupt on the boot CPU while a thread on any CPU registers or
    /// cancels a timer (SMP phase 4b, CosmosOS/Cosmos#3347). Nothing
    /// allocates under it: a registration builds the arrays it may need
    /// before taking it.
    /// </summary>
    private SchedSpinLock _lock;

    /// <summary>
    /// The registered timers, in the first <see cref="_count"/> slots. An
    /// array rather than a list, so that growing it is an allocation made
    /// outside the lock and a copy made under it.
    /// </summary>
    private TimerEntry?[] _timers = new TimerEntry?[InitialCapacity];

    /// <summary>Registered timers; the slots of <see cref="_timers"/> from it on are null.</summary>
    private int _count;

    /// <summary>
    /// Timers found due by the current <see cref="HandleTick"/>, so their
    /// callbacks run after the registry walk rather than inside it. Grown
    /// with the registry, to the same length, because one tick can find
    /// every registered timer due and the tick path must not allocate.
    /// </summary>
    private TimerEntry?[] _dueTimers = new TimerEntry?[InitialCapacity];

    /// <summary>
    /// Event handler for timer tick events.
    /// </summary>
    public TimerTickHandler? OnTick { get; set; }

    /// <summary>
    /// Initialize the tick source.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Gets the timer frequency in Hz.
    /// </summary>
    public abstract uint Frequency { get; }

    /// <summary>
    /// Sets the timer frequency in Hz. Tick sources divide a fixed input clock,
    /// so each has a range it can express and rejects the rest.
    /// </summary>
    /// <param name="frequency">Frequency in Hz.</param>
    /// <returns>
    /// True when the hardware accepted the frequency; false when it is outside
    /// what the hardware can divide to, in which case the tick is unchanged.
    /// </returns>
    public abstract bool SetFrequency(uint frequency);

    /// <summary>
    /// Registers a software timer driven by this source's periodic tick.
    /// The timer's callback runs in interrupt context and must not block.
    /// Any thread, on any CPU.
    /// </summary>
    /// <param name="timer">Timer to register.</param>
    public virtual void RegisterTimer(TimerEntry timer)
    {
        if (timer.IsActive)
        {
            return;
        }

        // A full registry grows outside the lock, from the count read under
        // it, and the registration is retried: nothing allocates under a
        // spinlock.
        bool registered = false;
        while (!registered)
        {
            int count;
            using (_lock.AcquireIrqSafe())
            {
                count = _count;
                if (count < _timers.Length)
                {
                    timer.SetActive(true);
                    _timers[count] = timer;
                    _count = count + 1;
                    registered = true;
                }
            }

            if (!registered)
            {
                Grow(count);
            }
        }
    }

    /// <summary>
    /// Doubles the registry and the due array past <paramref name="count"/>
    /// entries: both are built outside the lock and installed under it, the
    /// registry copied. Two registrars growing at once each build a pair at
    /// least as large as the count they saw; the larger pair is installed,
    /// the other is garbage.
    /// </summary>
    /// <param name="count">The registered count the caller found the registry full at.</param>
    private void Grow(int count)
    {
        int length = (count + 1) * 2;
        TimerEntry?[] timers = new TimerEntry?[length];
        TimerEntry?[] due = new TimerEntry?[length];
        using (_lock.AcquireIrqSafe())
        {
            if (_timers.Length < length)
            {
                Array.Copy(_timers, timers, _count);
                _timers = timers;
                _dueTimers = due;
            }
        }
    }

    /// <summary>
    /// Unregisters a previously registered software timer. Any thread, on
    /// any CPU, and a timer callback.
    /// </summary>
    /// <param name="timer">Timer to unregister.</param>
    /// <returns>
    /// True when the timer was registered and has been removed; false when it
    /// had already fired or was already unregistered.
    /// </returns>
    public virtual bool UnregisterTimer(TimerEntry timer)
    {
        using (_lock.AcquireIrqSafe())
        {
            // ReferenceEquals scan to match the kernel convention of avoiding
            // EqualityComparer<T>.Default.
            for (int i = 0; i < _count; i++)
            {
                if (ReferenceEquals(_timers[i], timer))
                {
                    timer.SetActive(false);
                    RemoveAt(i);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>Takes the timer at <paramref name="index"/> out of the registry, moving the ones after it down a slot. The caller holds the lock.</summary>
    private void RemoveAt(int index)
    {
        _count--;
        Array.Copy(_timers, index + 1, _timers, index, _count - index);
        _timers[_count] = null;
    }

    /// <summary>
    /// Advances all registered software timers and raises <see cref="OnTick"/>.
    /// Called by the subclass's interrupt handler with the elapsed tick duration.
    /// </summary>
    /// <param name="elapsedNs">Nanoseconds elapsed since the previous tick.</param>
    protected void HandleTick(ulong elapsedNs)
    {
        // Collect under the lock, invoke outside it. A callback may call
        // RegisterTimer or UnregisterTimer, which take the lock (it is not
        // re-entrant) and mutate the registry: re-indexing a registry a
        // callback has shrunk ticks one timer twice, and walks off the end
        // outright once a callback cancels two. The due array is captured
        // under the lock, because a registration on another CPU may install
        // a larger one before the callbacks run; it is as long as the
        // registry, so every registered timer fits.
        TimerEntry?[] due;
        int dueCount = 0;

        using (_lock.AcquireIrqSafe())
        {
            due = _dueTimers;
            for (int i = _count - 1; i >= 0; i--)
            {
                TimerEntry timer = _timers[i]!;

                if (!timer.Tick(elapsedNs))
                {
                    continue;
                }

                if (!timer.Recurring)
                {
                    timer.SetActive(false);
                    RemoveAt(i);
                }

                due[dueCount++] = timer;
            }
        }

        for (int i = 0; i < dueCount; i++)
        {
            TimerEntry? timer = due[i];
            due[i] = null;

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
    /// one-shot software timer to fire. Requires the tick to be running.
    /// </summary>
    /// <param name="ms">Milliseconds to wait.</param>
    public virtual void Wait(uint ms)
    {
        TimerEntry timer = new(static () => { }, ms * NanosecondsPerMillisecond, recurring: false);
        RegisterTimer(timer);

        while (timer.IsActive)
        {
            PlatformHAL.CpuOps?.Halt();
        }
    }
}
