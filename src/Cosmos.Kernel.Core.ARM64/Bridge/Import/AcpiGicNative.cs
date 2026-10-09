using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

/// <summary>
/// ARM64-specific ACPI GIC discovery import (GICD / GICR / GICC addresses).
/// </summary>
internal static unsafe partial class AcpiGicNative
{
    [LibraryImport("*", EntryPoint = "acpi_get_gic_info")]
    [SuppressGCTransition]
    public static partial void* GetGicInfo();
}
