using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.VAS;
using Cosmos.Kernel.Core.X64.Bridge;

namespace Cosmos.Kernel.Core.X64.Memory;

/// <summary>
/// x64 implementation of <see cref="IVirtualMemoryMapper"/>.
/// Manages 4-level page tables with 4 KiB pages.
/// </summary>
internal sealed unsafe class X64VirtualMemoryMapper : IVirtualMemoryMapper
{
    private const int Pml4Shift = 39;
    private const int PdptShift = 30;
    private const int PdShift = 21;
    private const int PtShift = 12;
    private const ulong TableIndexMask = 0x1FF;
    private const ulong PageMask = 0xFFF;

    /// <summary>
    /// Shared singleton instance.
    /// </summary>
    public static readonly X64VirtualMemoryMapper Instance = new X64VirtualMemoryMapper();

    private X64VirtualMemoryMapper()
    {
    }

    /// <inheritdoc />
    public void MapPages(ulong pageTableRoot, ulong virtualAddress, ulong physicalAddress, ulong pageCount, PageFlags flags)
    {
        if ((virtualAddress & PageMask) != 0 || (physicalAddress & PageMask) != 0)
        {
            Panic.Halt("X64VirtualMemoryMapper.MapPages: addresses not page aligned");
        }

        ulong hhdm = GetHhdmOffset();

        for (ulong i = 0; i < pageCount; i++)
        {
            ulong virt = virtualAddress + (i * PageAllocator.PageSize);
            ulong phys = physicalAddress + (i * PageAllocator.PageSize);
            MapSingle4KiB(pageTableRoot, virt, phys, flags, hhdm);
        }
    }

    /// <inheritdoc />
    public void UnmapPages(ulong pageTableRoot, ulong virtualAddress, ulong pageCount)
    {
        if ((virtualAddress & PageMask) != 0)
        {
            Panic.Halt("X64VirtualMemoryMapper.UnmapPages: address not page aligned");
        }

        ulong hhdm = GetHhdmOffset();

        for (ulong i = 0; i < pageCount; i++)
        {
            ulong virt = virtualAddress + (i * PageAllocator.PageSize);
            UnmapSingle4KiB(pageTableRoot, virt, hhdm);
        }
    }

    /// <inheritdoc />
    public ulong CloneHigherHalf(ulong sourceRoot)
    {
        ulong hhdm = GetHhdmOffset();

        Pml4Table* newPml4 = (Pml4Table*)PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (newPml4 == null)
        {
            Panic.Halt("X64VirtualMemoryMapper.CloneHigherHalf: PML4 allocation failed");
        }

        Pml4Table* sourcePml4 = (Pml4Table*)((sourceRoot & PageTableEntry.AddressMask) + hhdm);

        // Copy the higher-half PML4 entries (indices 256..511). Share the lower-level tables.
        for (int i = 256; i < 512; i++)
        {
            newPml4->Entries[i] = sourcePml4->Entries[i];
        }

        return PageAllocator.VirtualToPhysical((ulong)newPml4);
    }

    /// <inheritdoc />
    public void InvalidatePage(ulong virtualAddress)
    {
        X64CpuNative.InvalidatePage(virtualAddress);
    }

    /// <inheritdoc />
    public ulong ReadRoot()
    {
        return X64CpuNative.ReadCr3();
    }

    /// <inheritdoc />
    public void WriteRoot(ulong pageTableRoot)
    {
        X64CpuNative.WriteCr3(pageTableRoot);
    }

    private static void MapSingle4KiB(ulong root, ulong virtualAddress, ulong physicalAddress, PageFlags flags, ulong hhdm)
    {
        Pml4Table* pml4 = (Pml4Table*)((root & PageTableEntry.AddressMask) + hhdm);
        PdptTable* pdpt = GetOrCreatePdpt(pml4, GetIndex(virtualAddress, Pml4Shift), hhdm);
        PdTable* pd = GetOrCreatePd(pdpt, GetIndex(virtualAddress, PdptShift), hhdm);
        PtTable* pt = GetOrCreatePt(pd, GetIndex(virtualAddress, PdShift), hhdm);

        int ptIndex = (int)GetIndex(virtualAddress, PtShift);
        pt->Entries[ptIndex].SetPage(physicalAddress, flags);

        X64CpuNative.InvalidatePage(virtualAddress);
    }

    private static void UnmapSingle4KiB(ulong root, ulong virtualAddress, ulong hhdm)
    {
        Pml4Table* pml4 = (Pml4Table*)((root & PageTableEntry.AddressMask) + hhdm);

        int pml4Index = (int)GetIndex(virtualAddress, Pml4Shift);
        if (!pml4->Entries[pml4Index].Present)
        {
            return;
        }

        PdptTable* pdpt = (PdptTable*)((pml4->Entries[pml4Index].PhysicalAddress) + hhdm);
        int pdptIndex = (int)GetIndex(virtualAddress, PdptShift);
        PdptEntry pdptEntry = pdpt->Entries[pdptIndex];
        if (!pdptEntry.Present || pdptEntry.PageSize)
        {
            return;
        }

        PdTable* pd = (PdTable*)((pdptEntry.PhysicalAddress) + hhdm);
        int pdIndex = (int)GetIndex(virtualAddress, PdShift);
        PdEntry pdEntry = pd->Entries[pdIndex];
        if (!pdEntry.Present || pdEntry.PageSize)
        {
            return;
        }

        PtTable* pt = (PtTable*)((pdEntry.PhysicalAddress) + hhdm);
        int ptIndex = (int)GetIndex(virtualAddress, PtShift);
        pt->Entries[ptIndex].RawValue = 0;

        X64CpuNative.InvalidatePage(virtualAddress);
    }

    private static PdptTable* GetOrCreatePdpt(Pml4Table* pml4, int index, ulong hhdm)
    {
        Pml4Entry entry = pml4->Entries[index];
        if (entry.Present)
        {
            return (PdptTable*)((entry.PhysicalAddress) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Panic.Halt("X64VirtualMemoryMapper: PDPT allocation failed");
        }

        pml4->Entries[index] = Pml4Entry.Table(PageAllocator.VirtualToPhysical((ulong)page));
        return (PdptTable*)page;
    }

    private static PdTable* GetOrCreatePd(PdptTable* pdpt, int index, ulong hhdm)
    {
        PdptEntry entry = pdpt->Entries[index];
        if (entry.Present)
        {
            if (entry.PageSize)
            {
                Panic.Halt("X64VirtualMemoryMapper: encountered 1 GiB page where PD table expected");
            }

            return (PdTable*)((entry.PhysicalAddress) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Panic.Halt("X64VirtualMemoryMapper: PD allocation failed");
        }

        pdpt->Entries[index] = PdptEntry.Table(PageAllocator.VirtualToPhysical((ulong)page));
        return (PdTable*)page;
    }

    private static PtTable* GetOrCreatePt(PdTable* pd, int index, ulong hhdm)
    {
        PdEntry entry = pd->Entries[index];
        if (entry.Present)
        {
            if (entry.PageSize)
            {
                Panic.Halt("X64VirtualMemoryMapper: encountered 2 MiB page where PT table expected");
            }

            return (PtTable*)((entry.PhysicalAddress) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Panic.Halt("X64VirtualMemoryMapper: PT allocation failed");
        }

        pd->Entries[index] = PdEntry.Table(PageAllocator.VirtualToPhysical((ulong)page));
        return (PtTable*)page;
    }

    private static int GetIndex(ulong virtualAddress, int shift)
    {
        return (int)((virtualAddress >> shift) & TableIndexMask);
    }

    private static ulong GetHhdmOffset()
    {
        return Limine.HHDM.Response != null
            ? Limine.HHDM.Response->Offset
            : PageAllocator.DefaultHhdmOffset;
    }
}
