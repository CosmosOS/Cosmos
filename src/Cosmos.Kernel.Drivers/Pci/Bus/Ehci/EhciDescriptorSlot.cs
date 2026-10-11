// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// One 64-byte slot of a page of an <see cref="EhciDescriptorPool"/>,
/// holding a queue head or a transfer descriptor. The page and the index
/// identify it; its physical address is what a link pointer carries. The
/// descriptor is reached through <c>MemoryMarshal.AsRef</c> over the page's
/// DMA span, never a pointer. Any context, allocation-free.
/// </summary>
/// <param name="Page">The pool page the slot lies in.</param>
/// <param name="Index">The slot's index in the page.</param>
internal readonly record struct EhciDescriptorSlot(DmaBuffer Page, int Index)
{
    /// <summary>Physical address of the slot's first byte, 64-byte aligned.</summary>
    public ulong PhysicalAddress => Page.PhysicalAddress + ((ulong)Index * EhciDescriptorPool.SlotBytes);

    /// <summary>The slot's physical address as a qTD pointer: a 32-bit address with the T bit clear.</summary>
    public uint TransferPointer => (uint)PhysicalAddress;

    /// <summary>The slot's bytes.</summary>
    public Span<byte> Span => Page.Span.Slice(Index * EhciDescriptorPool.SlotBytes, EhciDescriptorPool.SlotBytes);

    /// <summary>The slot as a queue head.</summary>
    public ref EhciQueueHead QueueHead => ref MemoryMarshal.AsRef<EhciQueueHead>(Span);

    /// <summary>The slot as a transfer descriptor.</summary>
    public ref EhciTransferDescriptor TransferDescriptor => ref MemoryMarshal.AsRef<EhciTransferDescriptor>(Span);

    /// <summary>Zeroes the slot. Only for a slot the controller does not hold.</summary>
    public void Clear()
    {
        Span.Clear();
    }
}
