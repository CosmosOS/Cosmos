using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Bridge;

/// <summary>
/// x64-specific ACPI MADT import (APIC / I/O APIC / ISO discovery).
/// </summary>
internal static unsafe partial class AcpiMadtNative
{
    [LibraryImport("*", EntryPoint = "acpi_get_madt_info")]
    [SuppressGCTransition]
    public static partial void* GetMadtInfo();
}
