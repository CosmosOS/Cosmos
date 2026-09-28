// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// Knows whether the current code runs inside a driver's interrupt handler.
/// The trampoline enters it around every handler, and the binding's
/// thread-context members refuse to run while it is entered: a handler that
/// tries to sleep, wait, allocate a resource or publish a device is stopped
/// with the member's name instead of a deadlock. In a synthetic dispatch
/// (a test raising a source from thread context) that is an exception the
/// test can assert; in a real interrupt it is a panic, because building an
/// exception there would allocate in the very place the guard forbids it.
/// Single CPU today: the depth is one word, to become per-CPU with the
/// scheduler.
/// </summary>
internal static class InterruptContextGuard
{
    private static int s_depth;
    private static bool s_synthetic;

    /// <summary>True while a driver interrupt handler runs on this CPU.</summary>
    public static bool IsInHandler => Volatile.Read(ref s_depth) > 0;

    /// <summary>Marks the start of a handler dispatch. Interrupts are masked by the caller.</summary>
    /// <param name="synthetic">True when a test raises the source from thread context.</param>
    internal static void Enter(bool synthetic)
    {
        s_synthetic = synthetic;
        s_depth++;
    }

    /// <summary>Marks the end of a handler dispatch.</summary>
    internal static void Exit() => s_depth--;

    /// <summary>Stops a thread-context member called from a driver interrupt handler.</summary>
    /// <param name="member">The member the caller is in, for the message.</param>
    /// <exception cref="InvalidOperationException">A driver interrupt handler is running, in a synthetic dispatch.</exception>
    public static void ThrowIfInHandler(string member)
    {
        if (!IsInHandler)
        {
            return;
        }

        if (s_synthetic)
        {
            throw new InvalidOperationException($"{member} cannot be called from an interrupt handler; hand the work to a work item.");
        }

        Panic.Halt(member);
    }
}
