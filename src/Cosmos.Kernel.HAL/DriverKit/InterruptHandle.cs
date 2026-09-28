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
    private readonly InterruptSource _source;
    private volatile bool _masked;

    internal InterruptHandle(InterruptSource source)
    {
        _source = source;
    }

    /// <summary>The source this handle is connected to.</summary>
    public InterruptSource Source => _source;

    /// <summary>True while deliveries are stopped at the controller.</summary>
    public bool IsMasked => _masked;

    /// <summary>Stops deliveries at the controller. Allocation-free; any context.</summary>
    public void Mask()
    {
        _masked = true;
        _source.Mask();
    }

    /// <summary>Lets deliveries through again. Allocation-free; any context.</summary>
    public void Unmask()
    {
        _masked = false;
        _source.Unmask();
    }

    /// <summary>The dispatcher connected for this handle.</summary>
    internal InterruptTrampoline? Trampoline { get; set; }

    /// <summary>Masks the source and disconnects the handler for good. Teardown only.</summary>
    internal void Disconnect()
    {
        _masked = true;
        Trampoline?.MarkDisconnected();
        _source.Disconnect();
    }
}
