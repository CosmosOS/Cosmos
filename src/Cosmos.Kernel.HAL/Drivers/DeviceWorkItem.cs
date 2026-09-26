// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.Engine;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// Work a driver's interrupt handler hands to thread context. The callback
/// runs on the kit's <c>driver-work</c> thread, where it may allocate,
/// block, wait on a <see cref="DeviceEvent"/> and deliver to the kernel, so
/// the handler itself only schedules it. Created during Probe through
/// <see cref="DeviceContext.TryCreateWorkItem"/> and owned by the binding:
/// scheduled during Probe, it runs once Probe returned Bound, and it never
/// runs if the attempt is declined or fails.
/// </summary>
internal sealed class DeviceWorkItem
{
    private readonly DeviceContext _context;
    private readonly Action _callback;

    // Both guarded by DriverWorkQueue's lock, which Schedule takes from
    // interrupt context as well as from threads.
    private WorkItemState _state = WorkItemState.Held;
    private bool _pending;

    /// <summary>
    /// What the item may do. It starts <see cref="Held"/> while Probe
    /// runs, and leaves that state once, for good.
    /// </summary>
    private enum WorkItemState
    {
        /// <summary>The binding's Probe is running: a Schedule is remembered, not queued.</summary>
        Held,

        /// <summary>The binding is Bound: a Schedule queues the item for the driver-work thread.</summary>
        Armed,

        /// <summary>The attempt was torn down, or the callback threw: Schedule refuses.</summary>
        Disarmed
    }

    /// <summary>The next item in the driver-work queue. Guarded by the queue's lock.</summary>
    internal DeviceWorkItem? NextQueued { get; set; }

    internal DeviceWorkItem(DeviceContext context, Action callback)
    {
        _context = context;
        _callback = callback;
    }

    /// <summary>
    /// Asks for the callback to run once on the driver-work thread. IRQ-safe:
    /// it allocates nothing and takes an IRQ-safe lock only, so the
    /// driver's interrupt handler may call it. Scheduling again while the
    /// item waits to run does nothing; once the callback has started, a new
    /// Schedule runs it again after it returns.
    /// </summary>
    /// <returns>
    /// True when the item is now waiting to run. False when it already was,
    /// or when it can never run again: its binding attempt was declined or
    /// failed, or its callback threw.
    /// </returns>
    public bool Schedule()
    {
        bool queued;
        using (DriverWorkQueue.AcquireLock())
        {
            if (_state == WorkItemState.Disarmed || _pending)
            {
                return false;
            }

            _pending = true;
            queued = _state == WorkItemState.Armed;
            if (queued)
            {
                DriverWorkQueue.EnqueueLocked(this);
            }
        }

        // Woken outside the lock: the wake-up readies the thread through the
        // scheduler, which has no business running under a driver's lock.
        if (queued)
        {
            DriverWorkQueue.Wake();
        }

        return true;
    }

    /// <summary>
    /// Called when the binding's Probe returned Bound: from now on a
    /// Schedule queues the item, and one made during Probe is queued now.
    /// </summary>
    internal void Arm()
    {
        bool queued;
        using (DriverWorkQueue.AcquireLock())
        {
            if (_state != WorkItemState.Held)
            {
                return;
            }

            _state = WorkItemState.Armed;
            queued = _pending;
            if (queued)
            {
                DriverWorkQueue.EnqueueLocked(this);
            }
        }

        if (queued)
        {
            DriverWorkQueue.Wake();
        }
    }

    /// <summary>
    /// Called when the binding attempt is torn down: a Schedule made during
    /// Probe is forgotten, and the item never runs.
    /// </summary>
    internal void Drop()
    {
        using (DriverWorkQueue.AcquireLock())
        {
            _state = WorkItemState.Disarmed;
            _pending = false;
        }
    }

    /// <summary>
    /// Called by the driver-work thread, holding the queue's lock, for an
    /// item it just took off the queue: the item is no longer pending, so
    /// a Schedule from here on queues it again.
    /// </summary>
    /// <returns>True when the callback should run; false for an item disarmed while it was queued.</returns>
    internal bool TakeForRunLocked()
    {
        _pending = false;
        return _state == WorkItemState.Armed;
    }

    /// <summary>
    /// Runs the callback on the driver-work thread. An exception is logged
    /// with the driver's name and the device's path, and disarms the item:
    /// a callback that failed once is left to fail no further.
    /// </summary>
    internal void Run()
    {
        try
        {
            _callback();
        }
        catch (Exception exception)
        {
            using (DriverWorkQueue.AcquireLock())
            {
                _state = WorkItemState.Disarmed;
                _pending = false;
            }

            _context.WriteLog($"work item threw and will not run again: {exception.Message}");
        }
    }
}
