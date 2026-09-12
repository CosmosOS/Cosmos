namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Outcome of <see cref="SchedulerInfo.RequestKill"/>.
/// </summary>
public enum ThreadKillResult : byte
{
    /// <summary>
    /// No live thread has the requested ID. Also the answer when no scheduler
    /// is installed and when scheduler support is compiled out, since neither
    /// leaves a registry to search.
    /// </summary>
    NotFound,

    /// <summary>
    /// The thread was removed from its run queue and terminated, on the
    /// managed side too: a <see cref="global::System.Threading.Thread.Join()"/>
    /// on it returns, and it reads as stopped, exactly as after a return
    /// from its entry point.
    /// </summary>
    Killed,

    /// <summary>
    /// The thread was running and has been marked dead. The next reschedule
    /// stops putting it back in a run queue, so it never runs again, but
    /// nothing reaps it: the registry slot, the stack, the allocation
    /// context and the policy's bookkeeping are released only by a thread's
    /// own exit path, which a thread stopped this way never reaches. Prefer
    /// letting a thread return from its entry point.
    /// </summary>
    MarkedForExit,

    /// <summary>The request named an idle thread, which cannot be killed.</summary>
    RefusedIdle,

    /// <summary>
    /// The thread is registered but is neither running nor queued: blocked,
    /// sleeping, created and never started, or already marked for exit by an
    /// earlier request. Blocked and sleeping threads sit outside the run
    /// queues with their share already returned to the policy, so terminating
    /// one from here would return it twice; wake it first.
    /// </summary>
    RefusedBlocked,
}
