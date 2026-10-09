// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Memory.Heap;

/// <summary>
/// will use more then 1 page
/// </summary>
internal static unsafe class LargeHeap
{
    public static ulong PrefixBytes => (ulong)sizeof(LargeHeapHeader);

    public static ulong MinSize => MediumHeap.MaxSize + 1;

    /// <summary>
    /// Re-allocates or "re-sizes" data asigned to a pointer.
    /// The pointer specified must be the start of an allocated block in the heap.
    /// This shouldn't be used with objects as a new address is given when realocating memory.
    /// </summary>
    /// <param name="ptr">Existing pointer</param>
    /// <param name="newSize">Size to extend to</param>
    /// <returns>New pointer with specified size while maintaining old data.</returns>
    public static byte* Realloc(byte* ptr, uint newSize)
    {
        LargeHeapHeader* header = (LargeHeapHeader*)(ptr - PrefixBytes);
        if (header->Used > newSize)
        {
            header->Size = newSize; // there is space
        }
        else
        {
            byte* newPtr = Alloc(newSize);
            MemoryOp.MemCopy(newPtr, ptr, (int)header->Used);
            // {!} Span
            // Span<byte> span = new(ptr, (int)header->Size);
            // span.CopyTo(new Span<byte>(newPtr, (int)newSize));
            Free(ptr);
            return newPtr;
        }

        return ptr;
    }

    public static LargeHeapHeader* GetHeader(byte* ptr) => (LargeHeapHeader*)(ptr - PrefixBytes);

    /// <summary>
    /// Alloc memory block, of a given size.
    /// </summary>
    /// <param name="aSize">A size of block to alloc, in bytes.</param>
    /// <returns>Byte pointer to the start of the block.</returns>
    public static byte* Alloc(uint aSize)
    {
        ulong pages = (aSize + PrefixBytes) / PageAllocator.PageSize + 1;
        void* ptr = PageAllocator.AllocPages(PageType.HeapLarge, pages, true);
        if (ptr == null)
        {
            return null;
        }
        LargeHeapHeader* header = (LargeHeapHeader*)ptr;
        header->Used = pages * PageAllocator.PageSize - PrefixBytes;
        header->Size = (uint)aSize;
        return (byte*)ptr + PrefixBytes;
    }

    // Keep as void* and not byte* or other. Reduces typecasting from callers
    // who may have typed the pointer to their own needs.
    /// <summary>
    /// Free a heap item.
    /// </summary>
    /// <param name="aPtr">A pointer to the heap item to be freed.</param>
    /// <exception cref="Exception">Thrown if:
    /// <list type="bullet">
    /// <item>Page type is not found.</item>
    /// <item>Heap item not found in RAT.</item>
    /// </list>
    /// </exception>
    public static void Free(void* aPtr)
    {
        uint xPageIdx = PageAllocator.GetFirstPageAllocatorIndex(aPtr);
        PageAllocator.Free(xPageIdx);
    }

    /// <summary>
    /// Collects all unreferenced objects after identifying them first
    /// </summary>
    /// <returns>Number of objects freed</returns>
    public static int Collect() => 0;
}
