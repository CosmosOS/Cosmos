// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// GICv3 LPI (Locality-specific Peripheral Interrupt) configuration. LPIs
/// are the GIC's mechanism for delivering message-signalled interrupts
/// (e.g. PCI MSI/MSI-X) and are routed via the ITS. This file owns:
/// <list type="bullet">
/// <item>Allocation of the per-redistributor LPI configuration table
/// (1 byte per LPI INTID, holds priority + enable bit).</item>
/// <item>Allocation of the LPI pending table (1 bit per LPI).</item>
/// <item>Programming <c>GICR_PROPBASER</c> / <c>GICR_PENDBASER</c> on the
/// boot CPU's redistributor and setting <c>GICR_CTLR.Enable_LPIs</c>.</item>
/// <item>Per-LPI enable via the config table + redistributor invalidate.</item>
/// </list>
///
/// LPI INTIDs always start at 8192. We use IDbits = 13 (GIC supports up to
/// 16K LPI INTIDs, range 8192..16383) — far more than the 1024 the
/// kernel's <see cref="CPU.InterruptManager"/> currently dispatches, but
/// the surplus is "just" zeroed table bytes and costs ~12 KiB total.
/// </summary>
public static unsafe class GICv3Lpi
{
    // GICR RD_base register offsets that matter for LPI setup. Note the
    // exact widths: CTLR and SYNCR are 32-bit, PROPBASER/PENDBASER/INVLPIR
    // are 64-bit. A 64-bit access to a 32-bit register gets silently
    // rejected (or split-and-misrouted) on QEMU, leaving the bit unset.
    private const uint GICR_PROPBASER = 0x0070; // 64-bit
    private const uint GICR_PENDBASER = 0x0078; // 64-bit
    private const uint GICR_INVLPIR = 0x00A0;   // 64-bit WO
    private const uint GICR_SYNCR = 0x00C0;     // 32-bit RO

    private const uint GICR_CTLR = 0x0000;      // 32-bit — Redistributor Control
    private const uint GICR_CTLR_ENABLE_LPIS = 1U << 0;
    private const uint GICR_CTLR_CES = 1U << 1;  // Clear Enable Supported
    private const uint GICR_CTLR_RWP = 1U << 3;

    // PROPBASER bit layout (ARM IHI 0069G §11.10.6):
    //   [4:0]   IDbits  (n means LPI INTIDs span 0..2^(n+1)-1)
    //   [9:7]   InnerCache (5 = Normal Inner WB RaWa)
    //   [11:10] Shareability (1 = Inner Shareable)
    //   [51:12] Physical address (4 KiB aligned)
    private const ulong PROPBASER_IDBITS_MASK = 0x1FUL;
    private const ulong PROPBASER_INNER_WB = 5UL << 7;
    private const ulong PROPBASER_INNER_SHAREABLE = 1UL << 10;

    // PENDBASER bit layout (§11.10.5):
    //   [9:7]   InnerCache
    //   [11:10] Shareability
    //   [51:16] Physical address (64 KiB aligned!)
    //   [62]    PTZ (Pending table is zeroed — set so the GIC skips re-init)
    private const ulong PENDBASER_ADDR_MASK = 0x000FFFFFFFFF0000UL;
    private const ulong PENDBASER_INNER_WB = 5UL << 7;
    private const ulong PENDBASER_INNER_SHAREABLE = 1UL << 10;
    private const ulong PENDBASER_PTZ = 1UL << 62;

    // Pending table must sit on a 64 KiB physical boundary — see PENDBASER [51:16].
    private const ulong PendTableAlignment = 64UL * 1024UL;
    private const ulong PendTableAlignmentMask = PendTableAlignment - 1;

    // Per-LPI configuration byte: bit 0 = Enable, bits [7:2] = priority
    // (the priority written is the shared GicArch.DefaultPriority policy).
    private const byte LPI_CFG_ENABLE = 0x01;

    private const int LpiIdBits = 13;            // 16K LPI INTID space
    private const ulong LpiPropTableSize = 1UL << (LpiIdBits + 1); // 16 KiB
    // With IDbits=13 the GIC consults INTIDs 8192..2^(13+1)-1 = 16383 only:
    // that is 8192 config bytes, half the (over-)allocated prop table. The
    // window checks below must use this bound — bounding against the full
    // table accepts INTIDs up to 24575 whose bytes the GIC never reads.
    private const uint LpiCount = (1U << (LpiIdBits + 1)) - GIC.LPI_START;

    /// <summary>Property table allocation in 4 KiB pages: 4 pages = 16 KiB, one config byte per INTID for IDbits=13.</summary>
    private const uint PropTablePages = 4;

    /// <summary>Pending table over-allocation in 4 KiB pages: 32 pages = 128 KiB guarantees one 64 KiB-aligned 64 KiB chunk inside the run.</summary>
    private const uint PendTableOverAllocPages = 32;

    /// <summary>Maximum poll iterations while waiting for GICR_CTLR.RWP to clear or GICR_SYNCR to read 0.</summary>
    private const int RegisterSyncSpinLimit = 1_000_000;

    private static ulong s_propTablePhys;
    private static ulong s_propTableVirt;
    private static ulong s_pendTablePhys;
    private static ulong s_pendTableVirt;
    private static ulong s_rdBase;
    private static bool s_initialized;

    public static bool IsInitialized => s_initialized;

    /// <summary>
    /// Initialize LPI delivery on the boot CPU's redistributor. <paramref name="rdBase"/>
    /// is the per-CPU RD_base frame address (the same value GICv3 already
    /// uses for the active redistributor — see <c>GICv3.cs</c> field
    /// <c>_currentCpuRdBase</c>). It is only ever dereferenced, so it must
    /// be a virtual (HHDM) address; the physical addresses programmed into
    /// PROPBASER/PENDBASER come from the tables this type allocates itself.
    /// </summary>
    public static void Initialize(ulong rdBase)
    {
        if (s_initialized)
        {
            return;
        }

        s_rdBase = rdBase;

        // If firmware ran with LPIs enabled, GICR_PROPBASER/PENDBASER still
        // point at tables we don't own: silently keeping them while every
        // later EnableLpi writes OUR (never-installed) table means INV
        // reloads "disabled" and no MSI ever fires — with the binder still
        // registered. EnableLPIs may only be cleared when GICR_CTLR.CES
        // advertises 1→0 write support; without CES, fail loudly so
        // InitializeMsi downgrades the whole MSI path to polled.
        // GICR_CTLR is a 32-bit register; using Write/Read64 here would
        // misalign the access and silently no-op on QEMU.
        uint ctlr = Native.MMIO.Read32(s_rdBase + GICR_CTLR);
        if ((ctlr & GICR_CTLR_ENABLE_LPIS) != 0)
        {
            if ((ctlr & GICR_CTLR_CES) == 0)
            {
                Serial.WriteString("[GICv3-LPI] ERROR: firmware left LPIs enabled and CES=0 — cannot reprogram, MSI path disabled\n");
                return;
            }

            Serial.WriteString("[GICv3-LPI] firmware left LPIs enabled; CES=1, disabling to reprogram\n");
            Native.MMIO.Write32(s_rdBase + GICR_CTLR, ctlr & ~GICR_CTLR_ENABLE_LPIS);
            for (int i = 0; i < RegisterSyncSpinLimit; i++)
            {
                if ((Native.MMIO.Read32(s_rdBase + GICR_CTLR) & GICR_CTLR_RWP) == 0)
                {
                    break;
                }
            }

            ctlr = Native.MMIO.Read32(s_rdBase + GICR_CTLR);
        }

        // Property table — 1 byte per INTID, 4KB-aligned. Allocate 4 pages
        // (16 KiB == 2^14) → covers IDbits=13.
        s_propTableVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, PropTablePages, zero: true);
        if (s_propTableVirt == 0)
        {
            Serial.WriteString("[GICv3-LPI] ERROR: prop table alloc failed\n");
            return;
        }
        s_propTablePhys = PageAllocator.VirtualToPhysical(s_propTableVirt);

        // Pending table — bits, 64 KiB-aligned. Over-allocate so we can
        // pick a 64KB-aligned starting page from the contiguous run.
        // 32 pages = 128 KiB guarantees one 64KB-aligned 64KB chunk inside.
        ulong pendBlockVirt = (ulong)PageAllocator.AllocPages(PageType.Unmanaged, PendTableOverAllocPages, zero: true);
        if (pendBlockVirt == 0)
        {
            Serial.WriteString("[GICv3-LPI] ERROR: pend table alloc failed\n");
            return;
        }
        ulong pendBlockPhys = PageAllocator.VirtualToPhysical(pendBlockVirt);
        // Round up to next 64KB-aligned phys; preserve same offset on virt.
        ulong pendAlignedPhys = (pendBlockPhys + PendTableAlignmentMask) & ~PendTableAlignmentMask;
        ulong delta = pendAlignedPhys - pendBlockPhys;
        s_pendTablePhys = pendAlignedPhys;
        s_pendTableVirt = pendBlockVirt + delta;

        ulong propbaser = ((ulong)LpiIdBits & PROPBASER_IDBITS_MASK)
                        | PROPBASER_INNER_WB
                        | PROPBASER_INNER_SHAREABLE
                        | (s_propTablePhys & GICv3.BASER_PHYS_ADDR_MASK);
        Native.MMIO.Write64(s_rdBase + GICR_PROPBASER, propbaser);

        ulong pendbaser = PENDBASER_INNER_WB
                        | PENDBASER_INNER_SHAREABLE
                        | PENDBASER_PTZ
                        | (s_pendTablePhys & PENDBASER_ADDR_MASK);
        Native.MMIO.Write64(s_rdBase + GICR_PENDBASER, pendbaser);

        // Enable LPIs (32-bit write).
        ctlr |= GICR_CTLR_ENABLE_LPIS;
        Native.MMIO.Write32(s_rdBase + GICR_CTLR, ctlr);

        // Wait for RWP to clear.
        for (int i = 0; i < RegisterSyncSpinLimit; i++)
        {
            if ((Native.MMIO.Read32(s_rdBase + GICR_CTLR) & GICR_CTLR_RWP) == 0)
            {
                break;
            }
        }

        Serial.WriteString("[GICv3-LPI] enabled, INTID range 8192..16383\n");

        s_initialized = true;
    }

    /// <summary>
    /// Enable LPI <paramref name="lpi"/> in the configuration table at
    /// default priority. Must be called after the ITS has wired the
    /// (DeviceID, EventID) → LPI mapping. <paramref name="lpi"/> is the
    /// absolute INTID (>= 8192).
    /// </summary>
    public static void EnableLpi(uint lpi)
    {
        if (!s_initialized || lpi < GIC.LPI_START)
        {
            return;
        }
        uint off = lpi - GIC.LPI_START;
        if (off >= LpiCount)
        {
            return;
        }
        byte* cfg = (byte*)s_propTableVirt;
        cfg[off] = (byte)(GicArch.DefaultPriority | LPI_CFG_ENABLE);

        // The redistributor reads the config table over the bus; the byte
        // write must reach memory BEFORE we tell it to refetch. INVLPIR is
        // a no-op on this redistributor (DirectLPI=0) but harmless; the
        // ITS INV command emitted from MapEvent is what actually refreshes
        // the cached LPI configuration.
        DeviceMapperNative.DsbIsb();
        Native.MMIO.Write64(s_rdBase + GICR_INVLPIR, lpi);
        for (int i = 0; i < RegisterSyncSpinLimit; i++)
        {
            if (Native.MMIO.Read32(s_rdBase + GICR_SYNCR) == 0)
            {
                break;
            }
        }
    }

    /// <summary>Disable an LPI in the config table.</summary>
    public static void DisableLpi(uint lpi)
    {
        if (!s_initialized || lpi < GIC.LPI_START)
        {
            return;
        }
        uint off = lpi - GIC.LPI_START;
        if (off >= LpiCount)
        {
            return;
        }
        byte* cfg = (byte*)s_propTableVirt;
        cfg[off] = (byte)(GicArch.DefaultPriority & ~LPI_CFG_ENABLE);
        DeviceMapperNative.DsbIsb();
        Native.MMIO.Write64(s_rdBase + GICR_INVLPIR, lpi);
        for (int i = 0; i < RegisterSyncSpinLimit; i++)
        {
            if (Native.MMIO.Read32(s_rdBase + GICR_SYNCR) == 0)
            {
                break;
            }
        }
    }
}
