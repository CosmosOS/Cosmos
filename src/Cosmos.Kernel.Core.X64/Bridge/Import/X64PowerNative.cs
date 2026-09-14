using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.X64.Bridge;

internal static partial class X64PowerNative
{
    [LibraryImport("*", EntryPoint = "_native_cpu_triple_fault")]
    [SuppressGCTransition]
    public static partial void TripleFault();
}
