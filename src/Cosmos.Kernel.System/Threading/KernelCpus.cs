// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.System.Threading;

/// <summary>
/// The CPUs the kernel runs on, as sets: how many the scheduler manages, which
/// are online, the housekeeping and compute split, the CPU this code runs on,
/// and the set a new thread takes by default. The reads answer on every
/// kernel; the one write, <see cref="DefaultAffinity"/>'s setter, needs the
/// scheduler (<c>CosmosEnableScheduler</c>).
/// <para>
/// The split is a default, not a mechanism: the kernel's own service threads
/// (the finalizer, the driver kit's, the console sessions', audio, alarms) stay on
/// <see cref="Housekeeping"/> whatever the default says, where the device
/// interrupts and the timers' callbacks are, and the threads a kernel starts
/// from <c>Main</c> go to <see cref="Compute"/>, where the scheduler spreads
/// them. A kernel that wants its main flow everywhere sets
/// <see cref="DefaultAffinity"/> to <see cref="Online"/> before it creates
/// threads. On two CPUs <see cref="Compute"/> is CPU 1 alone, so those threads
/// share one CPU, as they did on one. A thread allocates on whichever CPU it
/// runs: the collector stops every CPU.
/// </para>
/// </summary>
public static class KernelCpus
{
    /// <summary>
    /// The CPUs the scheduler manages, online or not: the per-CPU table's
    /// length. 1 before the scheduler is up, or with it compiled out.
    /// </summary>
    public static int Count => SchedulerManager.IsReady ? (int)SchedulerManager.CpuCount : 1;

    /// <summary>
    /// The CPUs running their scheduler tick: CPU 0 and every application
    /// processor that came up. A snapshot; CPUs only come online, never go.
    /// </summary>
    public static CpuSet Online => SchedulerManager.IsReady ? new(SchedulerManager.OnlineCpus) : Housekeeping;

    /// <summary>
    /// The CPUs the kernel's own service threads run on: CPU 0, where the
    /// driver stack's interrupt handlers are.
    /// </summary>
    public static CpuSet Housekeeping => new(SchedulerManager.HousekeepingCpus);

    /// <summary>
    /// The CPUs left to a kernel's own work: <see cref="Online"/> without
    /// <see cref="Housekeeping"/>, or <see cref="Online"/> itself on a
    /// machine with one CPU.
    /// </summary>
    public static CpuSet Compute => SchedulerManager.IsReady ? new(SchedulerManager.ComputeCpus) : Housekeeping;

    /// <summary>
    /// The index of the CPU this code runs on, exact on every read, with or
    /// without a scheduler. A thread not pinned to one CPU may move between
    /// two reads.
    /// </summary>
    public static uint Current => PerCpuBlock.CurrentIndex;

    /// <summary>
    /// The set a thread created from the boot thread (<c>Main</c>) takes:
    /// <see cref="Compute"/> until a kernel sets it. A thread created from
    /// any other thread takes its creator's set. It is also what
    /// <c>Environment.ProcessorCount</c> counts, and CoreLib keeps the count
    /// it reads the first time something asks: set the default first thing,
    /// or the thread pool keeps the size it took for the set it found.
    /// </summary>
    /// <exception cref="InvalidOperationException">On set: scheduler support is compiled out, or the scheduler is not up yet.</exception>
    /// <exception cref="ArgumentException">On set: the set is empty, or names no CPU under <see cref="Count"/>.</exception>
    public static CpuSet DefaultAffinity
    {
        get => SchedulerManager.IsReady ? new(SchedulerManager.DefaultAffinity) : Housekeeping;
        set
        {
            ThrowIfSchedulerUnavailable();
            ThrowIfNamesNoCpu(value, nameof(value));
            SchedulerManager.TrySetDefaultAffinity(value.ToSeam());
        }
    }

    /// <summary>
    /// Refuses a set no thread could take: empty, or naming no CPU the
    /// scheduler manages (a set past <see cref="Count"/> alone).
    /// </summary>
    internal static void ThrowIfNamesNoCpu(CpuSet set, string paramName)
    {
        int count = Count;
        for (uint cpu = 0; cpu < count; cpu++)
        {
            if (set.Contains(cpu))
            {
                return;
            }
        }

        throw new ArgumentException(set.IsEmpty
            ? "A CPU set must name at least one CPU."
            : "The CPU set names no CPU this kernel manages (KernelCpus.Count).", paramName);
    }

    /// <summary>The compiled-out and not-yet-up refusals, shared with <see cref="ThreadAffinity"/>.</summary>
    internal static void ThrowIfSchedulerUnavailable()
    {
        if (!KernelFeatures.Scheduler)
        {
            throw new InvalidOperationException("Scheduler support is disabled. Set CosmosEnableScheduler=true in your csproj to enable it.");
        }

        if (!SchedulerManager.IsReady)
        {
            throw new InvalidOperationException("The scheduler is not running yet: a CPU set is changed from Main or later.");
        }
    }
}
