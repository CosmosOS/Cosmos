// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.X64.Cpu;

/// <summary>
/// I/O APIC (Advanced Programmable Interrupt Controller) driver.
/// Routes external interrupts (IRQs) to Local APICs.
/// </summary>
public static class IoApic
{
    // I/O APIC register select (write to select register)
    private const uint IOREGSEL = 0x00;
    // I/O APIC register data (read/write selected register)
    private const uint IOWIN = 0x10;

    // I/O APIC registers (accessed via IOREGSEL/IOWIN)
    private const byte IOAPIC_ID = 0x00;
    private const byte IOAPIC_VER = 0x01;
    private const byte IOAPIC_ARB = 0x02;
    private const byte IOAPIC_REDTBL = 0x10;  // Redirection table starts here (2 registers per entry)

    // Redirection entry flags
    private const ulong MASKED = 1UL << 16;           // Interrupt masked
    private const ulong LEVEL_TRIGGERED = 1UL << 15;  // Level (vs Edge) triggered
    private const ulong ACTIVE_LOW = 1UL << 13;       // Active low (vs high)
    private const ulong LOGICAL_DEST = 1UL << 11;     // Logical (vs Physical) destination mode

    private static ulong s_baseAddress;
    private static uint s_gsiBase;
    private static byte s_maxRedirectionEntry;
    private static bool s_initialized;

    /// <summary>
    /// Gets whether the I/O APIC is initialized.
    /// </summary>
    public static bool IsInitialized => s_initialized;

    /// <summary>
    /// Initializes the I/O APIC with information from MADT.
    /// </summary>
    /// <param name="info">I/O APIC info from MADT.</param>
    public static void Initialize(IoApicInfo info)
    {
        s_baseAddress = info.Address;
        s_gsiBase = info.GsiBase;

        Serial.Write("[IOAPIC] Initializing at 0x", info.Address.ToString("X"), "\n");
        Serial.Write("[IOAPIC] GSI base: ", info.GsiBase, "\n");

        // Read I/O APIC ID and version
        uint id = Read(IOAPIC_ID);
        uint version = Read(IOAPIC_VER);

        s_maxRedirectionEntry = (byte)((version >> 16) & 0xFF);

        Serial.Write("[IOAPIC] ID: ", (id >> 24) & 0xF, "\n");
        Serial.Write("[IOAPIC] Version: ", version & 0xFF, "\n");
        Serial.Write("[IOAPIC] Max redirection entries: ", s_maxRedirectionEntry + 1, "\n");

        // Mask all interrupts initially
        for (int i = 0; i <= s_maxRedirectionEntry; i++)
        {
            SetRedirectionEntry((byte)i, 0, true);
        }

        s_initialized = true;
        Serial.Write("[IOAPIC] Initialization complete\n");
    }

    /// <summary>
    /// Routes an IRQ to a specific interrupt vector on a target APIC.
    /// </summary>
    /// <param name="irq">The ISA IRQ number (0-15).</param>
    /// <param name="vector">The interrupt vector (32-255).</param>
    /// <param name="targetApicId">The target Local APIC ID.</param>
    /// <param name="override">Optional IRQ override from MADT.</param>
    /// <param name="startMasked">If true, the IRQ starts masked and must be explicitly unmasked.</param>
    public static void RouteIrq(byte irq, byte vector, byte targetApicId, IrqOverride? @override = null, bool startMasked = false)
    {
        if (!s_initialized)
        {
            return;
        }

        uint gsi = irq;
        bool activeLow = false;
        bool levelTriggered = false;

        if (@override.HasValue)
        {
            gsi = @override.Value.Gsi;
            activeLow = @override.Value.IsActiveLow;
            levelTriggered = @override.Value.IsLevelTriggered;
        }

        uint redirIndex = gsi - s_gsiBase;
        if (redirIndex > s_maxRedirectionEntry)
        {
            return;
        }

        ulong entry = vector;
        if (activeLow)
        {
            entry |= ACTIVE_LOW;
        }

        if (levelTriggered)
        {
            entry |= LEVEL_TRIGGERED;
        }

        if (startMasked)
        {
            entry |= MASKED;
        }

        entry |= (ulong)targetApicId << 56;

        WriteRedirectionEntry((byte)redirIndex, entry);
    }

    /// <summary>
    /// Sets a redirection entry (masked or unmasked).
    /// </summary>
    private static void SetRedirectionEntry(byte index, byte vector, bool masked)
    {
        ulong entry = vector;
        if (masked)
        {
            entry |= MASKED;
        }

        WriteRedirectionEntry(index, entry);
    }

    /// <summary>
    /// Writes a 64-bit redirection table entry.
    /// </summary>
    private static void WriteRedirectionEntry(byte index, ulong entry)
    {
        byte regLow = (byte)(IOAPIC_REDTBL + index * 2);
        byte regHigh = (byte)(regLow + 1);

        Write(regLow, (uint)(entry & 0xFFFFFFFF));
        Write(regHigh, (uint)(entry >> 32));
    }

    /// <summary>
    /// Reads a 64-bit redirection table entry.
    /// </summary>
    private static ulong ReadRedirectionEntry(byte index)
    {
        byte regLow = (byte)(IOAPIC_REDTBL + index * 2);
        byte regHigh = (byte)(regLow + 1);

        uint low = Read(regLow);
        uint high = Read(regHigh);

        return low | ((ulong)high << 32);
    }

    /// <summary>
    /// Reads the redirection entry for an IRQ (public for debugging).
    /// </summary>
    public static ulong GetRedirectionEntry(byte irq)
    {
        if (!s_initialized)
        {
            return 0;
        }

        uint redirIndex = irq - s_gsiBase;
        if (redirIndex > s_maxRedirectionEntry)
        {
            return 0;
        }

        return ReadRedirectionEntry((byte)redirIndex);
    }

    /// <summary>
    /// Masks (disables) a global system interrupt at the I/O APIC level.
    /// The caller resolves an ISA IRQ to its GSI first (the MADT override)
    /// and holds interrupts disabled: the register pair below shares the
    /// IOREGSEL latch with every other caller.
    /// </summary>
    /// <param name="gsi">The global system interrupt.</param>
    public static void MaskIrq(uint gsi)
    {
        if (!s_initialized)
        {
            return;
        }

        uint redirIndex = gsi - s_gsiBase;
        if (redirIndex > s_maxRedirectionEntry)
        {
            return;
        }

        ulong entry = ReadRedirectionEntry((byte)redirIndex);
        entry |= MASKED;
        WriteRedirectionEntry((byte)redirIndex, entry);
    }

    /// <summary>
    /// Unmasks (enables) a global system interrupt at the I/O APIC level.
    /// Same rules as <see cref="MaskIrq"/>.
    /// </summary>
    /// <param name="gsi">The global system interrupt.</param>
    public static void UnmaskIrq(uint gsi)
    {
        if (!s_initialized)
        {
            return;
        }

        uint redirIndex = gsi - s_gsiBase;
        if (redirIndex > s_maxRedirectionEntry)
        {
            return;
        }

        ulong entry = ReadRedirectionEntry((byte)redirIndex);
        entry &= ~MASKED;
        WriteRedirectionEntry((byte)redirIndex, entry);
    }

    /// <summary>
    /// Reads from an I/O APIC register.
    /// </summary>
    private static uint Read(byte reg)
    {
        Native.MMIO.Write32(s_baseAddress + IOREGSEL, reg);
        return Native.MMIO.Read32(s_baseAddress + IOWIN);
    }

    /// <summary>
    /// Writes to an I/O APIC register.
    /// </summary>
    private static void Write(byte reg, uint value)
    {
        Native.MMIO.Write32(s_baseAddress + IOREGSEL, reg);
        Native.MMIO.Write32(s_baseAddress + IOWIN, value);
    }
}
