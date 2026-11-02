// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
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
/// (the finalizer, the driver kit's, the console sessions', audio) stay on
/// <see cref="Housekeeping"/> whatever the default says, and a kernel that
/// wants its main flow everywhere sets <see cref="DefaultAffinity"/> to
/// <see cref="Online"/> before it creates threads. Until the collector stops
/// every CPU (SMP phase 4), a thread away from CPU 0 must not allocate.
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
    /// collector runs and where the driver stack's interrupt handlers are.
    /// </summary>
    public static CpuSet Housekeeping => new(SchedulerManager.HousekeepingCpus);

    /// <summary>
    /// The CPUs left to a kernel's own work: <see cref="Online"/> without
    /// <see cref="Housekeeping"/>, or <see cref="Online"/> itself on a
    /// machine with one CPU.
    /// </summary>
    public static CpuSet Compute
    {
        get
        {
            CpuSet online = Online;
            CpuSet compute = online.Without(0);
            return compute.IsEmpty ? online : compute;
        }
    }

    /// <summary>
    /// The index of the CPU this code runs on, exact on every read, with or
    /// without a scheduler. A thread not pinned to one CPU may move between
    /// two reads.
    /// </summary>
    public static uint Current => PerCpuBlock.CurrentIndex;

    /// <summary>
    /// The set a thread created from the boot thread (<c>Main</c>) takes:
    /// <see cref="Housekeeping"/> until a kernel sets it. A thread created
    /// from any other thread takes its creator's set. Setting it is
    /// experimental until the collector stops every CPU (SMP phase 4): a
    /// thread away from CPU 0 must not allocate.
    /// </summary>
    /// <exception cref="InvalidOperationException">On set: scheduler support is compiled out, or the scheduler is not up yet.</exception>
    /// <exception cref="ArgumentException">On set: the set is empty, or names no CPU under <see cref="Count"/>.</exception>
    public static CpuSet DefaultAffinity
    {
        get => SchedulerManager.IsReady ? new(SchedulerManager.DefaultAffinity) : Housekeeping;

        [Experimental(Experimentals.AffinitySeamDiagId)]
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
