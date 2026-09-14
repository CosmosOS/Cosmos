using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.Core.Bridge;

/// <summary>
/// Bridge functions that let C library code allocate from the Cosmos heap.
/// </summary>
internal static unsafe class HeapNative
{
    /// <summary>
    /// Allocate memory from Cosmos heap
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_heap_alloc")]
    public static void* Alloc(nuint size)
    {
        return MemoryOp.Alloc((uint)size);
    }

    /// <summary>
    /// Free memory from Cosmos heap
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "__cosmos_heap_free")]
    public static void Free(void* ptr)
    {
        MemoryOp.Free(ptr);
    }
}
