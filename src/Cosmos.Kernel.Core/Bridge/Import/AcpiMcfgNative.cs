using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Architecture-neutral ACPI MCFG import (PCI ECAM discovery).
/// MADT lives in Cosmos.Kernel.Core.X64/Bridge/Import/AcpiMadtNative.cs;
/// GIC info lives in Cosmos.Kernel.Core.ARM64/Bridge/Import/Gic/AcpiGicNative.cs.
/// </summary>
internal static unsafe partial class AcpiMcfgNative
{
    [LibraryImport("*", EntryPoint = "acpi_get_mcfg_info")]
    [SuppressGCTransition]
    public static partial void* GetMcfgInfo();
}
