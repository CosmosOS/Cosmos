// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// A connected interrupt, returned by <see cref="DeviceBinding.TryRequestInterrupt"/>.
/// The driver masks and unmasks through it from any context; the kit
/// disconnects it at teardown. A handler that throws leaves its source
/// masked, with the fault recorded on the node; the driver may unmask again.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class InterruptHandle
{
    private volatile bool _masked;

    /// <summary>The source this handle is connected to.</summary>
    public InterruptSource Source { get; }

    /// <summary>True while deliveries are stopped at the controller.</summary>
    public bool IsMasked => _masked;

    /// <summary>The dispatcher connected for this handle.</summary>
    internal InterruptTrampoline? Trampoline { get; set; }

    internal InterruptHandle(InterruptSource source)
    {
        Source = source;
    }

    /// <summary>Stops deliveries at the controller. Allocation-free; any context.</summary>
    public void Mask()
    {
        _masked = true;
        Source.Mask();
    }

    /// <summary>Lets deliveries through again. Allocation-free; any context.</summary>
    public void Unmask()
    {
        _masked = false;
        Source.Unmask();
    }

    /// <summary>Masks the source and disconnects the handler for good. Teardown only.</summary>
    internal void Disconnect()
    {
        _masked = true;
        Trampoline?.MarkDisconnected();
        Source.Disconnect();
    }
}
