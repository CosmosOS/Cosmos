using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Bridge functions for memcpy / memcmp used by C library code.
/// </summary>
internal static unsafe class MemNative
{
    /// <summary>
    /// Copy memory using Cosmos MemoryOp.MemCopy
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_memcpy")]
    public static void* MemCopy(void* dest, void* src, nuint count)
    {
        MemoryOp.MemCopy((byte*)dest, (byte*)src, (int)count);
        return dest;
    }

    /// <summary>
    /// Compare memory using Cosmos MemoryOp.MemCmp
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_memcmp")]
    public static int MemCmp(void* s1, void* s2, nuint count)
    {
        return MemoryOp.MemCmp((byte*)s1, (byte*)s2, (int)count);
    }
}
