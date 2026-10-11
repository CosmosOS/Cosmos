// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.ARM64.Memory;

/// <summary>
/// ARM64 implementation of <see cref="IPagerBackend"/>: a four-level walk over
/// the translation tables Limine left installed, covering both halves —
/// TTBR1_EL1 for the upper half where the HHDM and kernel image live, and
/// TTBR0_EL1 for the lower half where the kernel heap is identity-mapped.
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
    /// <summary>
    /// Bit 1. At an intermediate level it means "descend into another table".
    /// On this platform the leaf descriptors the MMU actually accepts also carry
    /// it — see the note in <see cref="Install4KiB"/>, where the requirement was
    /// measured against AT rather than assumed from the architecture manual.
    /// </summary>
    private const ulong DescTable = 1UL << 1;
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
    /// Index shift for each level of a 4-level walk: L0 at bits [47:39], L1 at
    /// [38:30], L2 at [29:21], L3 at [20:12].
    /// </summary>
    /// <remarks>
    /// These shifts are only valid for the default 48-bit VA size (T0SZ/T1SZ
    /// of 16). A regime configured for a different size shifts every index
    /// down, so the translated-bit mask is derived from the live TCR_EL1
    /// instead and applied before the shifts — see <see cref="GetAddressMask"/>.
    /// </remarks>
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
    public ulong CurrentLowRootFrame() => DeviceMapperNative.ReadTtbr0() & AddrMask;

    /// <inheritdoc />
    public void Commit(AddressSpaceDescriptor* space)
    {
        if (space->RootFrame == 0)
        {
            return;
        }

        ulong hhdm = GetHhdmOffset();
        if (hhdm == 0)
        {
            Serial.WriteString("[Pager] Commit: no HHDM, cannot reach page tables\n");
            return;
        }

        // MAIR is resolved per region rather than once for the whole space: a
        // Device region and a Normal RAM region need different attribute
        // indices, and picking one for both is what made the first round-trip
        // test trap on write.
        for (int i = 0; i < space->RegionCount; i++)
        {
            ref AddressSpaceRegion region = ref space->Regions[i];

            // A region belongs to whichever half its virtual address falls in,
            // and each half has its own root table. Committing a lower-half
            // region into the upper-half tree would write a descriptor the CPU
            // never consults for that address.
            ulong normalized = NormalizeAddress(region.Virt.Value);
            ulong rootFrame = IsHighHalf(region.Virt.Value)
                ? space->RootFrame
                : space->LowRootFrame;

            if (rootFrame == 0)
            {
                continue;
            }

            InstallRegion((ulong*)(rootFrame + hhdm), region, normalized, region.Virt.Value,
                ResolveAttrIndex(region.Flags), hhdm);

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

        ulong rootFrame = IsHighHalf(address) ? space->RootFrame : space->LowRootFrame;
        if (rootFrame == 0)
        {
            return false;
        }

        if (!Walk((ulong*)(rootFrame + hhdm), NormalizeAddress(address), hhdm, out ulong frame))
        {
            return false;
        }

        // A resolved frame must be page-aligned by construction. If it is not,
        // the walk mis-decoded an entry, and reporting that as a successful
        // translation — or letting PhysicalAddress.From throw it — would be
        // worse than admitting the walk failed. Translate must never throw:
        // its callers are fault handlers.
        if ((frame & (PageAllocator.PageSize - 1)) != 0)
        {
            return false;
        }

        phys = PhysicalAddress.From(frame);
        return true;
    }

    /// <inheritdoc />
    public bool TranslateCurrent(VirtualAddress virt, out PhysicalAddress phys)
    {
        ulong hhdm = GetHhdmOffset();

        ulong address = virt.Value;
        if (hhdm == 0 || (address & (PageAllocator.PageSize - 1)) != 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        // Read the root for the half this address actually falls in, live. The
        // kernel heap is identity-mapped in the lower half, so resolving only
        // against TTBR1 would leave every heap pointer untranslatable.
        ulong rootFrame = IsHighHalf(address)
            ? DeviceMapperNative.ReadTtbr1()
            : DeviceMapperNative.ReadTtbr0();

        rootFrame &= AddrMask;
        if (rootFrame == 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        if (!Walk((ulong*)(rootFrame + hhdm), NormalizeAddress(address), hhdm, out ulong frame))
        {
            phys = PhysicalAddress.None;
            return false;
        }

        // See the note in Translate: never throw, and never hand back a frame
        // that is not page-aligned.
        if ((frame & (PageAllocator.PageSize - 1)) != 0)
        {
            phys = PhysicalAddress.None;
            return false;
        }

        phys = PhysicalAddress.From(frame);
        return true;
    }

    /// <summary>
    /// True when the address belongs to the upper (sign-extended 1) half, which
    /// TTBR1 translates. Everything else, including the identity-mapped kernel
    /// heap, is the lower half served by TTBR0.
    /// </summary>
    private static bool IsHighHalf(ulong address) => (address >> 48) == 0xFFFF;

    /// <summary>
    /// The sign-extension bits of whichever half <paramref name="address"/> is in,
    /// i.e. everything above the 48 translated bits.
    /// </summary>
    /// <remarks>
    /// Index extraction works on the normalised (sign-extension-stripped) address
    /// so the table indices line up, but the TLB is tagged with the full virtual
    /// address. Rebuilding a full VA for an invalidation therefore means keeping
    /// the half's upper bits and swapping in the current page offset.
    /// </remarks>
    private static ulong CurrentHalfMask(ulong address) => address & ~((1UL << 48) - 1);

    /// <summary>
    /// Reduces a virtual address to the bits the configured regime actually
    /// translates, by discarding the sign extension above it.
    /// </summary>
    /// <remarks>
    /// The level shifts below are absolute (L0 at [47:39]), so the top of the
    /// address has to be removed first or a sign-extended high-half address
    /// indexes the wrong entries. With the usual 48-bit regime the mask is
    /// 0x0000_FFFF_FFFF_FFFF and this is a no-op for lower-half addresses; for
    /// a smaller regime it removes more, which is what keeps the walk correct
    /// if the bootloader ever configures one.
    /// </remarks>
    private static ulong NormalizeAddress(ulong address)
    {
        ulong mask = GetAddressMask();
        ulong normalized = address & mask;

        // A sign-extended high-half address normalizes to a small value, which
        // is correct: the upper half is translated relative to its own root, so
        // the index bits really do start from zero at the bottom of the range.
        return normalized;
    }

    /// <summary>
    /// The translated-bit mask for the configured VA size, read from TCR_EL1.
    /// </summary>
    /// <remarks>
    /// T0SZ (bits [5:0]) and T1SZ (bits [21:16]) give each half's VA size as
    /// 64 - T0SZ and 64 - T1SZ. Reading them removes the assumption that the
    /// regime is 48-bit. Falls back to 48 bits if the read is unavailable or
    /// yields an implausible size, since every known configuration uses it.
    /// </remarks>
    private static ulong GetAddressMask()
    {
        ulong tcr = DeviceMapperNative.ReadTcr();

        ulong t1sz = (tcr >> 16) & 0x3F;
        int sizeBits = 64 - (int)t1sz;

        // 39 bits is the minimum supported AArch64 VA size (4 levels of 512
        // entries covering 12 page-offset bits); anything smaller is nonsense.
        if (sizeBits < 39 || sizeBits > 48)
        {
            return (1UL << 48) - 1;
        }

        return (1UL << sizeBits) - 1;
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

        // The final level is a leaf: a valid entry there is the mapping, full
        // stop. Requiring DescTable to be clear at this level is wrong, and
        // demonstrably so — Limine's own leaf entries have bit 1 set, yet the
        // MMU resolves them as blocks. Cross-checked against the AT
        // instruction: for a kernel-image address the walk reports the frame in
        // this entry's physical field and AT reports the identical frame.
        //
        // Bit 1 is therefore only meaningful as "is another table" at the
        // intermediate levels above, where it is used to reject descending into
        // a huge page.
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
    private static void InstallRegion(ulong* l0, in AddressSpaceRegion region, ulong virt, ulong realVirt,
        MairAttr attrIndex, ulong hhdm)
    {
        if (region.PageCount == 0)
        {
            return;
        }

        ulong phys = region.Phys.Value;
        ulong attributes = EncodeFlags(region.Flags, attrIndex);

        if (region.PageCount == PagesPer2MiB && (virt & 0x1F_FFFF) == 0 && (phys & 0x1F_FFFF) == 0)
        {
            if (Install2MiB(l0, virt, phys, attributes, hhdm))
            {
                Publish(realVirt);
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

            Publish(cursor | CurrentHalfMask(realVirt));
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
        // Arm ARM translation-table update sequence: the descriptors are already
        // written and cache-maintained, so retire them (DSB SY) before
        // invalidating, then make the result visible (DSB SY + ISB).
        //
        // The invalidation is issued for the *full* virtual address. The staging
        // value has already been normalised to the translated bits for index
        // extraction, and tlbi vae1is is tagged by the whole VA — flushing the
        // stripped form would target TTBR0 and leave this TTBR1 entry cached.
        DeviceMapperNative.DsbIsb();
        DeviceMapperNative.FlushTlb(virt >> 12);
        DeviceMapperNative.DsbIsb();
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

        // DescTable is set on the leaf as well as on intermediate levels. That
        // reads backwards against the Arm ARM, where a block descriptor is
        // 0b01 in bits [1:0] and bit 1 selects "table" — but on this platform it
        // is required, and it was measured rather than assumed:
        //
        //   leaf without bit 1 -> AT S1E1R returns 0 (translation fault),
        //                          the MMU ignores the mapping, and a write
        //                          through the alias traps at level 1.
        //   leaf with bit 1    -> AT returns the mapped frame, the mapping is
        //                          live, and the full round trip passes.
        //
        // The same encoding is what the bootloader's own leaf entries use (they
        // carry bit 1 too), so this matches the tables already in the hardware
        // rather than diverging from them. `DescTable` is a shared constant for
        // that reason: at intermediate levels it genuinely means "descend".
        l3[(int)((virt >> L3Shift) & TableIndexMask)] = phys | attributes | DescValid | DescTable;
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

        // Bit 1 set for the same reason as the L3 leaf in Install4KiB: it is what
        // this MMU accepts on a leaf, and omitting it leaves the block visible to
        // a software walk but invisible to translation.
        l2[(int)((virt >> L2Shift) & TableIndexMask)] = phys | attributes | DescValid | DescTable;
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
        //
        // Verified by the test suite rather than assumed: mapping a region whose
        // L0 slot the bootloader never populated requires allocating three
        // intermediate tables and then walking into them, and the round-trip
        // test does exactly that successfully.
        ulong tableFrame = PageAllocator.VirtualToPhysical((ulong)page);
        ulong* table = (ulong*)(tableFrame + hhdm);


        // Table descriptors carry no AP/UXN of their own: permissions come from
        // the block descriptors at the level above.
        parent[index] = tableFrame | DescValid | DescTable;

        // Both pages just written need cache maintenance before the MMU may use
        // them: the table that now points here, and the table itself. Without
        // it the hardware can walk to a descriptor that reads stale, which
        // presents as AT returning 0 for an address the software walk resolves
        // perfectly well.
        DeviceMapperNative.CleanCacheLine((ulong)table);
        DeviceMapperNative.CleanCacheLine((ulong)parent);

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

        // AP is deliberately left at 0b00, which is already "EL1 read/write,
        // EL0 no access" (Arm ARM DDI 0487, Table D5-30). No AP bit is set here
        // because the encodings are easy to invert: AP[2] (bit 2) selects
        // EL1 *read-only*, so setting it to make a page writable does the
        // exact opposite and every write traps with a permission fault.
        //
        // This also sidesteps the AttrIndx[0]/AP[2] overlap — both occupy bit
        // 2, and the combination AttrIndx[0]=1 with AP[2]=1 is a reserved
        // encoding. Leaving AP at 0b00 lets AttrIndx use the full three bits.
        //
        // PageFlags.Writable therefore currently cannot be distinguished from
        // the default: nothing is read-only yet. Both Device and Normal memory
        // are installed EL1-writable, which matches DeviceMapper, the other
        // writer in this kernel.

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