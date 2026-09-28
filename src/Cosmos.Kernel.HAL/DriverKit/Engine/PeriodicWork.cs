// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// A work item the platform timer schedules at a fixed interval, set up by
/// <see cref="DeviceBinding.TrySchedulePeriodic"/>. The software timer's
/// callback runs in the timer interrupt and only queues the item, which is
/// allocation-free; the item itself runs on the worker.
/// </summary>
internal sealed class PeriodicWork
{
    private const ulong NanosecondsPerMillisecond = 1_000_000;

    private readonly ITimerDevice _timer;
    private readonly SoftwareTimer _softwareTimer;
    private readonly WorkItem _item;

    internal PeriodicWork(ITimerDevice timer, WorkItem item, uint intervalMilliseconds)
    {
        _timer = timer;
        _item = item;
        _softwareTimer = new SoftwareTimer(Fire, intervalMilliseconds * NanosecondsPerMillisecond, recurring: true);
    }

    /// <summary>Registers the timer; the first firing is one interval away.</summary>
    internal void Start() => _timer.RegisterTimer(_softwareTimer);

    /// <summary>Unregisters the timer. Teardown only.</summary>
    internal void Cancel() => _timer.UnregisterTimer(_softwareTimer);

    private void Fire() => _item.Schedule();
}
