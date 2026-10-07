// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// Unified GIC facade that auto-detects GICv2 or GICv3 at runtime
/// and delegates to the appropriate implementation.
/// Supports configurable base addresses for real hardware (from DTB).
/// </summary>
public static class GIC
{
    // INTID space range starts (GIC architecture, common to v2 and v3)
    /// <summary>First SGI (Software Generated Interrupt) INTID; SGIs span 0..15.</summary>
    public const uint SGI_START = 0;
    /// <summary>First PPI (Private Peripheral Interrupt) INTID; PPIs span 16..31.</summary>
    public const uint PPI_START = 16;
    /// <summary>First SPI (Shared Peripheral Interrupt) INTID; SPIs span 32..1019.</summary>
    public const uint SPI_START = 32;
    /// <summary>First LPI (Locality-specific Peripheral Interrupt) INTID; the LPI space starts at 8192 per ARM IHI 0069.</summary>
    public const uint LPI_START = 8192;

    // Architected timer PPI INTIDs (same for v2 and v3)
    /// <summary>INTID 29 = secure physical timer PPI.</summary>
    public const uint TIMER_SECURE_PHYS = 29;
    /// <summary>INTID 30 = non-secure physical timer PPI.</summary>
    public const uint TIMER_NONSEC_PHYS = 30;
    /// <summary>INTID 27 = virtual timer PPI.</summary>
    public const uint TIMER_VIRT = 27;
    /// <summary>INTID 26 = hypervisor physical timer PPI.</summary>
    public const uint TIMER_HYP = 26;

    /// <summary>Default GICD (distributor) base address on the QEMU virt machine, used when DTB/ACPI are absent (internal: shared fallback default for GICv2 and GICv3).</summary>
    internal const ulong QemuVirtGicdBase = 0x08000000;

    /// <summary>Offset of the second 64 KiB ITS register frame (GITS_TRANSLATER page) from the ITS base, per GICv3 ITS spec.</summary>
    private const ulong ItsTranslationFrameOffset = 0x10000;

    /// <summary>Minimum MADT GICC GIC version field value indicating GICv3 (ACPI MADT interrupt controller structure).</summary>
    private const byte GicVersionV3 = 3;

    /// <summary>Size in bytes of one 2 MiB block mapped by DeviceMapper (AArch64 level-2 block granule).</summary>
    private const ulong DeviceBlockSizeBytes = 0x200000;

    /// <summary>Mask of the offset bits within a 2 MiB device block, used to align addresses down to a block boundary.</summary>
    private const ulong DeviceBlockOffsetMask = 0x1FFFFFUL;

    private static bool s_isV3;
    private static bool s_initialized;
    private static ulong s_distBase;

    /// <summary>
    /// Translates a physical address to a virtual address using Limine's HHDM offset.
    /// MMIO and ACPI physical addresses must go through this to be dereferenceable.
    /// If the address is already in the higher half, it's returned as-is.
    /// </summary>
    private static unsafe ulong PhysToVirt(ulong phys)
    {
        if (phys == 0)
        {
            return 0;
        }

        ulong hhdmOffset = 0;
        if (Limine.HHDM.Response != null)
        {
            hhdmOffset = Limine.HHDM.Response->Offset;
        }

        // Already virtual (above HHDM)?
        if (hhdmOffset != 0 && phys >= hhdmOffset)
        {
            return phys;
        }

        return phys + hhdmOffset;
    }

    /// <summary>
    /// Whether the GIC has been initialized.
    /// </summary>
    public static bool IsInitialized => s_initialized;

    /// <summary>
    /// Whether GICv3 is being used (false = GICv2).
    /// </summary>
    public static bool IsVersion3 => s_isV3;

    /// <summary>
    /// Brings the LPI/ITS path online if the platform reports an ITS, then
    /// registers the ARM64 MSI binder. Safe to call when no ITS is present
    /// as well: it logs and returns, leaving MsiRouting unregistered so callers
    /// fall back to whatever non-MSI path they support.
    /// </summary>
    private static unsafe void InitializeMsi(ulong itsBase)
    {
        if (!s_isV3)
        {
            Serial.Write("[GIC] MSI/ITS only supported on GICv3, skipping\n");
            return;
        }
        if (itsBase == 0)
        {
            Serial.Write("[GIC] No ITS in MADT, MSI-X path disabled\n");
            return;
        }
        if (GICv3.CurrentCpuRdBase == 0)
        {
            Serial.Write("[GIC] CurrentCpuRdBase == 0, ITS init skipped\n");
            return;
        }

        DeviceMapper.EnsureMapped(itsBase);
        DeviceMapper.EnsureMapped(itsBase + ItsTranslationFrameOffset);

        // MMIO dereferences must go through the Device-memory HHDM mapping
        // EnsureMapped installed above: the TTBR0 identity map is Normal
        // WB cacheable, and dereferencing raw physical only appears to work
        // because QEMU TCG ignores memory attributes. Addresses programmed
        // INTO the hardware (GITS_TRANSLATER MSI doorbell, MAPC RDbase)
        // remain physical, so both spaces are passed explicitly. On the
        // ACPI path CurrentCpuRdBase is already the HHDM alias (Configure
        // received PhysToVirt bases) and VirtualToPhysical recovers the
        // physical; on the legacy no-ACPI fallback it is a raw identity
        // address, which VirtualToPhysical passes through unchanged.
        ulong rdVirt = GICv3.CurrentCpuRdBase;
        ulong rdPhys = PageAllocator.VirtualToPhysical(rdVirt);

        GICv3Lpi.Initialize(rdVirt);
        if (!GICv3Lpi.IsInitialized)
        {
            Serial.Write("[GIC] LPI init failed, MSI-X path disabled\n");
            return;
        }

        GICv3Its.Initialize(PhysToVirt(itsBase), itsBase, rdVirt, rdPhys);
        if (!GICv3Its.IsInitialized)
        {
            Serial.Write("[GIC] ITS init failed, MSI-X path disabled\n");
            return;
        }

        MsiRouting.RegisterBinder(new Arm64MsiBinder());
        Serial.Write("[GIC] MsiRouting registered (GICv3 ITS)\n");
    }

    /// <summary>
    /// Initializes the GIC, auto-detecting v2 or v3.
    /// Discovery priority: ACPI MADT → default QEMU addresses.
    /// </summary>
    public static unsafe void Initialize()
    {
        // Priority 1: Try ACPI MADT (parsed by C code in kmain via acpi_early_init)
        var acpiGic = AcpiGic.GetGicInfo();
        if (acpiGic != null && acpiGic->Found != 0)
        {
            s_distBase = acpiGic->DistBase;
            s_isV3 = acpiGic->Version >= GicVersionV3;

            if (s_isV3 && acpiGic->RedistBase != 0)
            {
                // Default to sysreg-only (safe on hardware where GICD/GICR
                // MMIO is inaccessible). When the firmware advertises an
                // ITS, we MUST take the full MMIO path: LPI delivery
                // requires GICR_PROPBASER/PENDBASER writes against the
                // redistributor, and the redistributor walk to populate
                // CurrentCpuRdBase, neither of which run in sysreg-only.
                bool useSysregOnly = acpiGic->ItsFound == 0;
                Serial.Write("[GIC] ACPI: GICv3 GICD=0x");
                Serial.WriteHex(acpiGic->DistBase);
                Serial.Write(" GICR=0x");
                Serial.WriteHex(acpiGic->RedistBase);
                if (acpiGic->ItsFound != 0)
                {
                    Serial.Write(" ITS=0x");
                    Serial.WriteHex(acpiGic->ItsBase);
                    Serial.Write(" (full MMIO for ITS)");
                }
                else
                {
                    Serial.Write(" (sysreg-only)");
                }
                Serial.Write("\n");

                if (useSysregOnly)
                {
                    GICv3.Configure(PhysToVirt(acpiGic->DistBase), PhysToVirt(acpiGic->RedistBase));
                    GICv3.Initialize(sysregOnly: true);
                }
                else
                {
                    DeviceMapper.EnsureMapped(acpiGic->DistBase);
                    // The redistributor walk strides 128 KiB frames until
                    // GICR_TYPER.Last, spanning the whole MADT-advertised
                    // region, and one 2 MiB block only covers 16 frames, so
                    // with more CPUs the walk would dereference past the
                    // mapping and data-abort at boot. Map every 2 MiB block
                    // the region touches (aligned loop so an unaligned
                    // base+length still covers the tail block).
                    ulong redistLength = acpiGic->RedistLength > 0 ? acpiGic->RedistLength : 1;
                    ulong redistEnd = acpiGic->RedistBase + redistLength;
                    for (ulong block = acpiGic->RedistBase & ~DeviceBlockOffsetMask; block < redistEnd; block += DeviceBlockSizeBytes)
                    {
                        DeviceMapper.EnsureMapped(block);
                    }
                    // Same PhysToVirt as the sysreg-only path: GICD/GICR
                    // accesses must use the Device mapping just installed,
                    // not the WB-cacheable TTBR0 identity alias.
                    GICv3.Configure(PhysToVirt(acpiGic->DistBase), PhysToVirt(acpiGic->RedistBase));
                    GICv3.Initialize(sysregOnly: false);
                }
            }
            else if (s_isV3)
            {
                // GICv3 but no GICR in MADT - still use sysreg-only
                Serial.Write("[GIC] ACPI: GICv3 detected (no GICR in MADT, sysreg-only)\n");
                GICv3.Initialize(sysregOnly: true);
            }
            else if (!s_isV3 && acpiGic->CpuIfBase != 0)
            {
                // GICv2 always needs MMIO
                DeviceMapper.EnsureMapped(acpiGic->DistBase);
                DeviceMapper.EnsureMapped(acpiGic->CpuIfBase);
                Serial.Write("[GIC] ACPI: GICv2 GICD=0x");
                Serial.WriteHex(acpiGic->DistBase);
                Serial.Write(" GICC=0x");
                Serial.WriteHex(acpiGic->CpuIfBase);
                Serial.Write("\n");
                GICv2.Configure(PhysToVirt(acpiGic->DistBase), PhysToVirt(acpiGic->CpuIfBase));
                GICv2.Initialize();
            }
            else
            {
                // GICv2 but no GICC in MADT - try with MMIO
                DeviceMapper.EnsureMapped(acpiGic->DistBase);
                Serial.Write("[GIC] ACPI: GICv2 detected (no GICC in MADT)\n");
                GICv2.Initialize();
            }

            s_initialized = true;
            InitializeMsi(acpiGic->ItsFound != 0 ? acpiGic->ItsBase : 0);
            return;
        }

        // Fallback: the QEMU virt machine's default addresses (no ACPI MADT;
        // the GIC is not read from the device tree)
        Serial.Write("[GIC] No DTB/ACPI, using default QEMU addresses\n");
        s_distBase = PhysToVirt(QemuVirtGicdBase);

        // Map device MMIO into TTBR1 page tables before any MMIO access
        DeviceMapper.EnsureMapped(QemuVirtGicdBase);

        // Detect GIC version via distributor PIDR2.ArchRev
        s_isV3 = GICv3.IsGICv3Available(s_distBase);

        if (s_isV3)
        {
            Serial.Write("[GIC] Detected GICv3\n");
            GICv3.Initialize();
        }
        else
        {
            Serial.Write("[GIC] Detected GICv2\n");
            GICv2.Initialize();
        }

        s_initialized = true;

        // No MADT means no authoritative ITS address. Probing QEMU's
        // default 0x08080000 blindly reads GITS_CTLR on whatever sits
        // there: on `-M virt,its=off` (or any non-QEMU board reaching
        // this fallback) that's unbacked address space and the read
        // faults or hangs the bus at boot. Without a safe probe, leave
        // MSI off and let the drivers take their polled fallback.
        if (s_isV3)
        {
            Serial.Write("[GIC] No ACPI: ITS discovery unavailable, MSI path disabled\n");
        }
    }

    /// <summary>
    /// Initializes the CPU interface for the current CPU.
    /// </summary>
    public static void InitializeCpuInterface()
    {
        if (s_isV3)
        {
            GICv3.InitializeCpuInterface();
        }
        else
        {
            GICv2.InitializeCpuInterface();
        }
    }

    /// <summary>
    /// Enables a specific interrupt.
    /// </summary>
    public static void EnableInterrupt(uint intId)
    {
        if (s_isV3)
        {
            GICv3.EnableInterrupt(intId);
        }
        else
        {
            GICv2.EnableInterrupt(intId);
        }
    }

    /// <summary>
    /// Disables a specific interrupt.
    /// </summary>
    public static void DisableInterrupt(uint intId)
    {
        if (s_isV3)
        {
            GICv3.DisableInterrupt(intId);
        }
        else
        {
            GICv2.DisableInterrupt(intId);
        }
    }

    /// <summary>
    /// Sets the priority of an interrupt.
    /// </summary>
    public static void SetPriority(uint intId, byte priority)
    {
        if (s_isV3)
        {
            GICv3.SetPriority(intId, priority);
        }
        else
        {
            GICv2.SetPriority(intId, priority);
        }
    }

    /// <summary>
    /// Acknowledges an interrupt and returns its ID.
    /// </summary>
    public static uint AcknowledgeInterrupt()
    {
        return s_isV3
            ? GICv3.AcknowledgeInterrupt()
            : GICv2.AcknowledgeInterrupt();
    }

    /// <summary>
    /// Signals the end of interrupt processing.
    /// </summary>
    public static void EndOfInterrupt(uint intId)
    {
        if (s_isV3)
        {
            GICv3.EndOfInterrupt(intId);
        }
        else
        {
            GICv2.EndOfInterrupt(intId);
        }
    }

    /// <summary>
    /// Checks if an interrupt is pending.
    /// </summary>
    public static bool IsInterruptPending(uint intId)
    {
        return s_isV3
            ? GICv3.IsInterruptPending(intId)
            : GICv2.IsInterruptPending(intId);
    }

    /// <summary>
    /// Clears a pending interrupt.
    /// </summary>
    public static void ClearPending(uint intId)
    {
        if (s_isV3)
        {
            GICv3.ClearPending(intId);
        }
        else
        {
            GICv2.ClearPending(intId);
        }
    }

    /// <summary>
    /// Configures an interrupt as edge-triggered or level-triggered.
    /// </summary>
    public static void ConfigureInterrupt(uint intId, bool edgeTriggered)
    {
        if (s_isV3)
        {
            GICv3.ConfigureInterrupt(intId, edgeTriggered);
        }
        else
        {
            GICv2.ConfigureInterrupt(intId, edgeTriggered);
        }
    }
}
