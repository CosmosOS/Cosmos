// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="VirtioMmioTransportDriver"/> holds for one bound
/// slot, hung off <see cref="DeviceBinding.DriverState"/>: the access it
/// built, the transport under it and the child node it published, plus the
/// line handler. Written by the probe in thread context; the handler runs
/// in interrupt context.
/// </summary>
internal sealed class VirtioMmioTransportState
{
    /// <summary>The one entry an MMIO slot delivers: its line.</summary>
    private const int LineEntry = 0;

    private readonly VirtioAccess _access;
    private readonly VirtioMmioTransport _transport;
    private DeviceNode? _child;

    /// <summary>Records the objects the probe built. Thread context, from the probe.</summary>
    /// <param name="access">The access the child node carries.</param>
    /// <param name="transport">The transport under the access.</param>
    internal VirtioMmioTransportState(VirtioAccess access, VirtioMmioTransport transport)
    {
        _access = access;
        _transport = transport;
    }

    /// <summary>The access the child node carries. Any context.</summary>
    internal VirtioAccess Access => _access;

    /// <summary>The transport under the access. Any context.</summary>
    internal VirtioMmioTransport Transport => _transport;

    /// <summary>The virtio node the probe published; null until then. Thread context.</summary>
    internal DeviceNode? Child
    {
        get => _child;
        set => _child = value;
    }

    /// <summary>The line handler: dispatches entry 0, which reads and acknowledges the slot's status. Interrupt context; allocation-free.</summary>
    /// <param name="context">What a handler may do; unused, the leaf's handler runs nested with its own context.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The method has the InterruptHandler shape; the context is the leaf's to use.")]
    internal void OnInterrupt(InterruptContext context) => _access.Dispatch(LineEntry);
}
