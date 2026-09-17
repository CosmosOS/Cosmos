// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Bridge;

internal static unsafe partial class EfiNative
{
    /// <summary>
    /// Calls a two-argument UEFI entry point with the EFIAPI calling convention.
    /// On x86-64 that is the Microsoft x64 convention, not the System V one
    /// NativeAOT emits for <c>delegate* unmanaged</c> on linux-x64, so the
    /// arguments go through a thunk (<c>CPU/EfiCall.s</c> in each native
    /// project; on AArch64 EFIAPI is AAPCS64 and the thunk is a pass-through).
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_efi_call")]
    public static partial ulong Call(void* function, void* arg1, void* arg2);
}
