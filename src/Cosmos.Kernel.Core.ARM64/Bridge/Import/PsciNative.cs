using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.ARM64.Bridge;

internal static partial class PsciNative
{
    [LibraryImport("*", EntryPoint = "_native_psci_system_off")]
    [SuppressGCTransition]
    public static partial void SystemOff();

    [LibraryImport("*", EntryPoint = "_native_psci_system_reset")]
    [SuppressGCTransition]
    public static partial void SystemReset();
}
