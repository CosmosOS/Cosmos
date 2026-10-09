using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Base class for objects that can hold scheduler-specific extension data.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public abstract class SchedulerExtensible
{
    /// <summary>
    /// Scheduler-specific data. Each scheduler defines its own class and
    /// stores an instance here from its <see cref="IScheduler"/> lifecycle
    /// hooks (<see cref="IScheduler.InitializeCpu"/>,
    /// <see cref="IScheduler.OnThreadCreate"/>), and clears it again in
    /// <see cref="IScheduler.ShutdownCpu"/> and
    /// <see cref="IScheduler.OnThreadExit"/>.
    /// <para>
    /// One slot is the whole budget: a policy needing several values defines
    /// one class holding them. A policy swap moves every live thread with it:
    /// <see cref="SchedulerManager.SetScheduler"/> hands each one to the
    /// outgoing policy's <see cref="IScheduler.OnThreadExit"/> and then to
    /// the incoming policy's <see cref="IScheduler.OnThreadCreate"/>, so a
    /// hook is never handed a record another policy wrote.
    /// </para>
    /// <para>
    /// Read it with <c>as</c>, never a cast, all the same: a hook can still
    /// meet an empty slot, because <see cref="IScheduler.OnThreadExit"/>
    /// clears it and a thread can lose its record between a tick and the
    /// hook that observes it. A cast that fails does so inside the timer
    /// interrupt, and <c>as</c> costs nothing on a hook that handles null
    /// anyway.
    /// </para>
    /// </summary>
    public object? SchedulerData { get; set; }
}
