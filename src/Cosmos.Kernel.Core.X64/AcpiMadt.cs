// ACPI subsystem for Cosmos OS
// C# interop layer for LAI (Lightweight ACPI Implementation)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.X64.Bridge;

namespace Cosmos.Kernel.Core.X64;

/// <summary>
/// CPU information from MADT
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct CpuInfo
{
    public byte ProcessorId;
    public byte ApicId;
    public uint Flags;

    public readonly bool IsEnabled => (Flags & 1) != 0;
}

/// <summary>
/// I/O APIC information from MADT
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct IoApicInfo
{
    public byte Id;
    public uint Address;
    public uint GsiBase;
}

/// <summary>
/// Interrupt Source Override from MADT
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct IrqOverride
{
    public byte Source;      // ISA IRQ number
    public uint Gsi;         // Global System Interrupt
    public ushort Flags;

    public readonly bool IsActiveLow => (Flags & 0x02) != 0;
    public readonly bool IsLevelTriggered => (Flags & 0x08) != 0;
}

/// <summary>
/// Complete MADT information
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct MadtInfo
{
    private const int MaxCpus = 256;
    private const int MaxIoApics = 16;
    private const int MaxIsos = 32;

    public uint LocalApicAddress;
    public uint Flags;

    public uint CpuCount;
    private fixed byte _cpusData[MaxCpus * 8];

    public uint IoApicCount;
    private fixed byte _ioapicsData[MaxIoApics * 12];

    public uint IsoCount;
    private fixed byte _isosData[MaxIsos * 8];

    public readonly bool HasPic8259 => (Flags & 1) != 0;

    public readonly ReadOnlySpan<CpuInfo> Cpus
    {
        get
        {
            fixed (byte* ptr = _cpusData)
            {
                return new ReadOnlySpan<CpuInfo>(ptr, (int)CpuCount);
            }
        }
    }

    public readonly ReadOnlySpan<IoApicInfo> IoApics
    {
        get
        {
            fixed (byte* ptr = _ioapicsData)
            {
                return new ReadOnlySpan<IoApicInfo>(ptr, (int)IoApicCount);
            }
        }
    }

    public readonly ReadOnlySpan<IrqOverride> Overrides
    {
        get
        {
            fixed (byte* ptr = _isosData)
            {
                return new ReadOnlySpan<IrqOverride>(ptr, (int)IsoCount);
            }
        }
    }
}

/// <summary>
/// ACPI subsystem interface.
/// Native import lives in Cosmos.Kernel.Core.X64/Bridge/Import/AcpiMadtNative.cs.
/// </summary>
public static unsafe class AcpiMadt
{
    /// <summary>
    /// Get MADT (Multiple APIC Description Table) information
    /// NOTE: ACPI is initialized early in C code, this just retrieves the data
    /// </summary>
    /// <returns>Pointer to MADT info if found, null otherwise</returns>
    public static MadtInfo* GetMadtInfoPtr()
    {
        return (MadtInfo*)AcpiMadtNative.GetMadtInfo();
    }

    /// <summary>
    /// Get and display MADT information
    /// </summary>
    /// <returns>True if MADT was found and parsed successfully during early boot</returns>
    public static bool DisplayMadtInfo()
    {
        MadtInfo* ptr = (MadtInfo*)AcpiMadtNative.GetMadtInfo();

        if (ptr == null)
        {
            Serial.Write("[ACPI] Error: MADT not available (ACPI early init may have failed)\n");
            return false;
        }

        MadtInfo info = *ptr;

        // Log discovered information
        Serial.Write("[ACPI] Local APIC at 0x", info.LocalApicAddress.ToString("X8"), "\n");
        Serial.Write("[ACPI] Found ", info.CpuCount, " CPU(s)\n");

        foreach (var cpu in info.Cpus)
        {
            Serial.Write("[ACPI]   CPU ", cpu.ProcessorId, " -> APIC ID ", cpu.ApicId,
                        (cpu.IsEnabled ? " (enabled)" : " (disabled)"), "\n");
        }

        Serial.Write("[ACPI] Found ", info.IoApicCount, " I/O APIC(s)\n");

        foreach (var ioapic in info.IoApics)
        {
            Serial.Write("[ACPI]   I/O APIC ", ioapic.Id, " at 0x", ioapic.Address.ToString("X8"),
                        " (GSI base: ", ioapic.GsiBase, ")\n");
        }

        Serial.Write("[ACPI] Found ", info.IsoCount, " IRQ override(s)\n");

        foreach (var iso in info.Overrides)
        {
            Serial.Write("[ACPI]   IRQ ", iso.Source, " -> GSI ", iso.Gsi);

            if (iso.IsActiveLow)
            {
                Serial.Write(" (active low)");
            }

            if (iso.IsLevelTriggered)
            {
                Serial.Write(" (level triggered)");
            }

            Serial.Write("\n");
        }

        return true;
    }

}
