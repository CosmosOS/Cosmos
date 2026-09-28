// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Synthetic;

/// <summary>
/// One zeroed RAM page behind a synthetic device's register window: the
/// bus's own allocation for the node, released by the kit after the node's
/// teardown unless a driver thread may still touch it.
/// </summary>
internal sealed unsafe class SyntheticPage : IKitResource
{
    private volatile bool _released;

    private SyntheticPage(ulong address, ulong physicalAddress)
    {
        Address = address;
        PhysicalAddress = physicalAddress;
    }

    /// <summary>The kernel's virtual address of the page.</summary>
    public ulong Address { get; }

    /// <summary>The page's physical address.</summary>
    public ulong PhysicalAddress { get; }

    /// <summary>True once the page went back to the allocator.</summary>
    public bool IsReleased => _released;

    /// <summary>Allocates a zeroed page.</summary>
    /// <exception cref="InvalidOperationException">No page left.</exception>
    internal static SyntheticPage Allocate()
    {
        void* page = PageAllocator.AllocPages(PageType.Unmanaged, 1, zero: true);
        if (page == null)
        {
            throw new InvalidOperationException("No page left for a synthetic device window.");
        }

        return new SyntheticPage((ulong)page, PageAllocator.VirtualToPhysical((ulong)page));
    }

    /// <inheritdoc/>
    public void Release()
    {
        if (_released)
        {
            return;
        }

        _released = true;
        PageAllocator.Free((void*)Address);
    }
}
