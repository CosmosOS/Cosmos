using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.RISCV64.Bridge;

/// <summary>
/// The ecall into the SBI firmware (Cosmos.Kernel.Native.RISCV64, CPU/Sbi.s).
/// </summary>
internal static unsafe partial class SbiNative
{
    /// <summary>
    /// Calls SBI function <paramref name="function"/> of extension
    /// <paramref name="extension"/>. Returns the SBI error code (0 on
    /// success) and stores the value the call returns at
    /// <paramref name="value"/> when it is not null.
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_riscv64_sbi_call")]
    [SuppressGCTransition]
    public static partial long Call(ulong extension, ulong function, ulong arg0, ulong arg1, ulong arg2, ulong* value);
}
