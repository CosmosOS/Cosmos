using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Bridge;

internal static partial class X64CpuNative
{
    [LibraryImport("*", EntryPoint = "_native_cpu_rdtsc")]
    [SuppressGCTransition]
    public static partial ulong ReadTsc();

    [LibraryImport("*", EntryPoint = "_native_cpu_read_cr3")]
    [SuppressGCTransition]
    public static partial ulong ReadCr3();

    [LibraryImport("*", EntryPoint = "_native_cpu_invlpg")]
    [SuppressGCTransition]
    public static partial void InvalidatePage(ulong virtualAddress);
}
