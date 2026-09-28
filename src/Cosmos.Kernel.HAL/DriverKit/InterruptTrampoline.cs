// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The kit's dispatcher between an <see cref="InterruptSource"/> and a
/// driver's <see cref="InterruptHandler"/>. The source calls
/// <see cref="Invoke"/> with interrupts masked; the trampoline enters the
/// interrupt-context guard, runs the handler, and turns an exception into a
/// recorded fault instead of a crash: the fault is counted on the node, the
/// source is masked so a handler that throws once cannot throw on every
/// delivery, and a preallocated work item logs it in thread context. Nothing
/// here allocates: the message kept is the exception's own.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class InterruptTrampoline
{
    private readonly InterruptHandler _handler;
    private readonly InterruptContext _context;
    private readonly InterruptHandle _handle;
    private readonly DeviceNode _node;
    private readonly WorkItem _faultLog;
    private volatile bool _disconnected;

    internal InterruptTrampoline(InterruptHandler handler, InterruptContext context, InterruptHandle handle, DeviceNode node, WorkItem faultLog)
    {
        _handler = handler;
        _context = context;
        _handle = handle;
        _node = node;
        _faultLog = faultLog;
    }

    /// <summary>The node whose interrupt this dispatches, for the source's own log lines.</summary>
    public DeviceNode Node => _node;

    /// <summary>
    /// Runs the handler once. Called by the source with interrupts masked;
    /// <paramref name="synthetic"/> says whether the caller is a test raising
    /// the source from thread context rather than a real interrupt, which
    /// decides how the guard reports a violation.
    /// </summary>
    /// <param name="synthetic">True for a synthetic dispatch.</param>
    /// <returns>False when the handler did not run because the handle is disconnected or masked.</returns>
    public bool Invoke(bool synthetic)
    {
        if (_disconnected || _handle.IsMasked)
        {
            return false;
        }

        InterruptContextGuard.Enter(synthetic);
        try
        {
            _handler(_context);
        }
        catch (Exception exception)
        {
            _node.RecordFault(exception.Message);
            _handle.Mask();
            _faultLog.Schedule();
        }
        finally
        {
            InterruptContextGuard.Exit();
        }

        return true;
    }

    /// <summary>Makes every later <see cref="Invoke"/> return without running the handler.</summary>
    internal void MarkDisconnected() => _disconnected = true;
}
