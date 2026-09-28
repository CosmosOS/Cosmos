// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A thread a driver started through <see cref="DeviceBinding.TryStartThread"/>.
/// Its body loops on the binding's events until <see cref="DeviceBinding.IsDetaching"/>
/// turns true, then returns; teardown joins it with a bounded wait.
/// </summary>
internal sealed class DriverThread
{
    private readonly SchedulerThread _thread;

    internal DriverThread(string name, SchedulerThread thread)
    {
        Name = name;
        _thread = thread;
    }

    /// <summary>The name the driver gave the thread, for the log.</summary>
    public string Name { get; }

    /// <summary>True once the thread's body returned.</summary>
    public bool HasExited => _thread.State == SchedulerThreadState.Dead;

    /// <summary>True when the calling code runs on this thread.</summary>
    internal bool IsCurrent => ReferenceEquals(KitTime.CurrentThread, _thread);

    /// <summary>
    /// Waits for the thread to exit. Teardown only, from the worker.
    /// </summary>
    /// <param name="timeoutMilliseconds">Longest time to wait.</param>
    /// <returns>True when the thread exited in time.</returns>
    internal bool TryJoin(uint timeoutMilliseconds)
    {
        long deadline = KitTime.DeadlineAfter(timeoutMilliseconds);
        while (!HasExited)
        {
            if (KitTime.HasPassed(deadline))
            {
                return false;
            }

            KitTime.Sleep(1);
        }

        return true;
    }
}
