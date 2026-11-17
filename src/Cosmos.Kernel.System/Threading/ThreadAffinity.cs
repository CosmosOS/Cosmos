// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.Scheduler;
using SysThread = System.Threading.Thread;

namespace Cosmos.Kernel.System.Threading;

/// <summary>
/// Which CPUs a thread may run on, and which one the scheduler prefers for
/// it: a hard set the scheduler never leaves (a thread running on a CPU its
/// set excludes is moved at that CPU's next interrupt exit) and an ideal CPU
/// inside it. A thread inherits its creator's set, or
/// <see cref="KernelCpus.DefaultAffinity"/> when created from the boot
/// thread. Experimental until the collector stops every CPU (SMP phase 4): a
/// thread away from CPU 0 must not allocate, so what moves one is marked.
/// <para>
/// A <see cref="global::System.Threading.Thread"/> resolves to the scheduler's thread by its
/// handle: the calling thread always resolves, the boot thread included;
/// another thread resolves while the scheduler runs it, from its start to
/// its exit. The members that take a thread answer false for one that does
/// not resolve, and the writes answer false for a CPU's idle thread, which
/// never moves.
/// </para>
/// </summary>
[Experimental(Experimentals.AffinitySeamDiagId)]
public static class ThreadAffinity
{
    /// <summary>
    /// The CPUs the calling thread may run on. Setting a set that excludes
    /// the CPU the caller runs on returns once the caller runs on one it
    /// names; the boot thread, which runs <c>Main</c> on CPU 0's idle thread,
    /// cannot move.
    /// </summary>
    /// <exception cref="InvalidOperationException">On set: the scheduler is compiled out or not up, the caller is the boot thread or another idle thread, or interrupts are masked, which the move waits on.</exception>
    /// <exception cref="ArgumentException">On set: the set is empty, or names no CPU under <see cref="KernelCpus.Count"/>.</exception>
    public static CpuSet Current
    {
        get => SchedulerManager.CurrentThread is { } thread ? new(thread.Affinity) : KernelCpus.Housekeeping;
        set
        {
            KernelCpus.ThrowIfSchedulerUnavailable();
            KernelCpus.ThrowIfNamesNoCpu(value, nameof(value));

            SchedulerThread? thread = SchedulerManager.CurrentThread;
            if (thread is null || thread.IsIdle)
            {
                throw new InvalidOperationException("The boot thread runs Main on CPU 0's idle thread and cannot move; start a thread and set its CPU set.");
            }

            ThrowIfInterruptsMasked();
            SchedulerManager.SetAffinity(thread, value.ToSeam());
        }
    }

    /// <summary>Reads the CPUs <paramref name="thread"/> may run on.</summary>
    /// <param name="thread">The thread.</param>
    /// <param name="affinity">Its set, or <see cref="CpuSet.Empty"/> when it does not resolve.</param>
    /// <returns>False for a thread the scheduler is not running: not started, exited, or another CPU's idle thread.</returns>
    public static bool TryGet(SysThread thread, out CpuSet affinity)
    {
        ArgumentNullException.ThrowIfNull(thread);

        SchedulerThread? resolved = Resolve(thread);
        if (resolved is null)
        {
            affinity = CpuSet.Empty;
            return false;
        }

        affinity = new CpuSet(resolved.Affinity);
        return true;
    }

    /// <summary>
    /// Sets the CPUs <paramref name="thread"/> may run on. A thread running on
    /// a CPU the set excludes moves at that CPU's next interrupt exit, a
    /// parked or queued one at once; the calling thread returns once it runs
    /// on a CPU the set names.
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="affinity">The CPUs it may run on.</param>
    /// <returns>False for a thread the scheduler is not running, and for an idle thread.</returns>
    /// <exception cref="InvalidOperationException">The scheduler is compiled out or not up, or the thread is the caller and interrupts are masked.</exception>
    /// <exception cref="ArgumentException">The set is empty, or names no CPU under <see cref="KernelCpus.Count"/>.</exception>
    public static bool Set(SysThread thread, CpuSet affinity)
    {
        ArgumentNullException.ThrowIfNull(thread);
        KernelCpus.ThrowIfSchedulerUnavailable();
        KernelCpus.ThrowIfNamesNoCpu(affinity, nameof(affinity));

        SchedulerThread? resolved = Resolve(thread);
        if (resolved is null || resolved.IsIdle)
        {
            return false;
        }

        if (ReferenceEquals(resolved, SchedulerManager.CurrentThread))
        {
            ThrowIfInterruptsMasked();
        }

        SchedulerManager.SetAffinity(resolved, affinity.ToSeam());
        return true;
    }

    /// <summary>Reads the CPU the scheduler prefers for <paramref name="thread"/>, one of its set.</summary>
    /// <param name="thread">The thread.</param>
    /// <param name="cpu">The CPU, or 0 when the thread does not resolve.</param>
    /// <returns>False for a thread the scheduler is not running.</returns>
    public static bool TryGetIdealCpu(SysThread thread, out uint cpu)
    {
        ArgumentNullException.ThrowIfNull(thread);

        SchedulerThread? resolved = Resolve(thread);
        if (resolved is null)
        {
            cpu = 0;
            return false;
        }

        cpu = resolved.IdealCpu;
        return true;
    }

    /// <summary>
    /// Sets the CPU the scheduler prefers for <paramref name="thread"/>. A
    /// preference, not a move: the thread stays where it is.
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="cpu">The CPU, one of the thread's set.</param>
    /// <returns>False for a thread the scheduler is not running, for an idle thread, and when its set changed under the call to one excluding <paramref name="cpu"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="KernelCpus.Count"/>.</exception>
    /// <exception cref="ArgumentException">The thread's set excludes <paramref name="cpu"/>.</exception>
    public static bool SetIdealCpu(SysThread thread, uint cpu)
    {
        ArgumentNullException.ThrowIfNull(thread);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cpu, (uint)KernelCpus.Count);
        KernelCpus.ThrowIfSchedulerUnavailable();

        SchedulerThread? resolved = Resolve(thread);
        if (resolved is null || resolved.IsIdle)
        {
            return false;
        }

        if (!resolved.Affinity.Contains(cpu))
        {
            throw new ArgumentException("The thread's CPU set excludes that CPU; widen the set first.", nameof(cpu));
        }

        return SchedulerManager.SetIdealCpu(resolved, cpu);
    }

    /// <summary>
    /// Restricts <paramref name="thread"/> to one CPU: <see cref="Set"/>
    /// with <see cref="CpuSet.Single"/>, the ideal CPU following.
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <param name="cpu">The CPU.</param>
    /// <returns>False for a thread the scheduler is not running, and for an idle thread.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="cpu"/> is at or past <see cref="KernelCpus.Count"/>.</exception>
    public static bool Pin(SysThread thread, uint cpu)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cpu, (uint)KernelCpus.Count);
        return Set(thread, CpuSet.Single(cpu));
    }

    /// <summary>
    /// The scheduler's thread behind a managed one: the current CPU's current
    /// thread for the caller (which is how the boot thread, reached by no
    /// handle, resolves), a registry walk for any other.
    /// </summary>
    private static SchedulerThread? Resolve(SysThread thread)
    {
        if (!SchedulerManager.IsReady)
        {
            return null;
        }

        return ReferenceEquals(thread, SysThread.CurrentThread)
            ? SchedulerManager.CurrentThread
            : SchedulerManager.FindByManagedThread(thread);
    }

    /// <summary>
    /// A move of the calling thread waits for the reschedule interrupt on its
    /// own CPU, which cannot land while interrupts are masked.
    /// </summary>
    private static void ThrowIfInterruptsMasked()
    {
        if (!InternalCpu.InterruptsEnabled)
        {
            throw new InvalidOperationException("The CPU set of the calling thread cannot change while interrupts are masked.");
        }
    }
}
