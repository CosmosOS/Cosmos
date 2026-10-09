using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.VAS;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 implementation of <see cref="IVirtualMemoryMapper"/>.
/// Manages 4-level translation tables with a 4 KiB granule.
/// </summary>
internal sealed unsafe class ARM64VirtualMemoryMapper : IVirtualMemoryMapper
{
    private const int L0Shift = 39;
    private const int L1Shift = 30;
    private const int L2Shift = 21;
    private const int L3Shift = 12;
    private const ulong TableIndexMask = 0x1FF;
    private const ulong PageMask = 0xFFF;

    /// <summary>
    /// Shared singleton instance.
    /// </summary>
    public static readonly ARM64VirtualMemoryMapper Instance = new ARM64VirtualMemoryMapper();

    private byte _normalAttrIndx;
    private byte _deviceAttrIndx;
    private bool _mairInitialized;

    private ARM64VirtualMemoryMapper()
    {
    }

    /// <inheritdoc />
    public void MapPages(ulong pageTableRoot, ulong virtualAddress, ulong physicalAddress, ulong pageCount, PageFlags flags)
    {
        if ((virtualAddress & PageMask) != 0 || (physicalAddress & PageMask) != 0)
        {
            Panic.Halt("ARM64VirtualMemoryMapper.MapPages: addresses not page aligned");
        }

        EnsureMairIndices();
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
            Panic.Halt("ARM64VirtualMemoryMapper.UnmapPages: address not page aligned");
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

        L0Table* newL0 = (L0Table*)PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (newL0 == null)
        {
            Panic.Halt("ARM64VirtualMemoryMapper.CloneHigherHalf: L0 allocation failed");
        }

        L0Table* sourceL0 = (L0Table*)((sourceRoot & PageTableEntry.AddressMask) + hhdm);

        // Copy the higher-half L0 entries (indices 256..511). Share lower-level tables.
        for (int i = 256; i < 512; i++)
        {
            newL0->Entries[i] = sourceL0->Entries[i];
        }

        return PageAllocator.VirtualToPhysical((ulong)newL0);
    }

    /// <inheritdoc />
    public void InvalidatePage(ulong virtualAddress)
    {
        ARM64CpuNative.DsbIsb();
        ARM64CpuNative.InvalidateTlb(virtualAddress >> 12);
        ARM64CpuNative.DsbIsb();
    }

    /// <inheritdoc />
    public ulong ReadRoot()
    {
        return ARM64CpuNative.ReadTtbr1();
    }

    /// <inheritdoc />
    public void WriteRoot(ulong pageTableRoot)
    {
        ARM64CpuNative.WriteTtbr1(pageTableRoot);
    }

    private static void MapSingle4KiB(ulong root, ulong virtualAddress, ulong physicalAddress, PageFlags flags, ulong hhdm)
    {
        byte normalAttrIndx = Instance._normalAttrIndx;
        byte deviceAttrIndx = Instance._deviceAttrIndx;

        L0Table* l0 = (L0Table*)((root & PageTableEntry.AddressMask) + hhdm);
        L1Table* l1 = GetOrCreateL1(l0, GetIndex(virtualAddress, L0Shift), hhdm);
        L2Table* l2 = GetOrCreateL2(l1, GetIndex(virtualAddress, L1Shift), hhdm);
        L3Table* l3 = GetOrCreateL3(l2, GetIndex(virtualAddress, L2Shift), hhdm);

        int l3Index = (int)GetIndex(virtualAddress, L3Shift);
        L3Entry existing = l3->Entries[l3Index];

        if (existing.Valid)
        {
            // Break-before-make when replacing an existing page with different attributes.
            l3->Entries[l3Index].RawValue = 0;
            ARM64CpuNative.DsbIsb();
            ARM64CpuNative.InvalidateTlb(virtualAddress >> 12);
            ARM64CpuNative.DsbIsb();
        }

        l3->Entries[l3Index].SetPage(physicalAddress, flags, normalAttrIndx, deviceAttrIndx);

        ARM64CpuNative.DsbIsb();
        ARM64CpuNative.InvalidateTlb(virtualAddress >> 12);
        ARM64CpuNative.DsbIsb();
    }

    private static void UnmapSingle4KiB(ulong root, ulong virtualAddress, ulong hhdm)
    {
        L0Table* l0 = (L0Table*)((root & PageTableEntry.AddressMask) + hhdm);

        int l0Index = (int)GetIndex(virtualAddress, L0Shift);
        if (!l0->Entries[l0Index].Valid)
        {
            return;
        }

        L1Table* l1 = (L1Table*)((l0->Entries[l0Index].PhysicalAddress) + hhdm);
        int l1Index = (int)GetIndex(virtualAddress, L1Shift);
        L1Entry l1Entry = l1->Entries[l1Index];
        if (!l1Entry.Valid || !l1Entry.IsTable)
        {
            return;
        }

        L2Table* l2 = (L2Table*)((l1Entry.PhysicalAddress) + hhdm);
        int l2Index = (int)GetIndex(virtualAddress, L2Shift);
        L2Entry l2Entry = l2->Entries[l2Index];
        if (!l2Entry.Valid || !l2Entry.IsTable)
        {
            return;
        }

        L3Table* l3 = (L3Table*)((l2Entry.PhysicalAddress) + hhdm);
        int l3Index = (int)GetIndex(virtualAddress, L3Shift);
        l3->Entries[l3Index].RawValue = 0;

        ARM64CpuNative.DsbIsb();
        ARM64CpuNative.InvalidateTlb(virtualAddress >> 12);
        ARM64CpuNative.DsbIsb();
    }

    private static L1Table* GetOrCreateL1(L0Table* l0, int index, ulong hhdm)
    {
        L0Entry entry = l0->Entries[index];
        if (entry.Valid)
        {
            return (L1Table*)((entry.PhysicalAddress) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Panic.Halt("ARM64VirtualMemoryMapper: L1 allocation failed");
        }

        l0->Entries[index] = L0Entry.Table(PageAllocator.VirtualToPhysical((ulong)page));
        return (L1Table*)page;
    }

    private static L2Table* GetOrCreateL2(L1Table* l1, int index, ulong hhdm)
    {
        L1Entry entry = l1->Entries[index];
        if (entry.Valid)
        {
            if (!entry.IsTable)
            {
                Panic.Halt("ARM64VirtualMemoryMapper: encountered 1 GiB block where L2 table expected");
            }

            return (L2Table*)((entry.PhysicalAddress) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Panic.Halt("ARM64VirtualMemoryMapper: L2 allocation failed");
        }

        l1->Entries[index] = L1Entry.Table(PageAllocator.VirtualToPhysical((ulong)page));
        return (L2Table*)page;
    }

    private static L3Table* GetOrCreateL3(L2Table* l2, int index, ulong hhdm)
    {
        L2Entry entry = l2->Entries[index];
        if (entry.Valid)
        {
            if (!entry.IsTable)
            {
                Panic.Halt("ARM64VirtualMemoryMapper: encountered 2 MiB block where L3 table expected");
            }

            return (L3Table*)((entry.PhysicalAddress) + hhdm);
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page == null)
        {
            Panic.Halt("ARM64VirtualMemoryMapper: L3 allocation failed");
        }

        l2->Entries[index] = L2Entry.Table(PageAllocator.VirtualToPhysical((ulong)page));
        return (L3Table*)page;
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

    private void EnsureMairIndices()
    {
        if (_mairInitialized)
        {
            return;
        }

        ulong mair = ARM64CpuNative.ReadMair();

        _normalAttrIndx = FindMairIndex(mair, IsNormalAttribute);
        _deviceAttrIndx = FindMairIndex(mair, IsDeviceAttribute);

        if (_normalAttrIndx == byte.MaxValue)
        {
            Panic.Halt("ARM64VirtualMemoryMapper: no Normal memory MAIR index found");
        }

        if (_deviceAttrIndx == byte.MaxValue)
        {
            Panic.Halt("ARM64VirtualMemoryMapper: no Device memory MAIR index found");
        }

        _mairInitialized = true;
    }

    private static byte FindMairIndex(ulong mair, Func<byte, bool> predicate)
    {
        for (int i = 0; i < 8; i++)
        {
            byte attr = (byte)((mair >> (i * 8)) & 0xFF);
            if (predicate(attr))
            {
                return (byte)i;
            }
        }

        return byte.MaxValue;
    }

    private static bool IsNormalAttribute(byte attr)
    {
        // Limine typically uses 0xFF for Normal inner+outer Write-Back Read+Write Allocate.
        // Other common Normal encodings: 0x44 (non-cacheable), 0xF0, 0x0F, etc.
        return attr == 0xFF || attr == 0x44 || attr == 0xF0 || attr == 0x0F;
    }

    private static bool IsDeviceAttribute(byte attr)
    {
        // Device-nGnRnE = 0x00, Device-nGnRE = 0x04, Device-nGRE = 0x08, Device-GRE = 0x0C.
        return attr == 0x00 || attr == 0x04 || attr == 0x08 || attr == 0x0C;
    }
}
