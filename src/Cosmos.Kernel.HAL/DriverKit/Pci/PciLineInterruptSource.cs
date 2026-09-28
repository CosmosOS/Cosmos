// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Build.API.Enum;
using Cosmos.Kernel.Core.CPU;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.DriverKit.Pci;

/// <summary>
/// A PCI function's legacy interrupt line, routed the way the x64 platform
/// routes an ISA IRQ: the line register firmware wrote is the I/O APIC
/// input, programmed edge-triggered and active-high as it is today, which
/// QEMU delivers and a real chipset may not (there the register is not the
/// GSI and INTx is level-triggered). Only lines 3 to 15 are routable: the
/// platform's ISA routing is defined for lines 0 to 15, the lowest three
/// are its own (timer, keyboard, cascade), and a register above 15 would
/// land on a vector with another owner (the dynamic message vectors, the
/// CPU exceptions) while the I/O APIC silently ignores it. One function
/// per line: a line another handler holds is refused rather than shared.
/// Never routable on ARM64, where the register names nothing. Connection
/// state lives under one IRQ-safe lock; the adapter from the platform's
/// handler shape to the kit's trampoline is allocated once, at the first
/// connect, in thread context.
/// </summary>
internal sealed class PciLineInterruptSource : InterruptSource
{
    /// <summary>A line register of zero: the function has no line.</summary>
    private const byte NoLine = 0;
    /// <summary>A line register of 0xFF: firmware routed nothing.</summary>
    private const byte UnroutedLine = 0xFF;
    /// <summary>Lines below this are ISA lines the platform owns (timer, keyboard, cascade).</summary>
    private const byte FirstPciLine = 3;
    /// <summary>The last ISA line: the platform's line primitives are defined for 0 to 15, and a higher register names no routable input.</summary>
    private const byte LastIsaLine = 15;

    private readonly PciAccess _access;
    private readonly byte _line;
    private SchedSpinLock _lock;
    private InterruptTrampoline? _trampoline;
    private InterruptManager.IrqDelegate? _adapter;
    private volatile bool _connected;

    internal PciLineInterruptSource(PciAccess access)
    {
        _access = access;
        _line = access.InterruptLine;
    }

    /// <summary>True when the register names a line at all.</summary>
    private bool HasLine => _line != NoLine && _line != UnroutedLine;

    /// <inheritdoc/>
    public override string Describe() => HasLine ? $"line {_line}" : "line (none)";

    /// <inheritdoc/>
    protected override bool TryConnectCore(InterruptTrampoline trampoline)
    {
        if (PlatformHAL.Architecture != PlatformArchitecture.X64
            || !HasLine
            || _line < FirstPciLine
            || _line > LastIsaLine
            || !InterruptManager.IsControllerInitialized
            || InterruptManager.HasIrqHandler(_line))
        {
            return false;
        }

        _adapter ??= HandleIrq;
        using (_lock.AcquireIrqSafe())
        {
            if (_connected)
            {
                return false;
            }

            _trampoline = trampoline;
            _connected = true;
        }

        // The redirection entry is opened first: it is edge-triggered, so a
        // function whose interrupt is already pending has to assert the
        // line after the entry is unmasked, and clearing INTx disable is
        // what makes that edge. The other way round the first delivery
        // would be lost until the function deasserted and asserted again.
        InterruptManager.SetIrqHandler(_line, _adapter);
        _access.SetInterruptDisable(false);
        return true;
    }

    /// <inheritdoc/>
    protected override void MaskCore()
    {
        if (_connected)
        {
            InterruptManager.MaskIrq(_line);
        }
    }

    /// <inheritdoc/>
    protected override void UnmaskCore()
    {
        if (_connected)
        {
            InterruptManager.UnmaskIrq(_line);
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectCore()
    {
        if (!_connected)
        {
            return;
        }

        InterruptManager.ClearIrqHandler(_line);
        _access.SetInterruptDisable(true);
        using (_lock.AcquireIrqSafe())
        {
            _trampoline = null;
            _connected = false;
        }
    }

    /// <summary>The platform's handler: runs the kit's trampoline. Interrupt context.</summary>
    private void HandleIrq(ref IRQContext context)
    {
        InterruptTrampoline? trampoline;
        using (_lock.AcquireIrqSafe())
        {
            trampoline = _trampoline;
        }

        trampoline?.Invoke(synthetic: false);
    }
}
