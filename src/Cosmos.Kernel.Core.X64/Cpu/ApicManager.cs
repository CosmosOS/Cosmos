// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// Manages APIC initialization and configuration.
/// </summary>
public static class ApicManager
{
    private static bool s_initialized;

    /// <summary>
    /// Gets whether the Local APIC is up: its timer is calibrated, an
    /// interrupt it delivers takes an EOI, and MSI messages reach it.
    /// </summary>
    public static bool IsInitialized => s_initialized;

    /// <summary>
    /// Gets whether a hardware line can be routed: the Local APIC is up and
    /// the MADT described an I/O APIC to program. Any context.
    /// </summary>
    internal static bool CanRouteIrqs => s_initialized && IoApic.IsInitialized;

    /// <summary>
    /// Initializes the Local APIC from IA32_APIC_BASE and the I/O APIC from
    /// the MADT. The Local APIC is architectural, so a machine without ACPI
    /// still gets its timer, its EOIs and MSI delivery; only the I/O APIC
    /// and the ISA overrides need the table, and without it no hardware line
    /// is routed.
    /// </summary>
    public static unsafe void Initialize()
    {
        Serial.Write("[ApicManager] Starting APIC initialization...\n");

        // Disable the legacy 8259 PIC first
        LegacyPic.RemapAndDisable();

        if (!LocalApic.TryReadBaseAddress(out ulong localApicBase))
        {
            Serial.Write("[ApicManager] ERROR: no enabled Local APIC on this CPU!\n");
            return;
        }

        Serial.Write("[ApicManager] Initializing Local APIC...\n");
        LocalApic.Initialize(localApicBase);

        Serial.Write("[ApicManager] Calibrating LAPIC timer...\n");
        LocalApic.CalibrateTimer();

        s_initialized = true;

        MadtInfo* madtPtr = AcpiMadt.GetMadtInfoPtr();
        if (madtPtr == null)
        {
            Serial.Write("[ApicManager] WARNING: no MADT, so no I/O APIC: hardware lines will not be routed\n");
        }
        else if (madtPtr->IoApics.Length > 0)
        {
            Serial.Write("[ApicManager] Initializing I/O APIC...\n");
            // For now, just use the first I/O APIC
            IoApic.Initialize(madtPtr->IoApics[0]);
        }
        else
        {
            Serial.Write("[ApicManager] WARNING: No I/O APIC found in MADT!\n");
        }

        Serial.Write("[ApicManager] APIC system initialized\n");
    }

    /// <summary>
    /// Routes an ISA IRQ to an interrupt vector, on the GSI its MADT
    /// interrupt source override names when there is one. The I/O APIC
    /// register pair runs with interrupts disabled: it shares one IOREGSEL
    /// latch with <see cref="MaskIrq"/>, which a handler may call.
    /// </summary>
    /// <param name="irq">ISA IRQ number (0-15).</param>
    /// <param name="vector">Target interrupt vector.</param>
    /// <param name="startMasked">If true, the IRQ starts masked and must be explicitly unmasked.</param>
    public static void RouteIrq(byte irq, byte vector, bool startMasked = false)
    {
        if (!CanRouteIrqs)
        {
            Serial.Write("[ApicManager] ERROR: no I/O APIC to route IRQ ", irq, " through!\n");
            return;
        }

        IrqOverride? irqOverride = FindOverride(irq);
        byte targetApicId = LocalApic.GetId();
        using (InternalCpu.DisableInterruptsScope())
        {
            IoApic.RouteIrq(irq, vector, targetApicId, irqOverride, startMasked);
        }
    }

    /// <summary>
    /// Sends End of Interrupt signal.
    /// Must be called at the end of every interrupt handler.
    /// </summary>
    public static void SendEOI()
    {
        LocalApic.SendEOI();
    }

    /// <summary>
    /// Masks (disables) an ISA IRQ at the I/O APIC level, on the same GSI
    /// <see cref="RouteIrq"/> programmed for it (the MADT override applied).
    /// The IOREGSEL/IOWIN pair runs with interrupts disabled, since a
    /// handler masking its own line would otherwise cut another caller's
    /// pair in half. Allocation-free; any context.
    /// </summary>
    /// <param name="irq">ISA IRQ number (0-15).</param>
    public static void MaskIrq(byte irq)
    {
        uint gsi = ResolveGsi(irq);
        using (InternalCpu.DisableInterruptsScope())
        {
            IoApic.MaskIrq(gsi);
        }
    }

    /// <summary>
    /// Unmasks (enables) an ISA IRQ at the I/O APIC level, on the same GSI
    /// <see cref="RouteIrq"/> programmed for it. Same rules as
    /// <see cref="MaskIrq"/>. Allocation-free; any context.
    /// </summary>
    /// <param name="irq">ISA IRQ number (0-15).</param>
    public static void UnmaskIrq(byte irq)
    {
        uint gsi = ResolveGsi(irq);
        using (InternalCpu.DisableInterruptsScope())
        {
            IoApic.UnmaskIrq(gsi);
        }
    }

    /// <summary>The GSI an ISA IRQ arrives on: its MADT override's, or the IRQ number itself.</summary>
    /// <param name="irq">ISA IRQ number (0-15).</param>
    private static uint ResolveGsi(byte irq) => FindOverride(irq) is { } irqOverride ? irqOverride.Gsi : irq;

    /// <summary>
    /// The MADT interrupt source override for <paramref name="irq"/>, when
    /// firmware declares one: the GSI the ISA line really arrives on, with
    /// its polarity and trigger. Read in place from the table the early
    /// ACPI parse filled. Allocation-free; any context.
    /// </summary>
    /// <param name="irq">ISA IRQ number (0-15).</param>
    private static unsafe IrqOverride? FindOverride(byte irq)
    {
        MadtInfo* madtPtr = AcpiMadt.GetMadtInfoPtr();
        if (madtPtr == null)
        {
            return null;
        }

        ReadOnlySpan<IrqOverride> overrides = madtPtr->Overrides;
        for (int i = 0; i < overrides.Length; i++)
        {
            if (overrides[i].Source == irq)
            {
                return overrides[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Blocks for the specified number of milliseconds using the LAPIC timer.
    /// </summary>
    /// <param name="ms">Number of milliseconds to wait.</param>
    public static void Wait(uint ms)
    {
        LocalApic.Wait(ms);
    }

    /// <summary>
    /// Gets whether the LAPIC timer is calibrated.
    /// </summary>
    public static bool IsTimerCalibrated => LocalApic.IsTimerCalibrated;
}
