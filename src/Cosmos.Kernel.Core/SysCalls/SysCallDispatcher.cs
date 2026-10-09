// This code is licensed under MIT license (see LICENSE for details)

using Cosmos.Kernel.Core.Bridge.Export;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.SysCalls;

/// <summary>
/// Architecture-neutral syscall registration and routing. Owns the dense
/// handler table indexed by <see cref="SysCallNumber"/>. Mirrors the layout
/// of <see cref="CPU.InterruptManager"/>: a single managed entry
/// (<see cref="SysCallNative"/>) forwards the trap stub's
/// <see cref="SysCallContext"/> here, and the per-syscall handlers are
/// plugged in by Cosmos.Kernel.System (file/console/process drivers, ...).
/// </summary>
internal static class SysCallDispatcher
{
    /// <summary>
    /// Syscall handler signature. Receives the captured register frame by
    /// ref so arch-specific handlers can read user pointer arguments off it
    /// without copying. Returning <see cref="SysCallResult.Failure"/> with
    /// <see cref="SysCallError.Enosys"/> is the convention for "not implemented".
    /// </summary>
    /// <param name="context">Register frame captured by the trap stub.</param>
    public delegate SysCallResult SysCallHandler(ref SysCallContext context);

    private static SysCallHandler?[]? s_handlers;

    /// <summary>Guards RMW on <see cref="s_handlers"/> so concurrent
    /// Register/Unregister calls (e.g. subsystem init vs. driver unload)
    /// can't tear a slot write.</summary>
    private static Scheduler.SpinLock s_lock;

    /// <summary>
    /// Whether the syscall dispatch surface is compiled in. Gated by the
    /// <c>CosmosEnableSysCalls</c> MSBuild property via
    /// <see cref="CosmosFeatures.SysCallsEnabled"/>; when false, ILC trims
    /// the entire subsystem away.
    /// </summary>
    public static bool IsEnabled => CosmosFeatures.SysCallsEnabled;

    /// <summary>
    /// Allocate the handler table. Idempotent: re-init clears all
    /// registrations. Called once by kernel startup after the scheduler is
    /// up, before the trap stub is wired into the IDT/exception vector.
    /// Throws when the syscall feature is disabled — the dispatch table
    /// must not be reachable in that build.
    /// </summary>
    public static void Initialize()
    {
        if (!CosmosFeatures.SysCallsEnabled)
        {
            Panic.Halt("SysCallDispatcher.Initialize called while SysCalls disabled");
        }

        s_handlers = new SysCallHandler?[(int)SysCallNumber.Munmap + 1];
    }

    /// <summary>
    /// Register a handler for <paramref name="number"/>. Replaces any
    /// handler already registered for that slot. Must be called after
    /// <see cref="Initialize"/>; a missing table is a kernel bug and halts.
    /// </summary>
    public static void Register(uint number, SysCallHandler handler)
    {
        if (s_handlers == null)
        {
            Panic.Halt("SysCallDispatcher.Register called before Initialize");
        }

        s_lock.Acquire();
        try
        {
            s_handlers[(uint)number] = handler;
        }
        finally
        {
            s_lock.Release();
        }
    }

    /// <summary>
    /// Remove the handler registered for <paramref name="number"/>. After
    /// this returns, calls to that number return <see cref="SysCallError.Enosys"/>.
    /// Safe to call on a never-registered number.
    /// </summary>
    public static void Unregister(uint number)
    {
        if (s_handlers == null)
        {
            return;
        }

        s_lock.Acquire();
        try
        {
            s_handlers[(uint)number] = null;
        }
        finally
        {
            s_lock.Release();
        }
    }

    /// <summary>
    /// Route a syscall context to the registered handler. Out-of-range
    /// numbers and unregistered slots return <see cref="SysCallError.Enosys"/>
    /// rather than panicking — userspace probing an unimplemented call is a
    /// normal condition, not a kernel fault. A null handler table means
    /// startup raced the trap; treat as fatal.
    /// </summary>
    public static SysCallResult Dispatch(ref SysCallContext context)
    {
        SysCallHandler?[]? handlers = s_handlers;
        if (handlers == null)
        {
            Panic.Halt("SysCallDispatcher.Dispatch called before Initialize");
        }

        uint number = (uint)context.Number;
        if (number >= (uint)handlers.Length)
        {
            return SysCallResult.Failure(SysCallError.Enosys);
        }

        // Snapshot the slot under the lock so a concurrent Unregister can't
        // null it between the null check and the invoke. The delegate itself
        // is immutable; invoking it off-lock is safe.
        SysCallHandler? handler;
        s_lock.Acquire();
        try
        {
            handler = handlers[number];
        }
        finally
        {
            s_lock.Release();
        }

        if (handler == null)
        {
            return SysCallResult.Failure(SysCallError.Enosys);
        }

        return handler(ref context);
    }
}
