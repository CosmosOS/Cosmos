// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 implementation of <see cref="IPagerBackend"/>: a three-level walk over
/// the TTBR1 tables Limine left installed.
/// </summary>
/// <remarks>
/// This is the paging machinery <c>DeviceMapper</c> does a single fixed-purpose
/// version of. That one can only install 2 MiB Device-nGnRnE blocks at the HHDM
/// alias and depends on a single pre-allocated L2 table in <c>.bss</c>, which is
/// consumed on first use and can never serve a second split. This backend
/// allocates every intermediate table from the frame allocator instead, so the
/// number of mappings is not bounded by a linker symbol.
/// <para>
/// One structural difference from x64 is worth stating plainly: ARM64
/// attributes live in MAIR_EL1, not in the descriptor. A descriptor only names
/// an MAIR index in bits [4:2], so this backend reads the live MAIR to pick an
/// index and cannot invent one — writing a new MAIR_EL1 is out of scope here
/// precisely because it would change the attributes of every existing mapping.
/// </para>
/// </remarks>
internal sealed unsafe class Arm64PagerBackend : IPagerBackend
{
    // ARM64 descriptor bits (Arm ARM DDI 0487, Table D5-30).
    private const ulong DescValid = 1UL << 0;
    private const ulong DescTable = 1UL << 1;   // 1 = table descriptor, 0 = block
    private const ulong DescAf = 1UL << 10;      // access flag
    private const ulong DescPxN = 1UL << 53;
    private const ulong DescUxN = 1UL << 54;

    /// <summary>Physical-address field shared by table and block descriptors.</summary>
    private const ulong AddrMask = 0x0000_FFFF_FFFF_F000UL;
    private const ulong Block2MiBAddrMask = 0x0000_FFFF_FFE0_0000UL;
    private const ulong Block1GiBAddrMask = 0x0000_FFFF_C000_0000UL;

    /// <summary>Mask selecting the 9-bit table index (512 entries per table).</summary>
    private const ulong TableIndexMask = 0x1FF;

    /// <summary>AttrIndx selector occupies bits [4:2] of a block descriptor.</summary>
    private const int DescAttrIndexShift = 2;

    /// <summary>
    /// AP[2]: the block is writable from EL1. Without it a write traps even to
    /// Normal memory.
    /// </summary>
    private const ulong DescApEl1Writable = 1UL << 2;

    /// <summary>
    /// Index shift for each level. TTBR1 is configured by Limine with T1SZ=16,
    /// so the virtual address is used directly: L0 at bits [47:39], L1 at
    /// [38:30], L2 at [29:21], L3 at [20:12].
    /// </summary>
    private const int L0Shift = 39;
    private const int L1Shift = 30;
    private const int L2Shift = 21;
    private const int L3Shift = 12;

    private const ulong PagesPer2MiB = 512;

    /// <summary>
    /// A resolved MAIR attribute index plus whether it is Device memory.
    /// </summary>
    /// <remarks>
    /// The distinction is load-bearing rather than cosmetic: Device attributes
    /// are architecturally read-only, so a block using one must leave AP[2]
    /// clear no matter what <see cref="PageFlags.Writable"/> asks for.
    /// </remarks>
    private readonly struct MairAttr(ulong index, bool isDevice)
    {
        /// <summary>AttrIndx value placed in bits [4:2] of a block descriptor.</summary>
        public ulong Index { get; } = index;

        /// <summary>True when the attribute is Device-nGnRnE or Device-nGnRE.</summary>
        public bool IsDevice { get; } = isDevice;
    }

    /// <inheritdoc />
    public bool IsAvailable => true;

    /// <inheritdoc />
    public ulong CurrentRootFrame() => DeviceMapperNative.ReadTtbr1() & AddrMask;

    /// <inheritdoc />
    public void Commit(AddressSpaceDescriptor* space)
    {
        ulong rootFrame = space->RootFrame;
        if (rootFrame == 0)
        {
            return;
        }

        ulong hhdm = GetHhdmOffset();
        if (hhdm == 0)
        {
            Serial.WriteString("[Pager] Commit: no HHDM, cannot reach page tables\n");
            return;
        }

        ulong* l0 = (ulong*)(rootFrame + hhdm);

        // MAIR is resolved per region rather than once for the whole space: a
        // Device region and a Normal RAM region need different attribute
        // indices, and picking one for both is what made the first round-trip
        // test trap on write.
        for (int i = 0; i < space->RegionCount; i++)
        {
            ref AddressSpaceRegion region = ref space->Regions[i];
            InstallRegion(l0, in region, ResolveAttrIndex(region.Flags), hhdm);
        }
    }

    /// <inheritdoc />
    public bool Translate(AddressSpaceDescriptor* space, VirtualAddress virt, out PhysicalAddress phys)
    {
        phys = PhysicalAddress.None;

        if (space->RootFrame == 0)
        {
            return false;
        }

        ulong hhdm = GetHhdmOffset();
        if (hhdm == 0)
        {
            return false;
        }

        ulong address = virt.Value;
        if ((address & (PageAllocator.PageSize - 1)) != 0)
        {
            return false;
        }

        if (!Walk((ulong*)(space->RootFrame + hhdm), address, hhdm, out ulong frame))
        {
            return false;
        }

        phys = PhysicalAddress.From(frame);
        return true;
    }

    /// <inheritdoc />
    public bool TranslateCurrent(VirtualAddress virt, out PhysicalAddress phys)
    {
        ulong rootFrame = CurrentRootFrame();
        ulong hhdm = GetHhdmOffset();

        if (rootFrame == 0 || hhdm == 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        ulong address = virt.Value;
        if ((address & (PageAllocator.PageSize - 1)) != 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        if (!Walk((ulong*)(rootFrame + hhdm), address, hhdm, out ulong frame))
        {
            phys = PhysicalAddress.None;
            return false;
        }

        phys = PhysicalAddress.From(frame);
        return true;
    }

    /// <summary>
    /// Resolves a virtual address to a physical frame, stopping at the first
    /// block descriptor it meets.
    /// </summary>
    private static bool Walk(ulong* l0, ulong address, ulong hhdm, out ulong frame)
    {
        frame = 0;

        ulong l0Entry = ReadEntry(l0, (int)((address >> L0Shift) & TableIndexMask));
        if (l0Entry == 0 || (l0Entry & DescTable) == 0)
        {
            return false;
        }

        ulong* l1 = (ulong*)((l0Entry & AddrMask) + hhdm);

        ulong l1Entry = ReadEntry(l1, (int)((address >> L1Shift) & TableIndexMask));
        if (l1Entry == 0)
        {
            return false;
        }

        if ((l1Entry & DescTable) == 0)
        {
            // 1 GiB block.
            frame = (l1Entry & Block1GiBAddrMask) + (address & 0x3FFF_FFFF);
            return true;
        }

        ulong* l2 = (ulong*)((l1Entry & AddrMask) + hhdm);

        ulong l2Entry = ReadEntry(l2, (int)((address >> L2Shift) & TableIndexMask));
        if (l2Entry == 0)
        {
            return false;
        }

        if ((l2Entry & DescTable) == 0)
        {
            // 2 MiB block.
            frame = (l2Entry & Block2MiBAddrMask) + (address & 0x1F_FFFF);
            return true;
        }

        ulong* l3 = (ulong*)((l2Entry & AddrMask) + hhdm);

        ulong l3Entry = ReadEntry(l3, (int)((address >> L3Shift) & TableIndexMask));
        if (l3Entry == 0)
        {
            return false;
        }

        if ((l3Entry & DescTable) != 0)
        {
            // A table descriptor at L3 is not a valid leaf.
            return false;
        }

        frame = l3Entry & AddrMask;
        return true;
    }

    /// <summary>
    /// Reads one entry and returns it only when it is valid, so callers can
    /// treat 0 as "not mapped".
    /// </summary>
    private static ulong ReadEntry(ulong* table, int index)
    {
        ulong entry = table[index];
        return (entry & DescValid) != 0 ? entry : 0;
    }

    /// <summary>
    /// Installs one region as 2 MiB blocks where alignment allows, and as 4 KiB
    /// pages otherwise.
    /// </summary>
    private static void InstallRegion(ulong* l0, in AddressSpaceRegion region, MairAttr attrIndex, ulong hhdm)
    {
        if (region.PageCount == 0)
        {
            return;
        }

        ulong virt = region.Virt.Value;
        ulong phys = region.Phys.Value;
        ulong attributes = EncodeFlags(region.Flags, attrIndex);

        if (region.PageCount == PagesPer2MiB && (virt & 0x1F_FFFF) == 0 && (phys & 0x1F_FFFF) == 0)
        {
            if (Install2MiB(l0, virt, phys, attributes, hhdm))
            {
                Publish(virt);
                return;
            }
        }

        ulong cursor = virt;
        ulong remaining = region.PageCount;
        ulong frame = phys;

        while (remaining > 0)
        {
            if (!Install4KiB(l0, cursor, frame, attributes, hhdm))
            {
                return;
            }

            Publish(cursor);
            cursor += PageAllocator.PageSize;
            frame += PageAllocator.PageSize;
            remaining--;
        }
    }

    /// <summary>
    /// Makes a freshly written descriptor visible to translation: DSB SY so the
    /// store has retired, then the per-address TLB invalidation, then ISB.
    /// </summary>
    /// <remarks>
    /// The barrier has to come <em>before</em> the TLBI. Issuing a TLB
    /// invalidation against a descriptor the CPU cannot yet see leaves the old
    /// translation cached and the new one unusable, which is what a permission
    /// fault on a page that was demonstrably written a moment earlier looks
    /// like. This ordering mirrors what <c>DeviceMapper</c> does for the same
    /// reason.
    /// </remarks>
    private static void Publish(ulong virt)
    {
        DeviceMapperNative.DsbIsb();
        DeviceMapperNative.FlushTlb(virt >> 12);
    }

    /// <summary>
    /// Installs a single 4 KiB page as an L3 block descriptor, allocating any
    /// missing table.
    /// </summary>
    private static bool Install4KiB(ulong* l0, ulong virt, ulong phys, ulong attributes, ulong hhdm)
    {
        ulong* l3 = ResolveTablesToL3(l0, virt, hhdm);
        if (l3 is null)
        {
            return false;
        }

        l3[(int)((virt >> L3Shift) & TableIndexMask)] = phys | attributes | DescValid;
        return true;
    }

    /// <summary>
    /// Installs a single 2 MiB page as an L2 block descriptor.
    /// </summary>
    private static bool Install2MiB(ulong* l0, ulong virt, ulong phys, ulong attributes, ulong hhdm)
    {
        ulong* l1 = GetOrCreateTable(l0, (int)((virt >> L0Shift) & TableIndexMask), hhdm);
        if (l1 is null)
        {
            return false;
        }

        int l1Index = (int)((virt >> L1Shift) & TableIndexMask);
        ulong l1Entry = ReadEntry(l1, l1Index);
        if (l1Entry != 0 && (l1Entry & DescTable) == 0)
        {
            // Already a 1 GiB block covering this range.
            return true;
        }

        ulong* l2 = GetOrCreateTable(l1, l1Index, hhdm);
        if (l2 is null)
        {
            return false;
        }

        l2[(int)((virt >> L2Shift) & TableIndexMask)] = phys | attributes | DescValid;
        return true;
    }

    /// <summary>
    /// Walks (creating as needed) down to the L3 table containing
    /// <paramref name="virt"/>. Returns null when an intermediate level is a
    /// block descriptor that would have to be split, or an allocation fails.
    /// </summary>
    private static ulong* ResolveTablesToL3(ulong* l0, ulong virt, ulong hhdm)
    {
        ulong* l1 = GetOrCreateTable(l0, (int)((virt >> L0Shift) & TableIndexMask), hhdm);
        if (l1 is null)
        {
            return null;
        }

        int l1Index = (int)((virt >> L1Shift) & TableIndexMask);
        ulong l1Entry = ReadEntry(l1, l1Index);
        if (l1Entry != 0 && (l1Entry & DescTable) == 0)
        {
            Serial.WriteString("[Pager] 4KiB map under a 1GiB block not supported: virt 0x");
            Serial.WriteHex(virt);
            Serial.WriteString("\n");
            return null;
        }

        ulong* l2 = GetOrCreateTable(l1, l1Index, hhdm);
        if (l2 is null)
        {
            return null;
        }

        int l2Index = (int)((virt >> L2Shift) & TableIndexMask);
        ulong l2Entry = ReadEntry(l2, l2Index);
        if (l2Entry != 0 && (l2Entry & DescTable) == 0)
        {
            Serial.WriteString("[Pager] 4KiB map under a 2MiB block not supported: virt 0x");
            Serial.WriteHex(virt);
            Serial.WriteString("\n");
            return null;
        }

        return GetOrCreateTable(l2, l2Index, hhdm);
    }

    /// <summary>
    /// Follows <paramref name="parent"/>[<paramref name="index"/>] to its child
    /// table, allocating and linking a zeroed one when absent. Returns null when
    /// the entry is a block descriptor or an allocation fails.
    /// </summary>
    /// <remarks>
    /// This is where <c>DeviceMapper</c> differs: it uses a single
    /// <c>.bss</c> table guarded by a one-shot flag, which is why it can only
    /// ever split one 1 GiB block. Allocating per split costs a frame from the
    /// same pool the rest of the kernel uses, but it is not bounded to one use.
    /// </remarks>
    private static ulong* GetOrCreateTable(ulong* parent, int index, ulong hhdm)
    {
        ulong entry = parent[index];
        if ((entry & DescValid) != 0)
        {
            return (entry & DescTable) != 0
                ? (ulong*)((entry & AddrMask) + hhdm)
                : null;
        }

        void* page = PageAllocator.AllocPages(PageType.PageDirectory, 1, zero: true);
        if (page is null)
        {
            Serial.WriteString("[Pager] ERROR: page-table allocation failed\n");
            return null;
        }

        // PageAllocator hands back an address in whatever space it manages the
        // heap in: identity (phys == virt) on ARM64, HHDM-aliased on x64. The
        // descriptor needs the frame; the walk needs a dereferenceable pointer.
        // Resolving the frame with VirtualToPhysical handles both layouts, and
        // the HHDM alias of that frame is then always dereferenceable because
        // the HHDM covers all of RAM.
        ulong tableFrame = PageAllocator.VirtualToPhysical((ulong)page);
        ulong* table = (ulong*)(tableFrame + hhdm);

        // Table descriptors carry no AP/UXN of their own: permissions come from
        // the block descriptors at the level above.
        parent[index] = tableFrame | DescValid | DescTable;

        // The walk continues into this table immediately, so the link has to be
        // observable before the caller writes a leaf into it. Without the
        // barrier the CPU can read the parent's entry before the store lands
        // and fault against a descriptor that is valid in memory.
        DeviceMapperNative.DsbIsb();

        return table;
    }

    /// <summary>
    /// Translates the platform-agnostic <see cref="PageFlags"/> into ARM64
    /// descriptor bits, using <paramref name="mair"/> for the MAIR
    /// selector.
    /// </summary>
    /// <remarks>
    /// AF is always set. Hardware sets and clears it on access; pre-setting it
    /// means an entry does not fault on its first touch purely to record the
    /// access, and it is required for block descriptors that are not
    /// update-recorded.
    /// </remarks>
    private static ulong EncodeFlags(PageFlags flags, MairAttr mair)
    {
        ulong bits = DescAf | ((mair.Index & 0x7UL) << DescAttrIndexShift);

        if ((flags & PageFlags.Executable) == 0)
        {
            // PXN blocks kernel fetch, UXN blocks EL0 fetch. Both are set
            // because an executable-only-for-one-level mapping is not expressible
            // without two bits, and non-executable means non-executable.
            bits |= DescPxN | DescUxN;
        }

        // AP[2] is the EL1-writable bit; a block descriptor with it clear
        // rejects writes regardless of the MAIR attribute. Device memory
        // (Device-nGnRnE / Device-nGnRE) is architecturally non-writable, so
        // AP[2] must stay clear there — setting it would not grant write access,
        // it would just misreport the mapping as writable.
        //
        // For Normal memory AP[2] is what makes PageFlags.Writable real. Without
        // this bit every write to a mapped page traps, which is exactly the
        // fault the first round-trip test caught.
        if ((flags & PageFlags.Writable) != 0 && !mair.IsDevice)
        {
            bits |= DescApEl1Writable;
        }

        // PageFlags.User is not applied: it needs the EL0-accessible pair of
        // AP[2] plus the absence of UXN, and there is no ring-3 transition in
        // the kernel today to validate that against.
        //
        // PageFlags.Global has no ARM64 equivalent at all: TLBI invalidation is
        // issued at EL1 without an address-space tag, so there is no per-entry
        // global-page concept to set. Shared-on-swap mappings are a property of
        // the regime, not of a descriptor.

        return bits;
    }

    /// <summary>
    /// Picks a MAIR attribute index for device memory, reusing whatever Limine
    /// configured.
    /// </summary>
    /// <remarks>
    /// Limine's MAIR already contains a Device-nGnRnE (0x00) or Device-nGnRE
    /// (0x04) attribute, so the existing index is used rather than rewriting
    /// MAIR_EL1 — that register governs every live mapping, and changing it
    /// would silently re-attribute pages this backend does not own.
    /// <see cref="PageFlags.Cacheable"/> and <see cref="PageFlags.Uncached"/>
    /// cannot be distinguished here for the same reason: selecting a Normal
    /// Non-cacheable attribute would require adding a MAIR entry.
    /// </remarks>
    private static MairAttr ResolveAttrIndex(PageFlags flags)
    {
        ulong mair = DeviceMapperNative.ReadMair();
        bool wantsDevice = (flags & PageFlags.Device) != 0;

        int normalIndex = -1;
        int deviceIndex = -1;

        for (int i = 0; i < 8; i++)
        {
            byte attr = (byte)((mair >> (i * 8)) & 0xFF);

            // Device-nGnRnE (0x00) and Device-nGnRE (0x04) are the only two
            // attributes this kernel uses for MMIO. Both are read-only, so the
            // index is only usable for a region that asked for Device.
            if (attr is 0x00 or 0x04)
            {
                if (deviceIndex < 0)
                {
                    deviceIndex = i;
                }
            }
            else if (normalIndex < 0 && IsNormalCacheable(attr))
            {
                normalIndex = i;
            }
        }

        if (wantsDevice)
        {
            return deviceIndex >= 0
                ? new MairAttr((ulong)deviceIndex, true)
                : new MairAttr(0, false);
        }

        // Normal memory is the common case, and it must resolve to a
        // cacheable attribute: mapping RAM through the Device index silently
        // makes every write trap (Device memory is not writable).
        return normalIndex >= 0
            ? new MairAttr((ulong)normalIndex, false)
            : new MairAttr(0, false);
    }

    /// <summary>
    /// Recognises the write-back cacheable Normal attributes (Arm ARM DDI 0487,
    /// Table D2-12): Normal Write-Back, Write-Back No Write-Allocate, and
    /// Write-Back Transparent.
    /// </summary>
    private static bool IsNormalCacheable(byte attr)
    {
        return attr is 0xFF or 0xFE or 0xFC;
    }

    /// <summary>
    /// The HHDM offset, or 0 when Limine did not answer.
    /// </summary>
    private static ulong GetHhdmOffset()
    {
        return Limine.HHDM.Response is null
            ? 0
            : Limine.HHDM.Response->Offset;
    }
}