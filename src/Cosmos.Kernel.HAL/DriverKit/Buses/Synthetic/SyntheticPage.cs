// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Buses.Synthetic;

/// <summary>
/// One zeroed RAM page behind a synthetic device's register window: the
/// bus's own allocation for the node, released by the kit after the node's
/// teardown unless a driver thread may still touch it.
/// </summary>
internal sealed class SyntheticPage : IKitResource
{
    private volatile bool _released;

    /// <summary>The page.</summary>
    public MemoryBlock Block { get; }

    /// <summary>The kernel's virtual address of the page.</summary>
    public ulong Address => Block.Base;

    /// <summary>The page's physical address.</summary>
    public ulong PhysicalAddress { get; }

    /// <summary>True once the page went back to the allocator.</summary>
    public bool IsReleased => _released;

    private SyntheticPage(MemoryBlock block, ulong physicalAddress)
    {
        Block = block;
        PhysicalAddress = physicalAddress;
    }

    /// <summary>Allocates a zeroed page.</summary>
    /// <exception cref="InvalidOperationException">No page left.</exception>
    internal static SyntheticPage Allocate()
    {
        MemoryBlock page = PageAllocator.AllocBlock(PageType.Unmanaged, 1, zero: true)
            ?? throw new InvalidOperationException("No page left for a synthetic device window.");
        return new SyntheticPage(page, PageAllocator.VirtualToPhysical(page.Base));
    }

    /// <inheritdoc/>
    public void Release()
    {
        if (_released)
        {
            return;
        }

        _released = true;
        PageAllocator.Free(Block);
    }
}
