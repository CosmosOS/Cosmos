// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// GICv3 Interrupt Translation Service (ITS) driver. The ITS is the
/// component that turns a PCI MSI write — a 32-bit EventID dropped into
/// <c>GITS_TRANSLATER</c> — into an LPI delivered to a target CPU's
/// redistributor. To do that it consults two in-memory tables it owns
/// (Device table and Collection table) plus a per-device Interrupt
/// Translation Table (ITT) whose layout the spec leaves opaque.
///
/// <para>This driver:</para>
/// <list type="bullet">
/// <item>Allocates the device and collection tables from <c>GITS_BASER</c>
/// configuration.</item>
/// <item>Allocates a 4 KiB command-queue ring and points <c>GITS_CBASER</c>
/// at it.</item>
/// <item>Issues one <c>MAPC</c> at init time mapping collection 0 to the
/// boot CPU's redistributor.</item>
/// <item>Exposes <see cref="MapDevice"/> (issues <c>MAPD</c>) and
/// <see cref="MapEvent"/> (issues <c>MAPTI</c>) for the MSI-X binder, and
/// their inverses <see cref="DiscardEvent"/> (<c>DISCARD</c>) and
/// <see cref="UnmapDevice"/> (<c>MAPD</c> V=0) for its teardown.</item>
/// </list>
///
/// Single-CPU only: every (DeviceID, EventID) is mapped to collection 0.
/// </summary>
public static unsafe class GICv3Its
{
    // Register offsets (frame 0 = control frame).
    private const uint GITS_CTLR = 0x0000;
    private const uint GITS_TYPER = 0x0008;
    private const uint GITS_CBASER = 0x0080;
    private const uint GITS_CWRITER = 0x0088;
    private const uint GITS_CREADR = 0x0090;
    private const uint GITS_BASER0 = 0x0100;

    // GITS_BASER table: 8 entries at stride 8 bytes.
    private const int BaserCount = 8;
    private const ulong BaserStride = 8;

    // Translation register frame is at +0x10000 from ITS base.
    private const uint GITS_TRANSLATER_OFF = 0x10040;

    private const uint GITS_CTLR_ENABLED = 1U << 0;
    private const uint GITS_CTLR_QUIESCENT = 1U << 31;

    /// <summary>Spin iterations (one GITS_CTLR MMIO read each) to wait for GITS_CTLR.Quiescent after disabling the ITS; not time-calibrated (runs before any timer facility is usable).</summary>
    private const int QuiescentSpinLimit = 1_000_000;

    // Hang-breaker for the CREADR poll in FlushCommandQueue, in loop
    // iterations (one MMIO read each), not time: a healthy ITS consumes a
    // handful of queued commands in microseconds, so ~10M reads
    // (seconds-order even on slow buses) is orders of magnitude past any
    // legitimate completion and only trips when the ITS is wedged.
    // Deliberately not time-calibrated: this runs during early interrupt
    // bring-up, before any timer facility is usable from Core.ARM64
    // (PlatformHAL.DelayMicroseconds lives a layer above and can't be
    // referenced from here).
    private const int CommandFlushSpinLimit = 10_000_000;

    // GITS_TYPER bit layout (§11.10.13).
    private const int TYPER_ITT_ENTRY_SIZE_SHIFT = 4;
    private const ulong TYPER_ITT_ENTRY_SIZE_MASK = 0xFUL;   // bits [7:4]
    private const int TYPER_PTA_SHIFT = 19;
    private const ulong TYPER_PTA_MASK = 0x1UL;

    // GICR_TYPER Processor_Number field, bits [23:8].
    private const int GICR_TYPER_PROCNUM_SHIFT = 8;
    private const ulong GICR_TYPER_PROCNUM_MASK = 0xFFFFUL;

    // GITS_BASER<n> bit layout (§11.10.2).
    //   [63]    Valid       [62] Indirect
    //   [58:56] Type        [55:53] InnerCache
    //   [52:48] Entry_Size
    //   [47:12] Physical_Address (4 KiB page size)
    //   [11:10] Shareability
    //   [9:8]   Page_Size   [7:0] Size (pages - 1)
    private const ulong BASER_VALID = 1UL << 63;
    private const ulong BASER_INDIRECT = 1UL << 62;
    private const int BASER_TYPE_SHIFT = 56;
    private const ulong BASER_TYPE_MASK = 0x7UL;
    private const int BASER_ENTRY_SIZE_SHIFT = 48;
    private const ulong BASER_ENTRY_SIZE_MASK = 0x1FUL;
    private const ulong BASER_ADDR_MASK = 0x0000FFFFFFFFF000UL;
    private const int BASER_SHAREABILITY_SHIFT = 10;
    private const ulong BASER_SHAREABILITY_MASK = 0x3UL;
    private const ulong BASER_PAGES_MASK = 0xFFUL;
    private const ulong BASER_INNERSHAREABLE = 1UL << BASER_SHAREABILITY_SHIFT;
    // [61:59] InnerCache: 5 = Normal Inner Read-Allocate Write-Allocate Write-Back.
    // Anything else (e.g. value 1 = Non-cacheable) makes the ITS bypass CPU
    // caches when reading these tables, which silently desyncs from our
    // (cacheable) writes — MAPD/MAPTI appear to complete but the device
    // entry the ITS actually fetches is whatever's still in DRAM.
    // GITS_CBASER shares the identical InnerCache bit slice, so this value
    // is also used when programming the command queue.
    private const ulong BASER_INNER_CACHE_RaWaWb = 5UL << 59;
    private const int BASER_PAGE_SIZE_SHIFT = 8;
    private const ulong BASER_PAGE_SIZE_MASK = 0x3UL;
    private const ulong BASER_PAGE_SIZE_4K = 0UL << BASER_PAGE_SIZE_SHIFT;
    /// <summary>BASER Page_Size field value (unshifted) selecting a 16 KiB granule; 2 selects 64 KiB (§11.10.2).</summary>
    private const ulong BASER_PAGE_SIZE_FIELD_16K = 1UL;
    /// <summary>Bytes per BASER table page when the ITS forces a 16 KiB granule.</summary>
    private const ulong Granule16KBytes = 16UL * 1024;
    /// <summary>Bytes per BASER table page when the ITS forces a 64 KiB granule.</summary>
    private const ulong Granule64KBytes = 64UL * 1024;

    // BASER Type field values (§11.10.2 Table 11-22).
    private const byte BASER_TYPE_DEVICE = 1;
    private const byte BASER_TYPE_COLLECTION = 4;

    // GITS_CBASER bit layout — same address encoding as PROPBASER (bits [51:12]).
    private const ulong CBASER_VALID = 1UL << 63;
    private const ulong CBASER_INNERSHAREABLE = 1UL << 10;

    // ITS command layout: every command is 32 bytes (4 × u64) in the queue.
    private const uint ItsCommandSize = 32;
    // Command queue lives in one 4 KiB page → 128 commands of 32 bytes each.
    private const uint CommandQueueBytes = CommandQueuePages * ItsPageSize;
    /// <summary>Number of 4 KiB pages backing the command-queue ring (CommandQueueBytes worth).</summary>
    private const uint CommandQueuePages = 1;

    // Page-sized allocations for ITT/table buffers.
    private const uint ItsPageSize = 4096;
    private const uint ItsPageMask = ItsPageSize - 1;

    // Command opcodes (low byte of cmd[0]).
    private const byte CMD_MAPD = 0x08;
    private const byte CMD_MAPC = 0x09;
    private const byte CMD_MAPTI = 0x0A;
    private const byte CMD_INV = 0x0C;
    private const byte CMD_SYNC = 0x05;
    private const byte CMD_DISCARD = 0x0F;

    // Valid bit shared by MAPC / MAPD encodings, cmd[2] bit 63.
    private const ulong CMD_VALID = 1UL << 63;
    // DeviceID lives in the upper 32 bits of cmd[0] for MAPD/MAPTI/INV/DISCARD.
    private const int CMD_DEVICE_ID_SHIFT = 32;
    // MAPTI encodes the LPI INTID in the upper 32 bits of cmd[1].
    private const int MAPTI_LPI_SHIFT = 32;
    // MAPC / SYNC target_addr field in cmd[2], bits [47:16] (PTA mode) or
    // proc_num<<16 in non-PTA mode — same bit slice either way.
    private const ulong CMD_TARGET_ADDR_MASK = 0x0000FFFFFFFF0000UL;
    private const int CMD_TARGET_PROCNUM_SHIFT = 16;
    // MAPD size field in cmd[1], bits [4:0]: log2(num_event_ids) - 1.
    private const ulong MAPD_SIZE_MASK = 0x1FUL;
    // MAPD ITT_address in cmd[2], bits [51:8] (256-byte aligned).
    private const ulong MAPD_ITT_ADDR_MASK = 0x000FFFFFFFFFFF00UL;

    // Number of 4 KiB pages backing each shared ITS table. NOTE: a flat
    // device table is indexed by DeviceID VALUE, not device count — 32 KiB
    // at the typical 8-byte entry covers DeviceIDs 0..4095 only (BDF up to
    // bus 15). MapDevice enforces the real bound (computed from the entry
    // size the hardware reports) and rejects wider IDs loudly; supporting
    // sparse/wide DeviceID spaces properly means BASER.Indirect two-level
    // tables.
    private const uint DeviceTablePages = 8;
    private const uint CollectionTablePages = 1;   // 4 KiB — far more collections than we need

    // GITS_CREADR: bits[4:0] are status flags (Stalled in bit 0); the queue
    // offset occupies the rest of the register.
    private const ulong CREADR_OFFSET_MASK = ~0x1FUL;
    private const ulong CREADR_STALLED = 0x1UL;

    // Per-collection state.
    private const ushort BootCollectionId = 0;

    // ITT buffer sizing.
    private const uint IttMinEvents = 2;

    private static ulong s_itsBase;          // HHDM virtual base — every GITS_* MMIO access goes through this
    private static ulong s_translaterPhys;   // GITS_TRANSLATER physical address; written by devices via MSI
    private static ulong s_cmdQueueVirt;
    private static ulong s_cmdQueuePhys;
    private static uint s_cmdQueueSize;      // bytes
    private static ulong s_cmdWriteOff;      // mirror of GITS_CWRITER
    private static ulong s_maxDeviceId;      // highest DeviceID the flat device table can index
    private static int s_ittEntrySize;
    private static bool s_physicalTargetAddress;
    private static ulong s_bootRedistTarget; // target value for collection 0 (phys addr or proc number<<16)
    private static SchedSpinLock s_cmdLock;
    private static bool s_initialized;

    public static bool IsInitialized => s_initialized;

    /// <summary>
    /// The physical address devices write their <c>data</c> dword to when
    /// signalling an MSI. Becomes the MSI-X table entry's address field.
    /// </summary>
    public static ulong TranslaterPhysAddr => s_translaterPhys;

    /// <summary>
    /// Initialize the ITS. Must be called after
    /// <see cref="GICv3Lpi.Initialize"/> on the boot CPU's redistributor.
    /// Virt/phys are passed separately because both roles are needed: all
    /// GITS_* MMIO goes through the Device-memory HHDM mapping (the TTBR0
    /// identity map is Normal WB cacheable — only QEMU TCG's disregard for
    /// memory attributes made dereferencing raw physical appear to work),
    /// while GITS_TRANSLATER (handed to devices as the MSI doorbell) and
    /// the MAPC RDbase target are bus addresses and must stay physical.
    /// </summary>
    /// <param name="itsVirtBase">ITS register block, HHDM virtual (dereferenced).</param>
    /// <param name="itsPhysBase">ITS register block, physical (doorbell address source).</param>
    /// <param name="rdVirtBase">Boot CPU redistributor RD_base, HHDM virtual (dereferenced for GICR_TYPER).</param>
    /// <param name="rdPhysBase">Boot CPU redistributor RD_base, physical (MAPC RDbase when GITS_TYPER.PTA=1).</param>
    public static void Initialize(ulong itsVirtBase, ulong itsPhysBase, ulong rdVirtBase, ulong rdPhysBase)
    {
        if (s_initialized)
        {
            return;
        }

        s_itsBase = itsVirtBase;
        s_translaterPhys = itsPhysBase + GITS_TRANSLATER_OFF;

        // Make sure the ITS is disabled while we configure tables.
        uint ctlr = Native.MMIO.Read32(s_itsBase + GITS_CTLR);
        if ((ctlr & GITS_CTLR_ENABLED) != 0)
        {
            Native.MMIO.Write32(s_itsBase + GITS_CTLR, ctlr & ~GITS_CTLR_ENABLED);
            bool quiesced = false;
            for (int i = 0; i < QuiescentSpinLimit; i++)
            {
                if ((Native.MMIO.Read32(s_itsBase + GITS_CTLR) & GITS_CTLR_QUIESCENT) != 0)
                {
                    quiesced = true;
                    break;
                }
            }
            if (!quiesced)
            {
                // Not worth killing the boot over: leave the ITS
                // uninitialized so InitializeMsi downgrades to polled.
                Serial.WriteString("[GICv3-ITS] ERROR: timed out waiting for QUIESCENT after disable\n");
                return;
            }
        }

        ulong typer = Native.MMIO.Read64(s_itsBase + GITS_TYPER);
        s_ittEntrySize = (int)(((typer >> TYPER_ITT_ENTRY_SIZE_SHIFT) & TYPER_ITT_ENTRY_SIZE_MASK) + 1);
        s_physicalTargetAddress = ((typer >> TYPER_PTA_SHIFT) & TYPER_PTA_MASK) != 0;

        // Compute target address for MAPC / SYNC. With PTA=1 the RDbase
        // field carries the redistributor's PHYSICAL address (it is a bus
        // address the ITS emits, not something the CPU dereferences).
        if (s_physicalTargetAddress)
        {
            s_bootRedistTarget = rdPhysBase;
        }
        else
        {
            // Processor_Number is GICR_TYPER bits [23:8]. We're sole CPU so 0
            // is almost always right, but read it for correctness.
            ulong rdTyper = Native.MMIO.Read64(rdVirtBase + GICv3.GICR_TYPER);
            ulong procNum = (rdTyper >> GICR_TYPER_PROCNUM_SHIFT) & GICR_TYPER_PROCNUM_MASK;
            s_bootRedistTarget = procNum << CMD_TARGET_PROCNUM_SHIFT;
        }

        // Configure GITS_BASER[i] for Device and Collection tables.
        for (int i = 0; i < BaserCount; i++)
        {
            ulong off = GITS_BASER0 + (ulong)i * BaserStride;
            ulong baser = Native.MMIO.Read64(s_itsBase + (uint)off);
            byte type = (byte)((baser >> BASER_TYPE_SHIFT) & BASER_TYPE_MASK);
            if (type == BASER_TYPE_DEVICE || type == BASER_TYPE_COLLECTION)
            {
                uint pages = type == BASER_TYPE_DEVICE ? DeviceTablePages : CollectionTablePages;
                if (!ConfigureBaser(off, baser, type, pages))
                {
                    // Leave s_initialized false: InitializeMsi reports the
                    // failure and the MSI path downgrades to polled.
                    return;
                }
            }
            // Other types (vCPU, reserved) left untouched.
        }

        // Allocate command queue (one 4 KiB page, 128 commands of 32 bytes each).
        s_cmdQueueSize = CommandQueueBytes;
        s_cmdQueueVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, CommandQueuePages, zero: true);
        if (s_cmdQueueVirt == 0)
        {
            // Without this check CBASER would point the ITS at physical
            // page 0 — corruption instead of the clean MSI-off downgrade.
            Serial.WriteString("[GICv3-ITS] ERROR: command queue alloc failed\n");
            return;
        }

        s_cmdQueuePhys = PageAllocator.VirtualToPhysical(s_cmdQueueVirt);

        ulong cbaser = CBASER_VALID
                     | CBASER_INNERSHAREABLE
                     | BASER_INNER_CACHE_RaWaWb
                     | (s_cmdQueuePhys & GICv3.BASER_PHYS_ADDR_MASK)
                     | 0; // size = (4KB / 4KB) - 1 = 0
        Native.MMIO.Write64(s_itsBase + GITS_CBASER, cbaser);
        // Same policy as the BASER tables: a command queue whose
        // shareability reads back 0 is fetched without snooping CPU
        // caches, and command writes would sit in cache while the ITS
        // executes stale DRAM. Refuse rather than corrupt.
        ulong cbRb = Native.MMIO.Read64(s_itsBase + GITS_CBASER);
        if ((cbRb & (BASER_SHAREABILITY_MASK << BASER_SHAREABILITY_SHIFT)) == 0)
        {
            Serial.WriteString("[GICv3-ITS] ERROR: CBASER shareability forced to 0 (non-coherent ITS), no dcache maintenance available\n");
            return;
        }

        Native.MMIO.Write64(s_itsBase + GITS_CWRITER, 0);
        s_cmdWriteOff = 0;

        // Enable ITS.
        Native.MMIO.Write32(s_itsBase + GITS_CTLR, GITS_CTLR_ENABLED);

        // Map collection 0 -> boot CPU's redistributor.
        s_initialized = true;
        EnqueueMapc(BootCollectionId, s_bootRedistTarget, valid: true);
        EnqueueSync();
        FlushCommandQueue();

        Serial.WriteString("[GICv3-ITS] enabled at phys 0x");
        Serial.WriteHex(itsPhysBase);
        Serial.WriteString(" (virt 0x");
        Serial.WriteHex(itsVirtBase);
        Serial.WriteString(")\n");
    }

    /// <summary>
    /// Allocate an ITT for <paramref name="deviceId"/> and issue MAPD.
    /// <paramref name="maxEvents"/> is rounded up to the next power of two
    /// (minimum 2, the ITS hardware lower bound). Returns the ITT's virtual
    /// address, which the caller hands back to <see cref="UnmapDevice"/>
    /// (0 when the ITS is not initialized and nothing was mapped). The
    /// DeviceID must not be mapped already: unmap it first. Thread context
    /// only: the command queue lock is a plain spinlock.
    /// </summary>
    public static ulong MapDevice(uint deviceId, uint maxEvents)
    {
        if (!s_initialized)
        {
            return 0;
        }

        // The flat device table is indexed by DeviceID value: a MAPD past
        // its end is either rejected (command error -> STALLED) or walks
        // memory beyond the table, and the device's MSIs silently vanish.
        // Reject loudly instead; MsiX.Enable turns this into a clean
        // polled-mode downgrade for the device.
        if (deviceId > s_maxDeviceId)
        {
            Serial.WriteString("[GICv3-ITS] ERROR: DeviceID 0x");
            Serial.WriteHex(deviceId);
            Serial.WriteString(" exceeds flat device table max 0x");
            Serial.WriteHex(s_maxDeviceId);
            Serial.WriteString(" (grow DeviceTablePages or add BASER.Indirect support)\n");
            throw new System.InvalidOperationException("GICv3-ITS: DeviceID exceeds the flat device table range");
        }

        uint nrEvents = maxEvents < IttMinEvents ? IttMinEvents : RoundUpPow2(maxEvents);
        int sizeField = Log2(nrEvents) - 1; // ARM IHI 0069G: size = log2(nr_ites) - 1
        if (sizeField < 0)
        {
            sizeField = 0;
        }

        ulong ittBytes = (ulong)nrEvents * (ulong)s_ittEntrySize;
        // ITT must be 256-byte aligned. PageAllocator returns 4 KiB pages
        // which already satisfies that; allocate at least 1 page.
        ulong pages = (ittBytes + ItsPageMask) / ItsPageSize;
        if (pages == 0)
        {
            pages = 1;
        }
        ulong ittVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, (uint)pages, zero: true);
        if (ittVirt == 0)
        {
            // Same channel as the DeviceID bound: MsiX.Enable catches this
            // and the device downgrades to polled.
            Serial.WriteString("[GICv3-ITS] ERROR: ITT alloc failed\n");
            throw new System.InvalidOperationException("GICv3-ITS: ITT allocation failed");
        }

        ulong ittPhys = PageAllocator.VirtualToPhysical(ittVirt);

        EnqueueMapd(deviceId, ittPhys, sizeField, valid: true);
        EnqueueSync();
        FlushCommandQueue();
        return ittVirt;
    }

    /// <summary>
    /// Issue MAPD with V=0 for <paramref name="deviceId"/>, wait for the ITS
    /// to consume it, then free the ITT <see cref="MapDevice"/> returned.
    /// The ITS reads the ITT until the unmap completes, so the page goes
    /// back to the allocator only after the flush. DISCARD every mapped
    /// event first: the unmap drops the translations but not the LPIs they
    /// pointed at. Thread context only: the command queue lock is a plain
    /// spinlock.
    /// </summary>
    internal static void UnmapDevice(uint deviceId, ulong ittVirt)
    {
        if (!s_initialized || ittVirt == 0)
        {
            return;
        }

        // Size and ITT_addr are ignored when V=0 (IHI 0069, MAPD); the real
        // address is passed anyway so the command reads like the MAPD it undoes.
        EnqueueMapd(deviceId, PageAllocator.VirtualToPhysical(ittVirt), 0, valid: false);
        EnqueueSync();
        FlushCommandQueue();
        PageAllocator.Free((void*)ittVirt);
    }

    /// <summary>
    /// Wire (deviceId, eventId) → LPI INTID <paramref name="lpi"/> on the
    /// boot CPU's collection. The caller is expected to have allocated the
    /// LPI via <c>ARM64InterruptController.AllocateLpi</c> and enabled it via
    /// <c>GICv3Lpi.EnableLpi</c>.
    /// </summary>
    public static void MapEvent(uint deviceId, uint eventId, uint lpi)
    {
        if (!s_initialized)
        {
            return;
        }
        EnqueueMapti(deviceId, eventId, lpi, BootCollectionId);
        // INV invalidates the ITS's (and target redistributor's) cached
        // LPI configuration for (deviceId, eventId). Required: when
        // GICR_TYPER.DirectLPI == 0, GICR_INVLPIR is RES0, so writing the
        // prop-table byte alone never propagates — the only path that
        // refreshes the cache is an ITS INV / INVALL command.
        EnqueueInv(deviceId, eventId);
        EnqueueSync();
        FlushCommandQueue();
    }

    /// <summary>
    /// Undo <see cref="MapEvent"/>: DISCARD removes the (deviceId, eventId)
    /// translation and the pending state of the LPI it pointed at, and the
    /// SYNC that follows waits for the redistributor to apply that. Once
    /// this returns the LPI can be freed and handed to another device: a
    /// late write of this EventID no longer translates, and nothing left
    /// pending can fire under the next owner's handler. Thread context
    /// only: the command queue lock is a plain spinlock.
    /// </summary>
    internal static void DiscardEvent(uint deviceId, uint eventId)
    {
        if (!s_initialized)
        {
            return;
        }
        EnqueueDiscard(deviceId, eventId);
        EnqueueSync();
        FlushCommandQueue();
    }

    // ── BASER configuration ───────────────────────────────────────────

    private static bool ConfigureBaser(ulong off, ulong oldBaser, byte type, uint pages)
    {
        int entrySize = (int)(((oldBaser >> BASER_ENTRY_SIZE_SHIFT) & BASER_ENTRY_SIZE_MASK) + 1);

        ulong tableVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, pages, zero: true);
        if (tableVirt == 0)
        {
            Serial.WriteString("[GICv3-ITS] ERROR: BASER table alloc failed\n");
            return false;
        }

        ulong tablePhys = PageAllocator.VirtualToPhysical(tableVirt);
        ulong sizeBytes = (ulong)pages * ItsPageSize;

        ulong baser = ((ulong)type << BASER_TYPE_SHIFT)
                    | ((ulong)(entrySize - 1) << BASER_ENTRY_SIZE_SHIFT)
                    | (tablePhys & BASER_ADDR_MASK)
                    | BASER_INNER_CACHE_RaWaWb
                    | BASER_INNERSHAREABLE
                    | BASER_PAGE_SIZE_4K
                    | (ulong)((pages - 1) & BASER_PAGES_MASK)
                    | BASER_VALID;
        Native.MMIO.Write64(s_itsBase + (uint)off, baser);
        ulong rb = Native.MMIO.Read64(s_itsBase + (uint)off);

        // Page_Size is not guaranteed writable: a 16K/64K-only ITS holds a
        // larger granule in the read-back, and Size counts pages of THAT
        // granule — keeping Size = pages-1 would multiply the window
        // (8 x 64 KiB = 512 KiB over a 32 KiB allocation) and let the ITS
        // read and write far past the table. Re-allocate one properly
        // aligned granule and re-program with Size = 0 instead.
        ulong rbGranule = (rb >> BASER_PAGE_SIZE_SHIFT) & BASER_PAGE_SIZE_MASK;
        if (rbGranule != 0)
        {
            ulong granuleBytes = rbGranule == BASER_PAGE_SIZE_FIELD_16K ? Granule16KBytes : Granule64KBytes;
            // Over-allocate 2x so a granule-aligned block exists inside,
            // same trick as the LPI pending table's 64 KiB alignment.
            uint blockPages = (uint)(granuleBytes * 2 / ItsPageSize);
            ulong blockVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, blockPages, zero: true);
            if (blockVirt == 0)
            {
                Serial.WriteString("[GICv3-ITS] ERROR: BASER granule realloc failed\n");
                return false;
            }

            ulong blockPhys = PageAllocator.VirtualToPhysical(blockVirt);
            tablePhys = (blockPhys + granuleBytes - 1) & ~(granuleBytes - 1);
            sizeBytes = granuleBytes;

            Serial.WriteString("[GICv3-ITS] BASER granule forced to 0x");
            Serial.WriteHex(granuleBytes);
            Serial.WriteString(" bytes, re-allocated table\n");

            baser = ((ulong)type << BASER_TYPE_SHIFT)
                  | ((ulong)(entrySize - 1) << BASER_ENTRY_SIZE_SHIFT)
                  | (tablePhys & BASER_ADDR_MASK)
                  | BASER_INNER_CACHE_RaWaWb
                  | BASER_INNERSHAREABLE
                  | (rbGranule << BASER_PAGE_SIZE_SHIFT)
                  | BASER_VALID; // Size = 0: one granule
            Native.MMIO.Write64(s_itsBase + (uint)off, baser);
            rb = Native.MMIO.Read64(s_itsBase + (uint)off);
        }

        // Shareability forced to 0 means the ITS will not snoop CPU caches
        // for its table walks, and this driver has no dcache-maintenance
        // plumbing — keeping cacheable attributes would desync silently
        // (MAPD/MAPTI appear to complete, the ITS fetches stale DRAM).
        // Refuse and let Initialize downgrade the MSI path instead.
        ulong shareabilityField = BASER_SHAREABILITY_MASK << BASER_SHAREABILITY_SHIFT;
        if ((rb & shareabilityField) == 0)
        {
            Serial.WriteString("[GICv3-ITS] ERROR: BASER shareability forced to 0 (non-coherent ITS), no dcache maintenance available\n");
            return false;
        }

        if (type == BASER_TYPE_DEVICE)
        {
            // A flat table indexes by DeviceID value: this is the highest
            // ID MAPD can accept without the ITS walking past the table.
            s_maxDeviceId = sizeBytes / (ulong)entrySize - 1;
        }

        return true;
    }

    // ── Command queue posting ─────────────────────────────────────────

    private static void EnqueueRaw(ulong cmd0, ulong cmd1, ulong cmd2, ulong cmd3)
    {
        // Caller holds s_cmdLock.
        ulong* q = (ulong*)(s_cmdQueueVirt + s_cmdWriteOff);
        q[0] = cmd0;
        q[1] = cmd1;
        q[2] = cmd2;
        q[3] = cmd3;

        s_cmdWriteOff += ItsCommandSize;
        if (s_cmdWriteOff >= s_cmdQueueSize)
        {
            s_cmdWriteOff = 0;
        }
    }

    private static void EnqueueMapc(ushort collection, ulong target, bool valid)
    {
        s_cmdLock.Acquire();
        // cmd[0]: opcode in low byte
        // cmd[2]: collection (15:0) | target_addr [47:16] (PTA mode) or proc_num<<16 (non-PTA)
        //         valid in bit 63
        ulong c2 = (ulong)collection
                 | (target & CMD_TARGET_ADDR_MASK)
                 | (valid ? CMD_VALID : 0);
        EnqueueRaw(CMD_MAPC, 0, c2, 0);
        s_cmdLock.Release();
    }

    private static void EnqueueMapd(uint deviceId, ulong ittPhys, int sizeField, bool valid)
    {
        s_cmdLock.Acquire();
        ulong c0 = CMD_MAPD | ((ulong)deviceId << CMD_DEVICE_ID_SHIFT);
        ulong c1 = (ulong)sizeField & MAPD_SIZE_MASK;
        ulong c2 = (ittPhys & MAPD_ITT_ADDR_MASK) | (valid ? CMD_VALID : 0);
        EnqueueRaw(c0, c1, c2, 0);
        s_cmdLock.Release();
    }

    private static void EnqueueMapti(uint deviceId, uint eventId, uint lpi, ushort collection)
    {
        s_cmdLock.Acquire();
        ulong c0 = CMD_MAPTI | ((ulong)deviceId << CMD_DEVICE_ID_SHIFT);
        ulong c1 = (ulong)eventId | ((ulong)lpi << MAPTI_LPI_SHIFT);
        ulong c2 = (ulong)collection;
        EnqueueRaw(c0, c1, c2, 0);
        s_cmdLock.Release();
    }

    private static void EnqueueInv(uint deviceId, uint eventId)
    {
        s_cmdLock.Acquire();
        ulong c0 = CMD_INV | ((ulong)deviceId << CMD_DEVICE_ID_SHIFT);
        ulong c1 = (ulong)eventId;
        EnqueueRaw(c0, c1, 0, 0);
        s_cmdLock.Release();
    }

    private static void EnqueueDiscard(uint deviceId, uint eventId)
    {
        s_cmdLock.Acquire();
        // Same layout as INV (IHI 0069, DISCARD): DeviceID in cmd[0][63:32],
        // EventID in cmd[1][31:0], cmd[2] and cmd[3] RES0.
        ulong c0 = CMD_DISCARD | ((ulong)deviceId << CMD_DEVICE_ID_SHIFT);
        ulong c1 = (ulong)eventId;
        EnqueueRaw(c0, c1, 0, 0);
        s_cmdLock.Release();
    }

    private static void EnqueueSync()
    {
        s_cmdLock.Acquire();
        ulong c2 = s_bootRedistTarget & CMD_TARGET_ADDR_MASK;
        EnqueueRaw(CMD_SYNC, 0, c2, 0);
        s_cmdLock.Release();
    }

    /// <summary>
    /// Publish the new write offset and spin until the ITS has consumed
    /// every command we posted (CWRITER == CREADR).
    /// </summary>
    private static void FlushCommandQueue()
    {
        s_cmdLock.Acquire();
        // DSB so the command bytes are globally visible BEFORE the ITS sees
        // the advanced CWRITER pointer; otherwise the ITS may fetch stale
        // queue contents and ignore our command.
        DeviceMapperNative.DsbIsb();
        Native.MMIO.Write64(s_itsBase + GITS_CWRITER, s_cmdWriteOff);

        for (int i = 0; i < CommandFlushSpinLimit; i++)
        {
            ulong reader = Native.MMIO.Read64(s_itsBase + GITS_CREADR);
            if ((reader & CREADR_OFFSET_MASK) == s_cmdWriteOff)
            {
                // Spec: GITS_CREADR.Stalled (bit 0) == 1 means the ITS
                // rejected the last command and won't advance further.
                // Treat that as fatal — the next MAPD/MAPTI would be issued
                // against an ITS in unknown state and devices would never
                // see their interrupts.
                if ((reader & CREADR_STALLED) != 0)
                {
                    s_cmdLock.Release();
                    throw new System.InvalidOperationException("[GICv3-ITS] command queue STALLED");
                }
                s_cmdLock.Release();
                return;
            }
        }
        s_cmdLock.Release();
        throw new System.InvalidOperationException("[GICv3-ITS] command queue flush timeout");
    }

    private static uint RoundUpPow2(uint v)
    {
        v--;
        v |= v >> 1;
        v |= v >> 2;
        v |= v >> 4;
        v |= v >> 8;
        v |= v >> 16;
        v++;
        return v == 0 ? 1u : v;
    }

    private static int Log2(uint v)
    {
        int r = 0;
        while ((v >>= 1) != 0)
        {
            r++;
        }
        return r;
    }
}
