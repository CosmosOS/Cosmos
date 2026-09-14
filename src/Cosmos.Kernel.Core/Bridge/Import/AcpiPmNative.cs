using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

internal static partial class AcpiPmNative
{
    [LibraryImport("*", EntryPoint = "cosmos_acpi_shutdown")]
    public static partial int Shutdown();

    [LibraryImport("*", EntryPoint = "cosmos_acpi_reset")]
    public static partial int Reset();
}
