// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Virtio;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The handler of one message entry of a virtio PCI function: preallocated
/// by <see cref="VirtioPciTransportDriver"/>, one per entry, so the handler
/// itself allocates nothing and only hands the entry to the access's
/// dispatch.
/// </summary>
internal sealed class VirtioPciEntryHandler
{
    private readonly VirtioAccess _access;
    private readonly int _entry;

    /// <summary>The handler of <paramref name="entry"/> on <paramref name="access"/>. Thread context, from the probe.</summary>
    /// <param name="access">The access whose dispatch the entry reaches.</param>
    /// <param name="entry">The entry, from 0.</param>
    internal VirtioPciEntryHandler(VirtioAccess access, int entry)
    {
        _access = access;
        _entry = entry;
    }

    /// <summary>The entry this handler serves. Any context.</summary>
    internal int Entry => _entry;

    /// <summary>The access the entry is dispatched on. Any context.</summary>
    internal VirtioAccess Access => _access;

    /// <summary>Dispatches the entry: the access raises the sources assigned to it. Interrupt context; allocation-free.</summary>
    /// <param name="context">What a handler may do; unused, the leaf's handler runs nested with its own context.</param>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "The method has the InterruptHandler shape; the context is the leaf's to use.")]
    internal void OnInterrupt(InterruptContext context) => _access.Dispatch(_entry);
}
