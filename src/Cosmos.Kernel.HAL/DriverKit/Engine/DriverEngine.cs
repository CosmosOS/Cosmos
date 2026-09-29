// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Scheduler;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.Firmware;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// The kit's one execution engine: a FIFO queue of jobs (a node arrived, a
/// node was retracted, a work item is due) and the worker thread that runs
/// them one at a time. Probes, teardowns and work items therefore never
/// overlap. Without a scheduler the engine runs inline: the thread that
/// publishes or retracts drains the queue itself, and a probe that publishes
/// a child leaves it for the drain that is already running. A caller waits
/// for its own job, or for a fence queued behind everything pending, never
/// for the queue to be empty: periodic work would keep that from ever being
/// true. Every wait on the engine watches the worker and panics if it died,
/// rather than hang.
/// </summary>
internal static class DriverEngine
{
    /// <summary>How long a waiter parks between looks at the worker's health.</summary>
    private const uint PollMilliseconds = 100;

    private static SchedSpinLock s_queueLock;
    private static EngineJob? s_head;
    private static EngineJob? s_tail;
    private static readonly InterruptEvent s_wake = new();

    /// <summary>
    /// How many node jobs (arrivals and retractions) were ever queued, so a
    /// waiter can tell whether the jobs its fence covered queued more. Work
    /// item jobs are not counted: periodic items would keep it moving.
    /// Written under <see cref="s_queueLock"/>.
    /// </summary>
    private static int s_nodeJobsQueued;

    private static SchedulerThread? s_worker;
    private static EngineMode s_mode;
    private static bool s_draining;

    private static SchedSpinLock s_nodesLock;
    private static DeviceNode[] s_nodes = [];

    /// <summary>True once <see cref="Start"/> ran.</summary>
    public static bool IsStarted => s_mode != EngineMode.NotStarted;

    /// <summary>True when a worker thread runs the jobs; false before start and in inline mode.</summary>
    public static bool HasWorker => s_mode == EngineMode.Worker;

    /// <summary>Every node ever published, in publication order, retracted ones included. A snapshot.</summary>
    public static IReadOnlyList<DeviceNode> Nodes => s_nodes;

    /// <summary>True when the calling code runs on the worker thread.</summary>
    internal static bool IsOnWorker => s_worker is not null && ReferenceEquals(KitTime.CurrentThread, s_worker);

    /// <summary>
    /// Starts the engine: logs the manifest, publishes the firmware display
    /// when the bootloader handed one over, starts the worker (or settles
    /// for inline mode), and offers every node published so far. Returns
    /// once every node, the children a bus driver published from its probe
    /// included, has been offered. Called once from the kernel start path.
    /// </summary>
    public static void Start()
    {
        if (IsStarted)
        {
            return;
        }

        DriverLog.Manifest(DriverRegistry.Drivers);

        // Before the worker exists and before any offer runs, so the
        // retirement rule sees the firmware display when a driver binds the
        // function holding it. A consumer that throws costs the kit the
        // firmware display, not the boot: PublishFirmware has withdrawn it.
        // Behind the graphics switch, so a kernel without graphics carries
        // no firmware display type at all.
        if (CosmosFeatures.GraphicsEnabled)
        {
            if (BootFirmware.BootDisplay is { } display)
            {
                try
                {
                    PublishedDevice device = DeviceRegistry.PublishFirmware(DeviceKind.Display, display.Name, display);
                    DriverLog.FirmwarePublished(device);
                }
                catch (Exception exception)
                {
                    DriverLog.EngineError(exception.Message);
                }
            }
        }

        EngineMode mode = EngineMode.Inline;
        if (CosmosFeatures.SchedulerEnabled && KernelThread.TryStart(WorkerMain, out SchedulerThread? worker))
        {
            s_worker = worker;
            mode = EngineMode.Worker;
        }

        s_mode = mode;
        DriverLog.Started(HasWorker);
        WaitForQueuedJobs();
    }

    /// <summary>
    /// Inline mode: runs every queued job on the calling thread. Returns at
    /// once inside a drain, so a probe that publishes a child does not run
    /// the child's offer from inside its own.
    /// </summary>
    public static void RunPending()
    {
        if (s_draining)
        {
            return;
        }

        s_draining = true;
        try
        {
            while (TryDequeue(out EngineJob? job))
            {
                Execute(job);
            }
        }
        finally
        {
            s_draining = false;
        }
    }

    /// <summary>
    /// Returns once every job queued before the call has run, and every
    /// node job those jobs queued in turn: a fence goes to the back of the
    /// queue and the caller waits for it; when a job that ran ahead of the
    /// fence published or retracted a node, another fence follows, until one
    /// completes with no node job queued while it was in flight. So work a
    /// test or a bus just caused (an offer, the children a bus driver's
    /// probe published, a teardown, a work item a handler scheduled) is
    /// complete on return, while work queued afterwards that is not a node
    /// job, such as periodic items, does not hold it up. From the worker or
    /// inside a drain it returns at once, since waiting there would wait for
    /// itself; before <see cref="Start"/> nothing runs, so it returns too.
    /// Inline mode drains once: the loop already running picks nested jobs
    /// up itself.
    /// </summary>
    public static void WaitForQueuedJobs()
    {
        if (!IsStarted || IsOnWorker || s_draining)
        {
            return;
        }

        if (!HasWorker)
        {
            RunPending();
            return;
        }

        int seen;
        do
        {
            seen = Volatile.Read(ref s_nodeJobsQueued);
            if (!Submit(new EngineJob(EngineJobKind.Fence) { Completion = new InterruptEvent() }))
            {
                // Released early: the caller's own binding is being torn
                // down, and the teardown waits to join the caller.
                return;
            }
        }
        while (Volatile.Read(ref s_nodeJobsQueued) != seen);
    }

    /// <summary>
    /// Puts a node in the tree and queues its offer. Off the worker, after
    /// start, returns once the node was offered; from the worker, inside a
    /// drain, or before start, returns at once and the offer follows. A
    /// driver thread whose own binding starts detaching while it waits is
    /// released early, with the offer still queued: the teardown would
    /// otherwise wait to join a thread that waits on the teardown.
    /// </summary>
    /// <param name="node">A pending node.</param>
    /// <param name="owner">The binding publishing the node, when a driver does; null for a bus.</param>
    internal static void PublishNode(DeviceNode node, DeviceBinding? owner = null)
    {
        using (s_nodesLock.AcquireIrqSafe())
        {
            DeviceNode[] nodes = new DeviceNode[s_nodes.Length + 1];
            Array.Copy(s_nodes, nodes, s_nodes.Length);
            nodes[s_nodes.Length] = node;
            s_nodes = nodes;
        }

        Submit(new EngineJob(EngineJobKind.NodeArrived) { Node = node, Completion = NewCompletion() }, owner);
    }

    /// <summary>
    /// Queues a node's teardown. Same completion and early-release rules as
    /// <see cref="PublishNode"/>.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="hardwarePresent">Whether the hardware is still there to be quiesced.</param>
    /// <param name="owner">The binding retracting the node, when a driver does; null for a bus.</param>
    /// <exception cref="InvalidOperationException">Called from one of the node's own driver threads, which the teardown would have to join.</exception>
    internal static void RetractNode(DeviceNode node, bool hardwarePresent, DeviceBinding? owner = null)
    {
        if (node.Binding is { } binding && binding.IsCurrentThreadOwned)
        {
            throw new InvalidOperationException("A node cannot be retracted from its own driver's thread.");
        }

        Submit(new EngineJob(EngineJobKind.NodeRetracted) { Node = node, HardwarePresent = hardwarePresent, Completion = NewCompletion() }, owner);
    }

    /// <summary>Queues a work item's job. Allocation-free; any context.</summary>
    /// <param name="job">The item's own job.</param>
    /// <returns>False without a worker, or when the job is already queued.</returns>
    internal static bool TryEnqueueWorkItem(EngineJob job)
    {
        if (!HasWorker)
        {
            return false;
        }

        using (s_queueLock.AcquireIrqSafe())
        {
            if (job.IsQueued)
            {
                return false;
            }

            EnqueueLocked(job);
        }

        s_wake.Signal();
        return true;
    }

    /// <summary>Takes a job out of the queue if it is there. Any context.</summary>
    /// <param name="job">The job.</param>
    internal static void Dequeue(EngineJob job)
    {
        using (s_queueLock.AcquireIrqSafe())
        {
            if (!job.IsQueued)
            {
                return;
            }

            EngineJob? previous = null;
            EngineJob? current = s_head;
            while (current is not null && !ReferenceEquals(current, job))
            {
                previous = current;
                current = current.Next;
            }

            if (current is null)
            {
                job.IsQueued = false;
                return;
            }

            if (previous is null)
            {
                s_head = current.Next;
            }
            else
            {
                previous.Next = current.Next;
            }

            if (ReferenceEquals(s_tail, current))
            {
                s_tail = previous;
            }

            current.Next = null;
            current.IsQueued = false;
        }
    }

    /// <summary>
    /// Tears a node down now, on the calling job: its binding (children
    /// first, inside it), then the node is marked retracted, the bus's
    /// after-teardown hook runs when the node had a binding, and the bus's
    /// allocation for it is released unless resources leaked. Worker or
    /// drain only.
    /// </summary>
    internal static void TeardownNode(DeviceNode node, DetachCause cause, bool hardwarePresent)
    {
        if (node.State == NodeState.Retracted)
        {
            return;
        }

        try
        {
            node.Binding?.Teardown(new DetachReason(cause, hardwarePresent));
        }
        finally
        {
            // Retracted whatever the teardown managed: a node left pending
            // with a half-dead binding would be offered or torn down again.
            node.State = NodeState.Retracted;

            // Even when resources leaked: a function whose driver thread is
            // stuck still has its bus mastering turned off. A node nobody
            // bound is left alone: a legacy driver may operate its function.
            if (node.Binding is not null && node.AccessObject is INodeHooks hooks)
            {
                try
                {
                    hooks.AfterTeardown(hardwarePresent);
                }
                catch (Exception exception)
                {
                    DriverLog.HookThrew(node, "after teardown", exception.Message);
                }
            }

            if (node.LeakedResourceCount == 0)
            {
                node.BusResource?.Release();
            }

            DriverLog.Retracted(node);
        }
    }

    private static InterruptEvent? NewCompletion() => IsStarted && HasWorker && !IsOnWorker ? new InterruptEvent() : null;

    /// <summary>Queues a job and, when it carries a completion, waits for it.</summary>
    /// <param name="job">The job.</param>
    /// <param name="owner">The binding submitting it, when a driver does; null otherwise.</param>
    /// <returns>False when the wait was given up because the caller's own binding is being torn down; true otherwise, for a job nobody waits on too.</returns>
    private static bool Submit(EngineJob job, DeviceBinding? owner = null)
    {
        using (s_queueLock.AcquireIrqSafe())
        {
            EnqueueLocked(job);
            if (job.Kind is EngineJobKind.NodeArrived or EngineJobKind.NodeRetracted)
            {
                s_nodeJobsQueued++;
            }
        }

        s_wake.Signal();

        if (job.Completion is not null)
        {
            while (!job.Completion.Wait(PollMilliseconds))
            {
                PanicIfWorkerDied();
                if (IsCallerDetaching(owner))
                {
                    // The worker is tearing down the caller's own binding and
                    // will wait to join this thread; the job stays queued and
                    // runs afterwards, against a node that is then retracted.
                    return false;
                }
            }

            return true;
        }

        if (IsStarted && !HasWorker)
        {
            RunPending();
        }

        return true;
    }

    /// <summary>
    /// True when the calling thread belongs to a binding that is being torn
    /// down: the one given, or, for a call that names none, any bound node's.
    /// </summary>
    private static bool IsCallerDetaching(DeviceBinding? owner)
    {
        if (owner is not null)
        {
            return owner.IsDetaching;
        }

        IReadOnlyList<DeviceNode> nodes = s_nodes;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Binding is { IsDetaching: true } binding && binding.IsCurrentThreadOwned)
            {
                return true;
            }
        }

        return false;
    }

    private static void EnqueueLocked(EngineJob job)
    {
        if (job.IsQueued)
        {
            return;
        }

        job.IsQueued = true;
        job.Next = null;
        if (s_tail is null)
        {
            s_head = job;
        }
        else
        {
            s_tail.Next = job;
        }

        s_tail = job;
    }

    private static bool TryDequeue([NotNullWhen(true)] out EngineJob? job)
    {
        using (s_queueLock.AcquireIrqSafe())
        {
            job = s_head;
            if (job is null)
            {
                return false;
            }

            s_head = job.Next;
            if (s_head is null)
            {
                s_tail = null;
            }

            job.Next = null;
            job.IsQueued = false;
            return true;
        }
    }

    private static void WorkerMain()
    {
        // Start() assigns the mode after the thread began; jobs run only once
        // the rest of the engine knows the worker exists.
        while (!IsStarted)
        {
            KitTime.Sleep(1);
        }

        while (true)
        {
            if (TryDequeue(out EngineJob? job))
            {
                Execute(job);
            }
            else
            {
                s_wake.Wait();
            }
        }
    }

    /// <summary>Runs one job. Nothing escapes: the worker survives every driver.</summary>
    private static void Execute(EngineJob job)
    {
        try
        {
            switch (job.Kind)
            {
                case EngineJobKind.NodeArrived:
                    Arbitration.Offer(job.Node!);
                    break;
                case EngineJobKind.NodeRetracted:
                    TeardownNode(job.Node!, DetachCause.Retracted, job.HardwarePresent);
                    break;
                case EngineJobKind.RunWorkItem:
                    RunWorkItem(job.Item!);
                    break;
                default:
                    // A fence: its completion below is the whole job.
                    break;
            }
        }
        catch (Exception exception)
        {
            DriverLog.EngineError(exception.Message);
        }
        finally
        {
            job.Completion?.Signal();
        }
    }

    private static void RunWorkItem(WorkItem item)
    {
        try
        {
            item.Run();
        }
        catch (Exception exception)
        {
            if (item.Binding is { } binding)
            {
                DriverLog.WorkItemThrew(binding.Node, binding.Driver, exception.Message);
            }
            else
            {
                DriverLog.EngineError(exception.Message);
            }

            item.Cancel();
        }
    }

    private static void PanicIfWorkerDied()
    {
        if (s_worker is not null && s_worker.State == SchedulerThreadState.Dead)
        {
            Panic.Halt("[Drivers] the worker thread exited; nothing can run the driver stage.");
        }
    }
}
