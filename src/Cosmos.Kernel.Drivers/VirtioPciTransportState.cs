// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Everything <see cref="VirtioPciTransportDriver"/> holds for one bound
/// function, hung off <see cref="DeviceBinding.DriverState"/>: the access it
/// built, the transport under it, the entry handlers and the child node it
/// published. Written by the probe in thread context, read by the driver's
/// detach hook and by the Virtio suite.
/// </summary>
internal sealed class VirtioPciTransportState
{
    private readonly VirtioAccess _access;
    private readonly VirtioPciTransport _transport;
    private readonly VirtioPciEntryHandler[] _entryHandlers;
    private DeviceNode? _child;

    /// <summary>Records the objects the probe built. Thread context, from the probe.</summary>
    /// <param name="access">The access the child node carries.</param>
    /// <param name="transport">The transport under the access.</param>
    /// <param name="entryHandlers">One handler per message entry the driver may connect.</param>
    internal VirtioPciTransportState(VirtioAccess access, VirtioPciTransport transport, VirtioPciEntryHandler[] entryHandlers)
    {
        _access = access;
        _transport = transport;
        _entryHandlers = entryHandlers;
    }

    /// <summary>The access the child node carries. Any context.</summary>
    internal VirtioAccess Access => _access;

    /// <summary>The transport under the access. Any context.</summary>
    internal VirtioPciTransport Transport => _transport;

    /// <summary>The preallocated handlers, index n for entry n. Any context.</summary>
    internal ReadOnlySpan<VirtioPciEntryHandler> EntryHandlers => _entryHandlers;

    /// <summary>The virtio node the probe published; null until then. Thread context.</summary>
    internal DeviceNode? Child
    {
        get => _child;
        set => _child = value;
    }
}
