// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.ARM64.Bridge;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.ARM64.Cpu;

/// <summary>
/// ARM Generic Interrupt Controller v3 (GICv3) implementation.
/// GICv3 uses system registers (ICC_*) for the CPU interface instead of MMIO,
/// and adds a Redistributor component (one per CPU).
/// Base addresses are configurable to support both QEMU and real hardware.
/// Native imports live in Cosmos.Kernel.Core.ARM64/Bridge/Import/GICv3Native.cs.
/// </summary>
public static class GICv3
{
    /// <summary>Default GICR (redistributor) base address on the QEMU virt machine.</summary>
    private const ulong QemuVirtGicrBase = 0x080A0000;

    // Default QEMU virt machine GICv3 base addresses (overridable via Configure)
    private static ulong s_gicDistBase = GIC.QemuVirtGicdBase;
    private static ulong s_gicReDistBase = QemuVirtGicrBase;

    // Discovered redistributor base for the current CPU (found by TYPER walk)
    private static ulong s_currentCpuRdBase;
    private static ulong s_currentCpuSgiBase;

    /// <summary>
    /// Physical address field (bits [51:12], 4 KiB aligned) shared by GITS_CBASER and
    /// GICR_PROPBASER (ARM IHI 0069G §11.10). Not valid for GICR_PENDBASER (bits [51:16])
    /// or GITS_BASER (bits [47:12]), which keep their own masks.
    /// Internal: shared with the ITS and LPI configuration code.
    /// </summary>
    internal const ulong BASER_PHYS_ADDR_MASK = 0x000FFFFFFFFFF000UL;

    // Distributor registers (offsets from GICD_BASE)
    private const uint GICD_CTLR = 0x000;        // Distributor Control
    private const uint GICD_TYPER = 0x004;        // Interrupt Controller Type
    private const uint GICD_IIDR = 0x008;         // Implementer Identification
    private const uint GICD_ISENABLER = 0x100;    // Interrupt Set-Enable (base)
    private const uint GICD_ICENABLER = 0x180;    // Interrupt Clear-Enable (base)
    private const uint GICD_ISPENDR = 0x200;      // Interrupt Set-Pending (base)
    private const uint GICD_ICPENDR = 0x280;      // Interrupt Clear-Pending (base)
    private const uint GICD_IPRIORITYR = 0x400;   // Interrupt Priority (base)
    private const uint GICD_ICFGR = 0xC00;        // Interrupt Configuration (base)
    private const uint GICD_IROUTER = 0x6100;     // Interrupt Routing (base, 64-bit per SPI)

    // GICv3 Distributor CTLR bits
    private const uint GICD_CTLR_RWP = (1u << 31);       // Register Write Pending
    private const uint GICD_CTLR_ARE_NS = (1u << 4);     // Affinity Routing Enable (Non-Secure)
    private const uint GICD_CTLR_ENABLE_G1NS = (1u << 1); // Enable Group 1 Non-Secure
    private const uint GICD_CTLR_ENABLE_G1S = (1u << 2);  // Enable Group 1 Secure
    private const uint GICD_CTLR_ENABLE_G0 = (1u << 0);   // Enable Group 0

    // Redistributor registers (offsets from GICR_BASE per CPU)
    // RD_base frame (first 64KB)
    private const uint GICR_IIDR = 0x004;        // Implementer Identification
    internal const uint GICR_TYPER = 0x008;      // Redistributor Type (64-bit, shared with GICv3Its)
    private const uint GICR_WAKER = 0x014;       // Wake Register

    // SGI_base frame (second 64KB, offset 0x10000 from GICR per-CPU base)
    private const uint GICR_SGI_OFFSET = 0x10000;
    private const uint GICR_ISENABLER0 = 0x100;  // SGI/PPI Set-Enable
    private const uint GICR_ICENABLER0 = 0x180;  // SGI/PPI Clear-Enable
    private const uint GICR_ISPENDR0 = 0x200;    // SGI/PPI Set-Pending
    private const uint GICR_ICPENDR0 = 0x280;    // SGI/PPI Clear-Pending
    private const uint GICR_IPRIORITYR = 0x400;  // SGI/PPI Priority (base)
    private const uint GICR_ICFGR0 = 0xC00;      // SGI Configuration
    private const uint GICR_ICFGR1 = 0xC04;      // PPI Configuration
    private const uint GICR_IGROUPR0 = 0x080;    // SGI/PPI Group
    private const uint GICR_IGRPMODR0 = 0xD00;   // SGI/PPI Group Modifier

    // GICR_WAKER bits
    private const uint GICR_WAKER_PROCESSOR_SLEEP = (1u << 1);
    private const uint GICR_WAKER_CHILDREN_ASLEEP = (1u << 2);

    // GICR_TYPER bits
    private const ulong GICR_TYPER_LAST = (1ul << 4);

    // Redistributor stride: each CPU gets RD_base (64KB) + SGI_base (64KB) = 128KB
    private const ulong GICR_STRIDE = 0x20000;

    /// <summary>GICD_TYPER.ITLinesNumber field mask (bits [4:0]).</summary>
    private const uint GICD_TYPER_ITLINES_MASK = 0x1F;
    /// <summary>Interrupts covered per 32-bit enable/pending/group bitmap register, and per GICD_TYPER.ITLinesNumber unit.</summary>
    private const uint IntsPerBitmapReg = 32;
    /// <summary>Interrupt priorities packed per 32-bit IPRIORITYR register (one byte each).</summary>
    private const uint PrioritiesPerReg = 4;
    /// <summary>Interrupts configured per 32-bit ICFGR register (2 bits each).</summary>
    private const uint IntsPerCfgReg = 16;
    /// <summary>Configuration bits per interrupt in ICFGR registers.</summary>
    private const uint CfgBitsPerInt = 2;
    /// <summary>Byte stride of GICD_IROUTER entries (64-bit register per SPI).</summary>
    private const uint IRouterEntryBytes = 8;

    /// <summary>Mask keeping Aff3[39:32], Aff2[23:16], Aff1[15:8], Aff0[7:0] of MPIDR_EL1 (drops RES0, MT, U).</summary>
    private const ulong MpidrAffinityMask = 0xFF00FFFFFFul;
    /// <summary>Bit position of the Affinity_Value field in GICR_TYPER (bits [63:32]).</summary>
    private const int GicrTyperAffShift = 32;
    /// <summary>Safety limit on the number of redistributor frames walked during TYPER discovery.</summary>
    private const int MaxRedistFrames = 256;

    /// <summary>ICC_SRE_EL1.SRE bit - enables system register access to the CPU interface.</summary>
    private const uint IccSreSreBit = 0x1;
    /// <summary>ICC_IAR1_EL1 INTID field mask (24 bits wide, so LPIs up to 16777215 are not truncated).</summary>
    private const uint Iar1IntIdMask = 0xFFFFFF;
    /// <summary>ICC_HPPIR1_EL1 INTID mask (10 bits, SGI/PPI/SPI range).</summary>
    private const uint Hppir1IntIdMask = 0x3FF;

    /// <summary>Mask limiting an SGI number to its 4-bit range (0-15).</summary>
    private const uint SgiIdMask = 0xF;
    /// <summary>Bit position of the INTID field in ICC_SGI1R_EL1 (bits [27:24]).</summary>
    private const int Sgi1rIntIdShift = 24;
    /// <summary>ICC_SGI1R_EL1.IRM bit (bit 40): route to all PEs other than self.</summary>
    private const ulong Sgi1rIrmBit = (1ul << 40);
    /// <summary>ICC_SGI1R_EL1 TargetList bit for CPU 0 (self on the boot CPU).</summary>
    private const ulong Sgi1rTargetListCpu0 = 1;

    /// <summary>GICD_PIDR2 offset in the GICv2-compatible 4KB register frame.</summary>
    private const uint Pidr2OffsetGicv2 = 0xFE8;
    /// <summary>GICD_PIDR2 offset in the GICv3 64KB register frame.</summary>
    private const uint Pidr2OffsetGicv3 = 0xFFE8;
    /// <summary>Bit position of the ArchRev field in GICD_PIDR2 (bits [7:4]).</summary>
    private const int Pidr2ArchRevShift = 4;
    /// <summary>ArchRev field mask (4 bits) in GICD_PIDR2.</summary>
    private const uint Pidr2ArchRevMask = 0xF;
    /// <summary>Minimum GICD_PIDR2.ArchRev value indicating a GICv3 implementation.</summary>
    private const uint Gicv3MinArchRev = 3;

    /// <summary>Spin iterations allowed for GICR_WAKER.ChildrenAsleep to clear (generous for real hardware).</summary>
    private const uint RedistWakeTimeoutIterations = 10000000;
    /// <summary>Spin iterations allowed for GICD_CTLR.RWP to clear after a distributor write.</summary>
    private const uint RwpTimeoutIterations = 1000000;

    private static bool s_initialized;
    private static bool s_mmioAvailable;

    /// <summary>
    /// Whether the GICv3 has been initialized.
    /// </summary>
    public static bool IsInitialized => s_initialized;

    /// <summary>
    /// Whether GICD/GICR MMIO is accessible. False on devices where
    /// the GIC bus doesn't respond (e.g., Qualcomm wearable SoCs).
    /// </summary>
    public static bool IsMmioAvailable => s_mmioAvailable;

    /// <summary>
    /// RD_base of the redistributor for the current (boot) CPU, discovered
    /// via TYPER walk. Exposed so the LPI / ITS drivers can program
    /// PROPBASER/PENDBASER and the ITS collection-mapping target.
    /// </summary>
    public static ulong CurrentCpuRdBase => s_currentCpuRdBase;

    /// <summary>
    /// Configures the GICv3 base addresses. Must be called before Initialize()
    /// if running on hardware with non-QEMU addresses (e.g., from DTB).
    /// Both bases are pure MMIO dereference bases (never programmed into
    /// the hardware as values), so callers must pass DEREFERENCEABLE
    /// addresses — the HHDM alias of a DeviceMapper-mapped region, not raw
    /// physical (the TTBR0 identity alias is Normal WB cacheable).
    /// </summary>
    /// <param name="distBase">GICD base address (virtual).</param>
    /// <param name="redistBase">GICR base address (virtual).</param>
    public static void Configure(ulong distBase, ulong redistBase)
    {
        s_gicDistBase = distBase;
        s_gicReDistBase = redistBase;
        Serial.Write("[GIC] Configured GICD=0x");
        Serial.WriteHex(distBase);
        Serial.Write(" GICR=0x");
        Serial.WriteHex(redistBase);
        Serial.Write("\n");
    }

    /// <summary>
    /// Initializes the GICv3 distributor, redistributor, and CPU interface.
    /// On real hardware (Snapdragon, Exynos), the secure firmware (EL3/TZ) owns the
    /// distributor. We must NOT disable it entirely — only configure NS Group 1.
    /// </summary>
    /// <param name="sysregOnly">If true, skip all GICD/GICR MMIO and only configure
    /// the CPU interface via ICC_* system registers. Use this on platforms where
    /// GIC MMIO causes a bus hang (e.g., Qualcomm wearable SoCs).</param>
    public static void Initialize(bool sysregOnly = false)
    {
        Serial.Write("[GIC] Initializing GICv3...\n");
        Serial.Write("[GIC] GICD base: 0x");
        Serial.WriteHex(s_gicDistBase);
        Serial.Write(" GICR base: 0x");
        Serial.WriteHex(s_gicReDistBase);
        Serial.Write("\n");

        // Step 1: Enable system register access (critical for real hardware)
        EnableSystemRegisterAccess();

        if (sysregOnly)
        {
            Serial.Write("[GIC] Sysreg-only mode: skipping GICD/GICR MMIO\n");
            s_mmioAvailable = false;
            InitializeSysregOnly();
            return;
        }
        s_mmioAvailable = true;

        // Step 3: Read GIC type to get number of interrupt lines
        uint typer = ReadDistributor(GICD_TYPER);
        Serial.Write("[GIC] GICD_TYPER=0x");
        Serial.WriteHex(typer);
        uint itLinesNumber = typer & GICD_TYPER_ITLINES_MASK;
        uint maxInterrupts = IntsPerBitmapReg * (itLinesNumber + 1);
        Serial.Write(" Max interrupts: ");
        Serial.WriteNumber(maxInterrupts);
        Serial.Write("\n");

        // Step 4: Read current CTLR to understand firmware state
        Serial.Write("[GIC] Reading GICD_CTLR...\n");
        uint ctlr = ReadDistributor(GICD_CTLR);
        Serial.Write("[GIC] Current GICD_CTLR=0x");
        Serial.WriteHex(ctlr);
        Serial.Write("\n");

        // Step 4a: Enable affinity routing (ARE_NS) without disabling the distributor.
        // On real hardware, secure firmware may have Group 0 active — do NOT write 0.
        // Just ensure ARE_NS is set, then enable Group 1 NS.
        Serial.Write("[GIC] Setting ARE_NS...\n");
        WriteDistributor(GICD_CTLR, ctlr | GICD_CTLR_ARE_NS);
        WaitForDistributorWrite();
        Serial.Write("[GIC] ARE_NS set\n");

        // Step 5: Configure all SPIs - disable, clear pending, set priority
        Serial.Write("[GIC] Configuring SPIs...\n");
        for (uint i = GIC.SPI_START; i < maxInterrupts; i += IntsPerBitmapReg)
        {
            WriteDistributor(GICD_ICENABLER + ((i / IntsPerBitmapReg) * GicArch.RegisterStrideBytes), GicArch.AllInterruptsMask);
            WriteDistributor(GICD_ICPENDR + ((i / IntsPerBitmapReg) * GicArch.RegisterStrideBytes), GicArch.AllInterruptsMask);
        }

        for (uint i = GIC.SPI_START; i < maxInterrupts; i += PrioritiesPerReg)
        {
            WriteDistributor(GICD_IPRIORITYR + i, GicArch.DefaultPriorityAllBytes);
        }

        for (uint i = GIC.SPI_START; i < maxInterrupts; i += IntsPerCfgReg)
        {
            WriteDistributor(GICD_ICFGR + ((i / IntsPerCfgReg) * GicArch.RegisterStrideBytes), 0);
        }
        Serial.Write("[GIC] SPIs configured\n");

        // Step 6: Read current CPU's MPIDR affinity for SPI routing
        ulong mpidr = GICv3Native.ReadMpidr();
        ulong affinity = ExtractAffinity(mpidr);
        Serial.Write("[GIC] CPU MPIDR=0x");
        Serial.WriteHex(mpidr);
        Serial.Write(" affinity=0x");
        Serial.WriteHex(affinity);
        Serial.Write("\n");

        // Route all SPIs to current CPU using MPIDR affinity
        Serial.Write("[GIC] Routing SPIs...\n");
        for (uint i = GIC.SPI_START; i < maxInterrupts; i++)
        {
            WriteDistributor64(GICD_IROUTER + ((i - GIC.SPI_START) * IRouterEntryBytes), affinity);
        }
        Serial.Write("[GIC] SPIs routed\n");

        // Step 7: Enable Group 1 NS interrupts in distributor (additive, keep existing bits)
        Serial.Write("[GIC] Enabling G1NS in distributor...\n");
        ctlr = ReadDistributor(GICD_CTLR);
        WriteDistributor(GICD_CTLR, ctlr | GICD_CTLR_ARE_NS | GICD_CTLR_ENABLE_G1NS);
        WaitForDistributorWrite();
        Serial.Write("[GIC] Distributor enabled\n");

        // Step 8: Find and initialize redistributor for current CPU by MPIDR walk
        Serial.Write("[GIC] Finding redistributor...\n");
        if (!FindAndInitRedistributor(mpidr))
        {
            Serial.Write("[GIC] WARNING: Failed to find redistributor, trying index 0\n");
            // Fallback: assume first redistributor
            s_currentCpuRdBase = s_gicReDistBase;
            s_currentCpuSgiBase = s_gicReDistBase + GICR_SGI_OFFSET;
            InitializeRedistributorAt(s_currentCpuRdBase, s_currentCpuSgiBase);
        }

        // Step 9: Initialize CPU interface via system registers
        InitializeCpuInterface();

        s_initialized = true;
        Serial.Write("[GIC] GICv3 initialized (full MMIO)\n");
    }

    /// <summary>
    /// System-register-only initialization for platforms where GICD/GICR MMIO
    /// is inaccessible (bus hang). Relies on firmware having already configured
    /// the distributor and redistributor (common on Android/WearOS devices).
    /// Only the CPU interface (ICC_* registers) is touched.
    /// </summary>
    private static void InitializeSysregOnly()
    {
        Serial.Write("[GIC] Sysreg-only init: trusting firmware GIC config\n");

        // The CPU interface is fully system-register-based in GICv3.
        // Firmware already configured:
        //   - GICD: enabled, ARE_NS set, Group 1 NS enabled
        //   - GICR: awake, PPIs/SGIs configured
        // We just need to set up the CPU interface for our EL1 context.
        InitializeCpuInterface();

        s_initialized = true;
        Serial.Write("[GIC] GICv3 initialized (sysreg-only)\n");
    }

    /// <summary>
    /// Enables ICC_SRE_EL1.SRE so that system registers can be used for the CPU interface.
    /// On QEMU this is a no-op (already enabled), but on real hardware (e.g., Exynos)
    /// this MUST be done before any ICC_* register access.
    /// </summary>
    private static void EnableSystemRegisterAccess()
    {
        uint sre = GICv3Native.ReadSre();
        Serial.Write("[GIC] ICC_SRE_EL1=0x");
        Serial.WriteHex(sre);
        Serial.Write("\n");

        if ((sre & IccSreSreBit) == 0)
        {
            // Enable SRE bit
            sre |= IccSreSreBit;
            GICv3Native.WriteSre(sre);

            // Verify it took effect
            sre = GICv3Native.ReadSre();
            if ((sre & IccSreSreBit) == 0)
            {
                Serial.Write("[GIC] WARNING: Failed to enable ICC_SRE_EL1.SRE\n");
            }
            else
            {
                Serial.Write("[GIC] ICC_SRE_EL1.SRE enabled\n");
            }
        }
        else
        {
            Serial.Write("[GIC] ICC_SRE_EL1.SRE already enabled\n");
        }
    }

    /// <summary>
    /// Extracts the routing affinity value from MPIDR_EL1 in the format
    /// expected by GICD_IROUTER and GICR_TYPER:
    /// bits [31:24] = Aff3, [23:16] = Aff2, [15:8] = Aff1, [7:0] = Aff0
    /// </summary>
    private static ulong ExtractAffinity(ulong mpidr)
    {
        // MPIDR_EL1: Aff3[39:32], Aff2[23:16], Aff1[15:8], Aff0[7:0]
        // IROUTER:    Aff3[39:32], Aff2[23:16], Aff1[15:8], Aff0[7:0]
        // They share the same layout for the affinity fields
        return mpidr & MpidrAffinityMask; // mask out non-affinity bits (RES0, MT, U)
    }

    /// <summary>
    /// Walks the redistributor chain by reading GICR_TYPER to find the
    /// redistributor whose affinity matches the current CPU's MPIDR.
    /// This is the correct way to find redistributors on real hardware
    /// where stride/layout may differ from QEMU.
    /// </summary>
    private static bool FindAndInitRedistributor(ulong mpidr)
    {
        ulong cpuAff = ExtractAffinity(mpidr);
        ulong rdBase = s_gicReDistBase;

        // Walk redistributor frames using GICR_TYPER.Last bit
        for (int i = 0; i < MaxRedistFrames; i++) // safety limit
        {
            ulong typer = ReadRedistributor64(rdBase, GICR_TYPER);

            // GICR_TYPER affinity is in bits [39:32][23:8] → same layout as MPIDR
            ulong rdAff = typer >> GicrTyperAffShift;
            // Shift to match: GICR_TYPER[63:32] contains Aff3[31:24]|0[23]|Aff2[20:16]|Aff1[15:8]|Aff0[7:0]
            // Actually GICR_TYPER bits [39:32] = Aff3, [31:24]=Aff2(??).. let's use the spec:
            // GICR_TYPER: [63:32] = Affinity_Value (Aff3[63:56] Aff2[55:48] Aff1[47:40] Aff0[39:32])
            // But reading 64-bit at offset 0x8 gives us the full register
            // Actually: GICR_TYPER[63:32] = Aff3[63:56] | res0[55:48] | Aff2[47:40] | Aff1[39:32]
            // Wait - let's just compare directly with MPIDR masked:
            // GICR_TYPER affinity in bits[39:32] = Aff0, [47:40] = Aff1, [55:48] = Aff2, [63:56] = Aff3
            ulong rdAffValue = (typer >> GicrTyperAffShift) & MpidrAffinityMask;

            Serial.Write("[GIC] RD@0x");
            Serial.WriteHex(rdBase);
            Serial.Write(" TYPER=0x");
            Serial.WriteHex(typer);
            Serial.Write(" aff=0x");
            Serial.WriteHex(rdAffValue);
            Serial.Write("\n");

            if (rdAffValue == cpuAff)
            {
                Serial.Write("[GIC] Found matching redistributor at 0x");
                Serial.WriteHex(rdBase);
                Serial.Write("\n");

                s_currentCpuRdBase = rdBase;
                s_currentCpuSgiBase = rdBase + GICR_SGI_OFFSET;
                InitializeRedistributorAt(rdBase, rdBase + GICR_SGI_OFFSET);
                return true;
            }

            // Check if this is the last redistributor
            if ((typer & GICR_TYPER_LAST) != 0)
            {
                Serial.Write("[GIC] Reached last redistributor without match\n");
                return false;
            }

            // Move to next redistributor frame (RD_base + SGI_base = 128KB)
            rdBase += GICR_STRIDE;
        }

        return false;
    }

    /// <summary>
    /// Initializes a specific redistributor at the given base addresses.
    /// </summary>
    private static void InitializeRedistributorAt(ulong rdBase, ulong sgiBase)
    {
        Serial.Write("[GIC] Initializing redistributor at 0x");
        Serial.WriteHex(rdBase);
        Serial.Write("...\n");

        // Wake up the redistributor
        uint waker = ReadRedistributor(rdBase, GICR_WAKER);
        waker &= ~GICR_WAKER_PROCESSOR_SLEEP;
        WriteRedistributor(rdBase, GICR_WAKER, waker);

        // Wait for children to wake up (with generous timeout for real hardware)
        uint timeout = RedistWakeTimeoutIterations;
        while ((ReadRedistributor(rdBase, GICR_WAKER) & GICR_WAKER_CHILDREN_ASLEEP) != 0)
        {
            if (--timeout == 0)
            {
                Serial.Write("[GIC] WARNING: Redistributor wake timeout\n");
                break;
            }
        }

        // Set all SGIs/PPIs to Group 1 Non-Secure
        WriteRedistributor(sgiBase, GICR_IGROUPR0, GicArch.AllInterruptsMask);
        WriteRedistributor(sgiBase, GICR_IGRPMODR0, 0x00000000);

        // Disable all SGIs and PPIs
        WriteRedistributor(sgiBase, GICR_ICENABLER0, GicArch.AllInterruptsMask);

        // Clear all pending
        WriteRedistributor(sgiBase, GICR_ICPENDR0, GicArch.AllInterruptsMask);

        // Set default priority for all SGIs and PPIs
        for (uint i = GIC.SGI_START; i < GIC.SPI_START; i += PrioritiesPerReg)
        {
            WriteRedistributor(sgiBase, GICR_IPRIORITYR + i, GicArch.DefaultPriorityAllBytes);
        }

        // Configure SGIs as edge-triggered, PPIs as level-triggered
        WriteRedistributor(sgiBase, GICR_ICFGR0, 0);
        WriteRedistributor(sgiBase, GICR_ICFGR1, 0);

        Serial.Write("[GIC] Redistributor initialized\n");
    }

    /// <summary>
    /// Initializes the GICv3 CPU interface via system registers.
    /// </summary>
    public static void InitializeCpuInterface()
    {
        Serial.Write("[GIC] Initializing CPU interface (system registers)...\n");

        // Disable Group 1 interrupts during configuration
        GICv3Native.WriteIgrpen1(0);

        // Set priority mask to allow all priorities
        GICv3Native.WritePmr(GicArch.PriorityMaskAllowAll);

        // Set binary point to 0
        GICv3Native.WriteBpr1(0);

        // Enable Group 1 Non-Secure interrupts
        GICv3Native.WriteIgrpen1(1);

        Serial.Write("[GIC] CPU interface initialized\n");
    }

    /// <summary>
    /// Enables a specific interrupt.
    /// In sysreg-only mode, this is a no-op (firmware config is trusted).
    /// </summary>
    /// <param name="intId">Interrupt ID (0-1019).</param>
    public static void EnableInterrupt(uint intId)
    {
        if (!s_mmioAvailable)
        {
            Serial.Write("[GIC] EnableInterrupt(");
            Serial.WriteNumber(intId);
            Serial.Write("): skipped (no MMIO, trusting firmware)\n");
            return;
        }

        if (intId < GIC.SPI_START)
        {
            // SGI/PPI: use redistributor SGI_base frame
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            WriteRedistributor(s_currentCpuSgiBase, GICR_ISENABLER0, bit);
        }
        else
        {
            // SPI: use distributor
            uint regOffset = GICD_ISENABLER + ((intId / IntsPerBitmapReg) * GicArch.RegisterStrideBytes);
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            WriteDistributor(regOffset, bit);
        }

        Serial.Write("[GIC] Enabled interrupt ");
        Serial.WriteNumber(intId);
        Serial.Write("\n");
    }

    /// <summary>
    /// Disables a specific interrupt.
    /// In sysreg-only mode, this is a no-op.
    /// </summary>
    /// <param name="intId">Interrupt ID.</param>
    public static void DisableInterrupt(uint intId)
    {
        if (!s_mmioAvailable)
        {
            return;
        }

        if (intId < GIC.SPI_START)
        {
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            WriteRedistributor(s_currentCpuSgiBase, GICR_ICENABLER0, bit);
        }
        else
        {
            uint regOffset = GICD_ICENABLER + ((intId / IntsPerBitmapReg) * GicArch.RegisterStrideBytes);
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            WriteDistributor(regOffset, bit);
        }
    }

    /// <summary>
    /// Sets the priority of an interrupt.
    /// In sysreg-only mode, this is a no-op (firmware config is trusted).
    /// </summary>
    /// <param name="intId">Interrupt ID.</param>
    /// <param name="priority">Priority (0 = highest, 0xFF = lowest).</param>
    public static void SetPriority(uint intId, byte priority)
    {
        if (!s_mmioAvailable)
        {
            Serial.Write("[GIC] SetPriority(");
            Serial.WriteNumber(intId);
            Serial.Write(", 0x");
            Serial.WriteHex(priority);
            Serial.Write("): skipped (no MMIO)\n");
            return;
        }

        if (intId < GIC.SPI_START)
        {
            // SGI/PPI: use redistributor SGI_base frame
            unsafe
            {
                byte* ptr = (byte*)(s_currentCpuSgiBase + GICR_IPRIORITYR + intId);
                *ptr = priority;
            }
        }
        else
        {
            // SPI: use distributor
            uint regOffset = GICD_IPRIORITYR + intId;
            unsafe
            {
                byte* ptr = (byte*)(s_gicDistBase + regOffset);
                *ptr = priority;
            }
        }
    }

    /// <summary>
    /// Acknowledges an interrupt and returns its ID.
    /// Uses ICC_IAR1_EL1 system register. The INTID field is 24 bits wide
    /// (LPIs occupy 8192..16777215), so we mask to that width — masking to
    /// 10 bits would truncate any LPI delivered via the ITS.
    /// </summary>
    /// <returns>The interrupt ID, or 1023 if spurious.</returns>
    public static uint AcknowledgeInterrupt()
    {
        return GICv3Native.ReadIar1() & Iar1IntIdMask;
    }

    /// <summary>
    /// Signals the end of interrupt processing.
    /// Uses ICC_EOIR1_EL1 system register.
    /// </summary>
    /// <param name="intId">The interrupt ID that was acknowledged.</param>
    public static void EndOfInterrupt(uint intId)
    {
        GICv3Native.WriteEoir1(intId);
    }

    /// <summary>
    /// Checks if an interrupt is pending.
    /// In sysreg-only mode, uses ICC_HPPIR1_EL1 to check highest pending.
    /// </summary>
    /// <param name="intId">Interrupt ID.</param>
    /// <returns>True if pending.</returns>
    public static bool IsInterruptPending(uint intId)
    {
        if (!s_mmioAvailable)
        {
            // Best effort: check if the highest pending interrupt matches
            uint hppir = GICv3Native.ReadHppir1() & Hppir1IntIdMask;
            return hppir == intId;
        }

        if (intId < GIC.SPI_START)
        {
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            return (ReadRedistributor(s_currentCpuSgiBase, GICR_ISPENDR0) & bit) != 0;
        }
        else
        {
            uint regOffset = GICD_ISPENDR + ((intId / IntsPerBitmapReg) * GicArch.RegisterStrideBytes);
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            return (ReadDistributor(regOffset) & bit) != 0;
        }
    }

    /// <summary>
    /// Clears a pending interrupt.
    /// In sysreg-only mode, this is a no-op.
    /// </summary>
    /// <param name="intId">Interrupt ID.</param>
    public static void ClearPending(uint intId)
    {
        if (!s_mmioAvailable)
        {
            return;
        }

        if (intId < GIC.SPI_START)
        {
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            WriteRedistributor(s_currentCpuSgiBase, GICR_ICPENDR0, bit);
        }
        else
        {
            uint regOffset = GICD_ICPENDR + ((intId / IntsPerBitmapReg) * GicArch.RegisterStrideBytes);
            uint bit = 1u << (int)(intId % IntsPerBitmapReg);
            WriteDistributor(regOffset, bit);
        }
    }

    /// <summary>
    /// Configures an interrupt as edge-triggered or level-triggered.
    /// In sysreg-only mode, this is a no-op (firmware config is trusted).
    /// </summary>
    /// <param name="intId">Interrupt ID.</param>
    /// <param name="edgeTriggered">True for edge-triggered, false for level-triggered.</param>
    public static void ConfigureInterrupt(uint intId, bool edgeTriggered)
    {
        if (!s_mmioAvailable)
        {
            return;
        }

        if (intId < GIC.SPI_START)
        {
            // SGI/PPI: use redistributor
            uint regIdx = intId < GIC.PPI_START ? 0u : 1u;
            uint regOffset = (regIdx == 0) ? GICR_ICFGR0 : GICR_ICFGR1;
            uint localId = intId - (regIdx * IntsPerCfgReg);
            uint shift = localId * CfgBitsPerInt;
            uint value = ReadRedistributor(s_currentCpuSgiBase, regOffset);

            if (edgeTriggered)
            {
                value |= (GicArch.IcfgrEdgeTriggered << (int)shift);
            }
            else
            {
                value &= ~(GicArch.IcfgrEdgeTriggered << (int)shift);
            }

            WriteRedistributor(s_currentCpuSgiBase, regOffset, value);
        }
        else
        {
            uint regOffset = GICD_ICFGR + ((intId / IntsPerCfgReg) * GicArch.RegisterStrideBytes);
            uint shift = (intId % IntsPerCfgReg) * CfgBitsPerInt;
            uint value = ReadDistributor(regOffset);

            if (edgeTriggered)
            {
                value |= (GicArch.IcfgrEdgeTriggered << (int)shift);
            }
            else
            {
                value &= ~(GicArch.IcfgrEdgeTriggered << (int)shift);
            }

            WriteDistributor(regOffset, value);
        }
    }

    /// <summary>
    /// Sends a Software Generated Interrupt (SGI) to the specified target.
    /// GICv3 uses ICC_SGI1R_EL1 for SGI generation.
    /// </summary>
    /// <param name="sgiId">SGI ID (0-15).</param>
    /// <param name="targetSelf">If true, target the current CPU.</param>
    public static void SendSGI(uint sgiId, bool targetSelf)
    {
        // ICC_SGI1R_EL1 format:
        // [27:24] = INTID (SGI number)
        // [40]    = IRM (1 = all other PEs, 0 = use target list)
        // [15:0]  = TargetList (bit mask of target CPUs in lowest affinity level)
        ulong value = ((ulong)(sgiId & SgiIdMask) << Sgi1rIntIdShift);

        if (targetSelf)
        {
            // Target self: set TargetList bit 0 (assumes CPU 0)
            value |= Sgi1rTargetListCpu0;
        }
        else
        {
            // Broadcast to all other PEs
            value |= Sgi1rIrmBit; // IRM = 1
        }

        GICv3Native.WriteSgi1r(value);
    }

    /// <summary>
    /// Detects whether the system has GICv3 support by reading GICD_PIDR2.ArchRev.
    /// Uses a two-step probe: first reads at the GICv2-compatible offset (0xFE8, within
    /// the 4KB page - always safe), then falls back to the GICv3 offset (0xFFE8, within
    /// the 64KB page) if the first read returns zero (GICv3 returns RAZ for reserved
    /// offsets in its first 4KB).
    /// </summary>
    /// <param name="distBase">GICD base address to probe.</param>
    /// <returns>True if GICv3 (ArchRev >= 3).</returns>
    public static bool IsGICv3Available(ulong distBase)
    {
        unsafe
        {
            // Step 1: Read PIDR2 at GICv2 offset (0xFE8) - always within 4KB, safe for both v2 and v3
            uint* ptr = (uint*)(distBase + Pidr2OffsetGicv2);
            uint pidr2 = System.Threading.Volatile.Read(ref *ptr);
            uint archRev = (pidr2 >> Pidr2ArchRevShift) & Pidr2ArchRevMask;

            Serial.Write("[GIC] PIDR2@0xFE8=0x");
            Serial.WriteHex(pidr2);

            if (pidr2 != 0)
            {
                // Got a valid PIDR2 from the 4KB-compatible offset
                Serial.Write(" ArchRev=");
                Serial.WriteNumber(archRev);
                Serial.Write("\n");
                return archRev >= Gicv3MinArchRev;
            }

            // Step 2: PIDR2 at 0xFE8 returned 0 - this happens on GICv3 where
            // the first 4KB doesn't have ID registers. Try 0xFFE8 (64KB layout).
            // This is safe because if 0xFE8 returned 0 (not faulted), the region
            // is mapped to at least 4KB, and a GICv3 maps 64KB.
            Serial.Write(" (zero, trying 0xFFE8)\n");
            ptr = (uint*)(distBase + Pidr2OffsetGicv3);
            pidr2 = System.Threading.Volatile.Read(ref *ptr);
            archRev = (pidr2 >> Pidr2ArchRevShift) & Pidr2ArchRevMask;

            Serial.Write("[GIC] PIDR2@0xFFE8=0x");
            Serial.WriteHex(pidr2);
            Serial.Write(" ArchRev=");
            Serial.WriteNumber(archRev);
            Serial.Write("\n");

            return archRev >= Gicv3MinArchRev;
        }
    }

    // Wait for distributor write to complete
    private static void WaitForDistributorWrite()
    {
        uint timeout = RwpTimeoutIterations;
        while ((ReadDistributor(GICD_CTLR) & GICD_CTLR_RWP) != 0)
        {
            if (--timeout == 0)
            {
                Serial.Write("[GIC] WARNING: Distributor RWP timeout\n");
                break;
            }
        }
    }

    // Distributor MMIO access (uses runtime s_gicDistBase)
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoInlining)]
    private static uint ReadDistributor(uint offset)
    {
        unsafe
        {
            uint* ptr = (uint*)(s_gicDistBase + offset);
            return System.Threading.Volatile.Read(ref *ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoInlining)]
    private static void WriteDistributor(uint offset, uint value)
    {
        unsafe
        {
            uint* ptr = (uint*)(s_gicDistBase + offset);
            System.Threading.Volatile.Write(ref *ptr, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoInlining)]
    private static void WriteDistributor64(uint offset, ulong value)
    {
        unsafe
        {
            ulong* ptr = (ulong*)(s_gicDistBase + offset);
            System.Threading.Volatile.Write(ref *ptr, value);
        }
    }

    // Redistributor MMIO access
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoInlining)]
    private static uint ReadRedistributor(ulong baseAddr, uint offset)
    {
        unsafe
        {
            uint* ptr = (uint*)(baseAddr + offset);
            return System.Threading.Volatile.Read(ref *ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoInlining)]
    private static ulong ReadRedistributor64(ulong baseAddr, uint offset)
    {
        unsafe
        {
            ulong* ptr = (ulong*)(baseAddr + offset);
            return System.Threading.Volatile.Read(ref *ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.NoInlining)]
    private static void WriteRedistributor(ulong baseAddr, uint offset, uint value)
    {
        unsafe
        {
            uint* ptr = (uint*)(baseAddr + offset);
            System.Threading.Volatile.Write(ref *ptr, value);
        }
    }
}
