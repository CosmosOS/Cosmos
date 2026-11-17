// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The pool every queue head and transfer descriptor comes from: 4 KiB DMA
/// pages carved into 64-byte slots (a queue head is 48 bytes, a qTD 32,
/// both 32-byte aligned), with a free list the memory sets take from and
/// return to. The pages are allocated once on the host's binding and never
/// freed early; the page array is replaced whole when a page is added.
/// Every member runs under the controller's lock, in thread context; the
/// handler never touches the pool.
/// </summary>
internal sealed class EhciDescriptorPool
{
    /// <summary>Bytes of one slot: the smallest power of two holding a queue head.</summary>
    public const int SlotBytes = 64;

    /// <summary>Bytes of one pool page.</summary>
    public const int PageBytes = 4096;

    /// <summary>Slots in one page.</summary>
    public const int SlotsPerPage = PageBytes / SlotBytes;

    private readonly List<EhciDescriptorSlot> _free = [];
    private DmaBuffer[] _pages = [];

    /// <summary>Pages the pool holds. Any context.</summary>
    public int PageCount => _pages.Length;

    /// <summary>Slots on the free list. Under the lock.</summary>
    public int FreeCount => _free.Count;

    /// <summary>
    /// Adds a page the host allocated, zeroed and page aligned, carving it
    /// into slots on the free list. The page array is replaced whole.
    /// Under the lock.
    /// </summary>
    /// <param name="page">One page of DMA memory below 4 GiB.</param>
    public void AddPage(DmaBuffer page)
    {
        DmaBuffer[] pages = new DmaBuffer[_pages.Length + 1];
        _pages.CopyTo(pages, 0);
        pages[_pages.Length] = page;
        _pages = pages;

        for (int index = 0; index < SlotsPerPage; index++)
        {
            _free.Add(new EhciDescriptorSlot(page, index));
        }
    }

    /// <summary>Takes a zeroed slot off the free list. Under the lock.</summary>
    /// <param name="slot">The slot, when one was free.</param>
    /// <returns>False when the free list is empty and a page must be added first.</returns>
    public bool TryTake(out EhciDescriptorSlot slot)
    {
        int last = _free.Count - 1;
        if (last < 0)
        {
            slot = default;
            return false;
        }

        slot = _free[last];
        _free.RemoveAt(last);
        return true;
    }

    /// <summary>Zeroes a slot the controller no longer holds and puts it back on the free list. Under the lock.</summary>
    /// <param name="slot">A slot taken from this pool.</param>
    public void Return(EhciDescriptorSlot slot)
    {
        slot.Clear();
        _free.Add(slot);
    }
}
