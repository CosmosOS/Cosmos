// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// The pool every ED, TD and SETUP packet comes from: pages below 4 GiB
/// carved into 16-byte slots, a free list, and the owner and generation of
/// each slot. The page array is replaced whole when a page is added, so
/// the handler reads a consistent array without the lock; the free list
/// and the slot records are written under the controller's lock. A slot
/// the controller may still hold is never put back (the caller parks it on
/// the controller's retired list instead). Nothing is ever freed: the kit
/// frees the pages in the binding's teardown.
/// </summary>
internal sealed class OhciDescriptorPool
{
    private readonly List<OhciDescriptorSlot> _free = [];
    private OhciDescriptorPage[] _pages = [];

    /// <summary>How many pages the pool holds. Any context.</summary>
    public int PageCount => _pages.Length;

    /// <summary>How many slots are free. Under the lock.</summary>
    public int FreeCount => _free.Count;

    /// <summary>Adds a page the host allocated and wrapped outside the lock: every slot of it joins the free list. Under the lock, or in the probe before the lock exists.</summary>
    /// <param name="page">A wrapped page of DMA memory below 4 GiB, page aligned, zeroed.</param>
    public void AddPage(OhciDescriptorPage page)
    {
        OhciDescriptorPage[] pages = new OhciDescriptorPage[_pages.Length + 1];
        _pages.CopyTo(pages, 0);
        pages[_pages.Length] = page;
        for (int index = OhciProtocol.DescriptorsPerPage - 1; index >= 0; index--)
        {
            _free.Add(new OhciDescriptorSlot(page, index));
        }

        _pages = pages;
    }

    /// <summary>Takes a free slot, its owner cleared. Under the lock.</summary>
    /// <param name="slot">The slot taken.</param>
    /// <returns>False when the pool is empty; the caller adds a page and tries again.</returns>
    public bool TryTake(out OhciDescriptorSlot slot)
    {
        int last = _free.Count - 1;
        if (last < 0)
        {
            slot = OhciDescriptorSlot.None;
            return false;
        }

        slot = _free[last];
        _free.RemoveAt(last);
        return true;
    }

    /// <summary>Gives a slot back once the controller can no longer name it. Under the lock.</summary>
    /// <param name="slot">A slot this pool handed out.</param>
    public void Return(OhciDescriptorSlot slot)
    {
        if (slot.Page is null)
        {
            return;
        }

        slot.Page.Owners[slot.Index] = null;
        _free.Add(slot);
    }

    /// <summary>Records which device or pipe a slot's TD belongs to. Under the lock.</summary>
    /// <param name="slot">A TD slot.</param>
    /// <param name="owner">The device or pipe, or null when the slot carries no TD.</param>
    public static void SetOwner(OhciDescriptorSlot slot, object? owner)
    {
        slot.Page?.Owners[slot.Index] = owner;
    }

    /// <summary>Bumps a slot's generation for the TD about to be queued in it. Under the lock.</summary>
    /// <param name="slot">A TD slot.</param>
    /// <returns>The new generation, which the owner stores with the TD.</returns>
    public static int NextTag(OhciDescriptorSlot slot)
    {
        if (slot.Page is null)
        {
            return 0;
        }

        return ++slot.Page.Tags[slot.Index];
    }

    /// <summary>
    /// Maps a done queue address back to its slot, owner and current
    /// generation. Any context, allocation-free: the page array is read
    /// whole and the records beside it belong to the pages themselves.
    /// </summary>
    /// <param name="address">A TD's physical address, from the done queue.</param>
    /// <param name="slot">The slot at that address.</param>
    /// <param name="owner">The device or pipe recorded for the slot.</param>
    /// <param name="tag">The slot's current generation.</param>
    /// <returns>False when the address lies outside every pool page or is not slot aligned.</returns>
    public bool TryFindByPhysicalAddress(uint address, out OhciDescriptorSlot slot, out object? owner, out int tag)
    {
        OhciDescriptorPage[] pages = _pages;
        for (int i = 0; i < pages.Length; i++)
        {
            OhciDescriptorPage page = pages[i];
            ulong start = page.PhysicalAddress;
            if (address < start || address >= start + (ulong)page.Buffer.Length)
            {
                continue;
            }

            ulong offset = address - start;
            if (offset % OhciProtocol.DescriptorBytes != 0)
            {
                break;
            }

            int index = (int)(offset / OhciProtocol.DescriptorBytes);
            slot = new OhciDescriptorSlot(page, index);
            owner = page.Owners[index];
            tag = page.Tags[index];
            return true;
        }

        slot = OhciDescriptorSlot.None;
        owner = null;
        tag = 0;
        return false;
    }
}
