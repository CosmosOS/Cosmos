// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Scheduler;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// The kit's one execution engine: a FIFO queue of jobs (a node arrived, a
/// node was retracted, a work item is due) and the worker thread that runs
/// them one at a time. Probes, teardowns and work items therefore never
/// overlap. Without a scheduler the engine runs inline: the thread that
/// publishes or retracts drains the queue itself, and a probe that publishes
/// a child leaves it for the drain that is already running. Every wait on
/// the engine watches the worker and panics if it died, rather than hang.
/// </summary>
internal static class DriverEngine
{
    /// <summary>How long a waiter parks between looks at the worker's health.</summary>
    private const uint PollMilliseconds = 100;

    private static SchedSpinLock s_queueLock;
    private static EngineJob? s_head;
    private static EngineJob? s_tail;
    private static bool s_running;
    private static int s_quiescenceWaiters;
    private static readonly InterruptEvent s_quiescent = new();
    private static readonly InterruptEvent s_wake = new();

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
    /// Starts the engine: logs the manifest, starts the worker (or settles
    /// for inline mode), and offers every node published so far. Returns once
    /// those offers are done. Called once from the kernel start path.
    /// </summary>
    public static void Start()
    {
        if (IsStarted)
        {
            return;
        }

        DriverLog.Manifest(DriverRegistry.Drivers);
        EngineMode mode = EngineMode.Inline;
        if (CosmosFeatures.SchedulerEnabled && KernelThread.TryStart(WorkerMain, out SchedulerThread? worker))
        {
            s_worker = worker;
            mode = EngineMode.Worker;
        }

        s_mode = mode;
        DriverLog.Started(HasWorker);
        WaitUntilQuiescent();
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
    /// Returns once the queue is empty and no job is running. From the worker
    /// or inside a drain it returns at once, since waiting there would wait
    /// for itself; before <see cref="Start"/> nothing runs, so it returns too.
    /// </summary>
    public static void WaitUntilQuiescent()
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

        while (true)
        {
            using (s_queueLock.AcquireIrqSafe())
            {
                if (s_head is null && !s_running)
                {
                    return;
                }

                s_quiescenceWaiters++;
            }

            if (!s_quiescent.Wait(PollMilliseconds))
            {
                using (s_queueLock.AcquireIrqSafe())
                {
                    if (s_quiescenceWaiters > 0)
                    {
                        s_quiescenceWaiters--;
                    }
                    else
                    {
                        // Signaled after the timeout: take the signal meant for us.
                        s_quiescent.TryConsume();
                    }
                }

                PanicIfWorkerDied();
            }
        }
    }

    /// <summary>
    /// Puts a node in the tree and queues its offer. Off the worker, after
    /// start, returns once the node was offered; from the worker, inside a
    /// drain, or before start, returns at once and the offer follows.
    /// </summary>
    /// <param name="node">A pending node.</param>
    internal static void PublishNode(DeviceNode node)
    {
        using (s_nodesLock.AcquireIrqSafe())
        {
            DeviceNode[] nodes = new DeviceNode[s_nodes.Length + 1];
            Array.Copy(s_nodes, nodes, s_nodes.Length);
            nodes[s_nodes.Length] = node;
            s_nodes = nodes;
        }

        Submit(new EngineJob(EngineJobKind.NodeArrived) { Node = node, Completion = NewCompletion() });
    }

    /// <summary>
    /// Queues a node's teardown. Same completion rule as <see cref="PublishNode"/>.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="hardwarePresent">Whether the hardware is still there to be quiesced.</param>
    /// <exception cref="InvalidOperationException">Called from one of the node's own driver threads, which the teardown would have to join.</exception>
    internal static void RetractNode(DeviceNode node, bool hardwarePresent)
    {
        if (node.Binding is { } binding && binding.IsCurrentThreadOwned)
        {
            throw new InvalidOperationException("A node cannot be retracted from its own driver's thread.");
        }

        Submit(new EngineJob(EngineJobKind.NodeRetracted) { Node = node, HardwarePresent = hardwarePresent, Completion = NewCompletion() });
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
    /// first, inside it), then the node is marked retracted and the bus's
    /// allocation for it released unless resources leaked. Worker or drain only.
    /// </summary>
    internal static void TeardownNode(DeviceNode node, DetachCause cause, bool hardwarePresent)
    {
        if (node.State == NodeState.Retracted)
        {
            return;
        }

        node.Binding?.Teardown(new DetachReason(cause, hardwarePresent));
        node.State = NodeState.Retracted;
        if (node.LeakedResourceCount == 0)
        {
            node.BusResource?.Release();
        }

        DriverLog.Retracted(node);
    }

    private static InterruptEvent? NewCompletion() => IsStarted && HasWorker && !IsOnWorker ? new InterruptEvent() : null;

    private static void Submit(EngineJob job)
    {
        using (s_queueLock.AcquireIrqSafe())
        {
            EnqueueLocked(job);
        }

        s_wake.Signal();

        if (job.Completion is not null)
        {
            while (!job.Completion.Wait(PollMilliseconds))
            {
                PanicIfWorkerDied();
            }

            return;
        }

        if (IsStarted && !HasWorker)
        {
            RunPending();
        }
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
                s_running = false;
                int waiters = s_quiescenceWaiters;
                s_quiescenceWaiters = 0;
                for (int i = 0; i < waiters; i++)
                {
                    s_quiescent.Signal();
                }

                return false;
            }

            s_head = job.Next;
            if (s_head is null)
            {
                s_tail = null;
            }

            job.Next = null;
            job.IsQueued = false;
            s_running = true;
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
                default:
                    RunWorkItem(job.Item!);
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
