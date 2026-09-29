// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// Where the kit placed the three rings of one split virtqueue in physical
/// memory: built by <see cref="VirtioAccess.TryCreateQueue"/>, consumed by a
/// transport's <see cref="VirtioTransport.ActivateQueue"/>, which programs the
/// addresses (a modern transport) or the page frame number and the alignment
/// (a legacy MMIO transport). Any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct VirtqueueLayout
{
    /// <summary>Records where the rings are.</summary>
    /// <param name="size">The queue size, a power of two.</param>
    /// <param name="descriptorTable">Physical address of the descriptor table.</param>
    /// <param name="availableRing">Physical address of the available ring.</param>
    /// <param name="usedRing">Physical address of the used ring.</param>
    /// <param name="baseAddress">Physical address of the page-aligned block holding the three.</param>
    /// <param name="alignment">The boundary the used ring was placed on.</param>
    public VirtqueueLayout(ushort size, ulong descriptorTable, ulong availableRing, ulong usedRing, ulong baseAddress, uint alignment)
    {
        Size = size;
        DescriptorTable = descriptorTable;
        AvailableRing = availableRing;
        UsedRing = usedRing;
        Base = baseAddress;
        Alignment = alignment;
    }

    /// <summary>The queue size: how many descriptors the table holds and how many entries each ring has.</summary>
    public ushort Size { get; }

    /// <summary>Physical address of the descriptor table, the first thing in the block.</summary>
    public ulong DescriptorTable { get; }

    /// <summary>Physical address of the available ring, right after the descriptor table.</summary>
    public ulong AvailableRing { get; }

    /// <summary>Physical address of the used ring, on the next <see cref="Alignment"/> boundary after the available ring.</summary>
    public ulong UsedRing { get; }

    /// <summary>Physical address of the page-aligned block holding the three rings; the same as <see cref="DescriptorTable"/>.</summary>
    public ulong Base { get; }

    /// <summary>
    /// The boundary the used ring was placed on: the kit's page size. A
    /// legacy MMIO transport writes it to QueueAlign and compares it with its
    /// own guest page size.
    /// </summary>
    public uint Alignment { get; }
}
