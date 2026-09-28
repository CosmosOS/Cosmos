// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Synthetic;

/// <summary>
/// An interrupt a test raises. <see cref="Raise"/> runs the connected
/// handler with interrupts masked, in a synthetic dispatch: the same guard
/// and fault handling as a real interrupt, with a violation reported as an
/// exception the test can assert rather than a panic. Connection and mask
/// state live under one IRQ-safe lock, so a raise never sees a half-built
/// connection and never runs a handler that was disconnected.
/// </summary>
internal sealed class SyntheticInterruptSource : InterruptSource
{
    private SchedSpinLock _lock;
    private InterruptTrampoline? _trampoline;
    private bool _masked;

    internal SyntheticInterruptSource(int index)
    {
        Index = index;
    }

    /// <summary>The source's index on its node.</summary>
    public int Index { get; }

    /// <summary>True while a handler is connected.</summary>
    public bool IsConnected => _trampoline is not null;

    /// <summary>True while deliveries are stopped.</summary>
    public bool IsMasked => _masked;

    /// <inheritdoc/>
    public override string Describe() => $"synthetic {Index}";

    /// <summary>
    /// Delivers one interrupt. Thread context.
    /// </summary>
    /// <returns>False when nothing is connected or the source is masked.</returns>
    internal bool Raise()
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            InterruptTrampoline? trampoline;
            using (_lock.AcquireIrqSafe())
            {
                if (_masked || _trampoline is null)
                {
                    return false;
                }

                trampoline = _trampoline;
            }

            return trampoline.Invoke(synthetic: true);
        }
    }

    /// <inheritdoc/>
    protected override bool TryConnectCore(InterruptTrampoline trampoline)
    {
        using (_lock.AcquireIrqSafe())
        {
            if (_trampoline is not null)
            {
                return false;
            }

            _trampoline = trampoline;
            _masked = false;
            return true;
        }
    }

    /// <inheritdoc/>
    protected override void MaskCore()
    {
        using (_lock.AcquireIrqSafe())
        {
            _masked = true;
        }
    }

    /// <inheritdoc/>
    protected override void UnmaskCore()
    {
        using (_lock.AcquireIrqSafe())
        {
            _masked = false;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectCore()
    {
        using (_lock.AcquireIrqSafe())
        {
            _masked = true;
            _trampoline = null;
        }
    }
}
