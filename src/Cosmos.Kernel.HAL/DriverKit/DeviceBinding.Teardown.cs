// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The teardown half of the binding: the fixed order in which the kit takes
/// back what the driver acquired, for a device going away and for a probe
/// that declined or failed.
/// </summary>
public sealed partial class DeviceBinding
{
    /// <summary>
    /// Tears the binding down for a device going away. Worker only. The
    /// order: the flag first, so nothing new is acquired; child nodes;
    /// published devices; interrupts; USB pipes; deferred work, events and
    /// threads;
    /// <see cref="Driver.OnDetach"/>; then memory, in reverse order of
    /// acquisition. A thread that does not stop in
    /// <see cref="JoinTimeoutMilliseconds"/> keeps the memory it may still
    /// touch: those resources leak on purpose and the node says how many.
    /// A step that throws is logged and the walk goes on, so a misbehaving
    /// consumer or source never leaves a handler connected or memory held.
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
        UsbPipeResource[] pipes;
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
            pipes = _pipes.ToArray();
            workItems = _workItems.ToArray();
            kitItems = _kitItems.ToArray();
            periodic = _periodic.ToArray();
            events = _events.ToArray();
            threads = _threads.ToArray();
            memory = _memory.ToArray();
        }

        int leakedCount = memory.Length + (Node.BusResource is null ? 0 : 1);

        // 1. Children, leaf first: each child's own teardown runs its children first.
        for (int i = 0; i < children.Length; i++)
        {
            try
            {
                DriverEngine.TeardownNode(children[i], DetachCause.ParentRetracted, reason.HardwarePresent);
            }
            catch (Exception exception)
            {
                DriverLog.TeardownStepThrew(Node, Driver, "child teardown", exception.Message);
            }
        }

        // 1b. A probe's unwind leaves the node alive for the next candidate:
        //     the children it published are off the node, so Children
        //     names only nodes still in the tree. A teardown keeps them on
        //     the dead parent.
        if (!runOnDetach)
        {
            using (_lock.AcquireIrqSafe())
            {
                for (int i = 0; i < children.Length; i++)
                {
                    Node.RemoveChild(children[i]);
                }
            }
        }

        // 2. Published devices: consumers are told, sinks go quiet.
        for (int i = 0; i < devices.Length; i++)
        {
            try
            {
                DeviceRegistry.Withdraw(devices[i]);
                DriverLog.Withdrew(Node, Driver, devices[i]);
            }
            catch (Exception exception)
            {
                DriverLog.TeardownStepThrew(Node, Driver, "withdraw", exception.Message);
            }
        }

        // 3. Interrupts: masked and disconnected, handlers never run again.
        for (int i = 0; i < handles.Length; i++)
        {
            try
            {
                handles[i].Disconnect();
            }
            catch (Exception exception)
            {
                DriverLog.TeardownStepThrew(Node, Driver, "interrupt disconnect", exception.Message);
            }
        }

        // 3b. Pipes: stopped and dropped on the controller while the ring they
        //     point at is still allocated; a waiter on one wakes with Stopped.
        for (int i = 0; i < pipes.Length; i++)
        {
            try
            {
                pipes[i].Release();
            }
            catch (Exception exception)
            {
                DriverLog.TeardownStepThrew(Node, Driver, "pipe close", exception.Message);
            }
        }

        // 4. Deferred work stops, every waiter wakes, threads are joined.
        DetachEvent.Cancel();
        for (int i = 0; i < periodic.Length; i++)
        {
            try
            {
                periodic[i].Cancel();
            }
            catch (Exception exception)
            {
                DriverLog.TeardownStepThrew(Node, Driver, "periodic cancel", exception.Message);
            }
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
                DriverLog.ThreadDidNotStop(Node, Driver, threads[i].Name, JoinTimeoutMilliseconds, leakedCount);
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
            Node.LeakedResourceCount = leakedCount;
        }
        else
        {
            for (int i = memory.Length - 1; i >= 0; i--)
            {
                try
                {
                    memory[i].Release();
                }
                catch (Exception exception)
                {
                    DriverLog.TeardownStepThrew(Node, Driver, "release", exception.Message);
                }
            }
        }

        return held;
    }
}
