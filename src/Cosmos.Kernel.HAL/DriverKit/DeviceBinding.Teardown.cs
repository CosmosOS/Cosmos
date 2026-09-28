// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The teardown half of the binding: the fixed order in which the kit takes
/// back what the driver acquired, for a device going away and for a probe
/// that declined or failed.
/// </summary>
internal sealed partial class DeviceBinding
{
    /// <summary>
    /// Tears the binding down for a device going away. Worker only. The
    /// order: the flag first, so nothing new is acquired; child nodes;
    /// published devices; interrupts; deferred work, events and threads;
    /// <see cref="Driver.OnDetach"/>; then memory, in reverse order of
    /// acquisition. A thread that does not stop in
    /// <see cref="JoinTimeoutMilliseconds"/> keeps the memory it may still
    /// touch: those resources leak on purpose and the node says how many.
    /// </summary>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    internal void Teardown(DetachReason reason) => Release(reason, runOnDetach: true);

    /// <summary>
    /// Releases everything a probe acquired before declining, failing or
    /// throwing. Same order as <see cref="Teardown"/> without the detach hook.
    /// Worker only.
    /// </summary>
    /// <returns>How many kit resources the probe had acquired.</returns>
    internal int Unwind() => Release(new DetachReason(DetachCause.Retracted, hardwarePresent: true), runOnDetach: false);

    private int Release(DetachReason reason, bool runOnDetach)
    {
        DeviceNode[] children;
        PublishedDevice[] devices;
        InterruptHandle[] handles;
        WorkItem[] workItems;
        WorkItem[] kitItems;
        PeriodicWork[] periodic;
        DeviceEvent[] events;
        DriverThread[] threads;
        IKitResource[] memory;
        int held;

        using (_lock.AcquireIrqSafe())
        {
            if (_detaching)
            {
                return 0;
            }

            _detaching = true;
            held = HeldResourceCount;
            children = _children.ToArray();
            devices = _devices.ToArray();
            handles = _handles.ToArray();
            workItems = _workItems.ToArray();
            kitItems = _kitItems.ToArray();
            periodic = _periodic.ToArray();
            events = _events.ToArray();
            threads = _threads.ToArray();
            memory = _memory.ToArray();
        }

        // 1. Children, leaf first: each child's own teardown runs its children first.
        for (int i = 0; i < children.Length; i++)
        {
            DriverEngine.TeardownNode(children[i], DetachCause.ParentRetracted, reason.HardwarePresent);
        }

        // 2. Published devices: consumers are told, sinks go quiet.
        for (int i = 0; i < devices.Length; i++)
        {
            DeviceRegistry.Withdraw(devices[i]);
            DriverLog.Withdrew(Node, Driver, devices[i]);
        }

        // 3. Interrupts: masked and disconnected, handlers never run again.
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i].Disconnect();
        }

        // 4. Deferred work stops, every waiter wakes, threads are joined.
        DetachEvent.Cancel();
        for (int i = 0; i < periodic.Length; i++)
        {
            periodic[i].Cancel();
        }

        for (int i = 0; i < workItems.Length; i++)
        {
            workItems[i].Cancel();
        }

        for (int i = 0; i < kitItems.Length; i++)
        {
            kitItems[i].Cancel();
        }

        for (int i = 0; i < events.Length; i++)
        {
            events[i].Cancel();
        }

        bool leaked = false;
        for (int i = 0; i < threads.Length; i++)
        {
            if (!threads[i].TryJoin(JoinTimeoutMilliseconds))
            {
                leaked = true;
                DriverLog.ThreadDidNotStop(Node, Driver, threads[i].Name, JoinTimeoutMilliseconds, memory.Length);
            }
        }

        // 5. The driver's one chance to quiesce the hardware.
        if (runOnDetach)
        {
            try
            {
                Driver.OnDetach(this, reason);
            }
            catch (Exception exception)
            {
                DriverLog.OnDetachThrew(Node, Driver, exception.Message);
            }
        }

        // 6. Memory, reverse order; kept when a thread may still touch it.
        if (leaked)
        {
            Node.LeakedResourceCount = memory.Length + (Node.BusResource is null ? 0 : 1);
        }
        else
        {
            for (int i = memory.Length - 1; i >= 0; i--)
            {
                memory[i].Release();
            }
        }

        return held;
    }
}
