using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

internal static partial class AcpiPmNative
{
    /// <summary><see cref="LoadNamespace"/>: the firmware's AML namespace is loaded and initialized.</summary>
    public const int NamespaceLoaded = 0;

    /// <summary><see cref="LoadNamespace"/>: uACPI failed to bring the namespace up.</summary>
    public const int NamespaceFailed = 1;

    /// <summary><see cref="LoadNamespace"/>: the architecture runs no AML interpreter (ARM64 powers off through PSCI).</summary>
    public const int NamespaceUnsupported = 2;

    /// <summary>
    /// Brings uACPI's interpreter up as far as <see cref="Shutdown"/> needs it,
    /// without entering a sleep state. Returns <see cref="NamespaceLoaded"/>,
    /// <see cref="NamespaceFailed"/> or <see cref="NamespaceUnsupported"/>.
    /// </summary>
    [LibraryImport("*", EntryPoint = "cosmos_acpi_load_namespace")]
    public static partial int LoadNamespace();

    [LibraryImport("*", EntryPoint = "cosmos_acpi_shutdown")]
    public static partial int Shutdown();

    [LibraryImport("*", EntryPoint = "cosmos_acpi_reset")]
    public static partial int Reset();
}
