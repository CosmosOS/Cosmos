using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Bridge;

internal static unsafe partial class GdtNative
{
    /// <summary>
    /// Loads the kernel GDT (5 entries: null, ring-0 code/data,
    /// ring-3 code/data) and reloads CS=0x08 / SS=0x10. Implemented in
    /// src/Cosmos.Kernel.Native.X64/CPU/Gdt.s. Interrupts are disabled on
    /// entry; the caller re-enables them once the IDT has been (re)loaded.
    /// </summary>
    [LibraryImport("*", EntryPoint = "_native_x64_load_gdt")]
    [SuppressGCTransition]
    public static partial void LoadGdt();
}