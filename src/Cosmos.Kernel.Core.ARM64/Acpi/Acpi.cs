// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.ARM64.Bridge;

namespace Cosmos.Kernel.Core.ARM64;

/// <summary>
/// C# bridge to native ACPI GIC discovery (acpi_wrapper.c in MultiArch).
/// The native code is called during early boot (kmain) and parses the MADT
/// to extract GICD/GICR/GICC addresses. This class just retrieves the result.
/// Native import lives in Cosmos.Kernel.Core.ARM64/Bridge/Import/Gic/AcpiGicNative.cs.
/// </summary>
public static unsafe class AcpiGic
{
    /// <summary>
    /// Mirrors the C struct acpi_gic_info_t from ACPI/acpi_wrapper.c.
    /// Must match the native layout exactly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GicInfo
    {
        public byte Found;
        public byte Version;       // GIC version: 2 or 3
        public byte ItsFound;      // 1 if at least one GIC ITS subtable seen
        private fixed byte _pad[5];
        public ulong DistBase;     // GICD physical base
        public ulong RedistBase;   // GICR physical base (GICv3)
        public ulong RedistLength; // GICR region length
        public ulong CpuIfBase;    // GICC physical base (GICv2)
        public ulong ItsBase;      // GIC ITS physical base; 0 if absent
        public uint ItsId;         // ITS Translation ID (referenced by IORT)
        private uint _pad2;
    }

    /// <summary>
    /// Gets GIC information discovered from ACPI MADT during early boot.
    /// Returns null if ACPI was not available or GIC entries weren't found.
    /// </summary>
    public static GicInfo* GetGicInfo()
    {
        return (GicInfo*)AcpiGicNative.GetGicInfo();
    }
}
