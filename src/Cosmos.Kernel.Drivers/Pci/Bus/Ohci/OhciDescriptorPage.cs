// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// One page of the descriptor pool: 256 slots of 16 bytes below 4 GiB, and
/// the owner record and generation tag of each slot, kept beside the page
/// so the drain can map a done queue address back to the pipe or device
/// whose TD it is. The arrays are written under the controller's lock and
/// read by the handler without it. Allocated once on the host's binding,
/// never freed early.
/// </summary>
internal sealed class OhciDescriptorPage
{
    /// <summary>Wraps a pool page the host allocated. Thread context.</summary>
    /// <param name="buffer">One page of DMA memory below 4 GiB, page aligned.</param>
    public OhciDescriptorPage(DmaBuffer buffer)
    {
        Buffer = buffer;
        PhysicalAddress = buffer.PhysicalAddress;
        Owners = new object?[OhciProtocol.DescriptorsPerPage];
        Tags = new int[OhciProtocol.DescriptorsPerPage];
    }

    /// <summary>The page.</summary>
    public DmaBuffer Buffer { get; }

    /// <summary>Physical address of slot 0.</summary>
    public ulong PhysicalAddress { get; }

    /// <summary>The device or pipe each slot's TD belongs to; null for an ED, a SETUP slot or a free slot.</summary>
    public object?[] Owners { get; }

    /// <summary>The generation of each slot, bumped every time it is queued as a TD; a done entry whose owner stored another generation is stale.</summary>
    public int[] Tags { get; }

    /// <summary>The slot at <paramref name="index"/> as an ED. Any context, allocation-free.</summary>
    public ref OhciEndpointDescriptor Endpoint(int index)
    {
        return ref MemoryMarshal.Cast<byte, OhciEndpointDescriptor>(Buffer.Span)[index];
    }

    /// <summary>The slot at <paramref name="index"/> as a TD. Any context, allocation-free.</summary>
    public ref OhciTransferDescriptor Transfer(int index)
    {
        return ref MemoryMarshal.Cast<byte, OhciTransferDescriptor>(Buffer.Span)[index];
    }

    /// <summary>The 16 bytes of the slot at <paramref name="index"/>. Any context, allocation-free.</summary>
    public Span<byte> Bytes(int index)
    {
        return Buffer.Span.Slice(index * OhciProtocol.DescriptorBytes, OhciProtocol.DescriptorBytes);
    }
}
