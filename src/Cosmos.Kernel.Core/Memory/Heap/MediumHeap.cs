// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Core.Memory.Heap;

/// <summary>
/// will mostly fill a whole page
/// </summary>
internal static unsafe class MediumHeap
{
    public static ulong PrefixBytes => (ulong)sizeof(MediumHeapHeader);

    public static ulong MaxSize => PageAllocator.PageSize - PrefixBytes;
    public static ulong MinSize => PageAllocator.PageSize / 2;

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
        MediumHeapHeader* header = (MediumHeapHeader*)(ptr - PrefixBytes);
        // Note: MediumHeap doesn't have a 'Used' field, just 'Size'
        // The 'Size' represents both allocated and usable space
        // So we always need to allocate new memory if size changes
        if (header->Size >= newSize)
        {
            // Current allocation is already large enough
            return ptr;
        }
        else
        {
            byte* newPtr = Alloc(newSize);
            // Copy the smaller of old size or new size to avoid buffer overread
            int copySize = header->Size < (int)newSize ? header->Size : (int)newSize;
            MemoryOp.MemCopy(newPtr, ptr, copySize);
            Free(ptr);
            return newPtr;
        }
    }

    public static MediumHeapHeader* GetHeader(byte* ptr) => (MediumHeapHeader*)(ptr - PrefixBytes);

    /// <summary>
    /// Alloc memory block, of a given size.
    /// </summary>
    /// <param name="aSize">A size of block to alloc, in bytes.</param>
    /// <returns>Byte pointer to the start of the block.</returns>
    public static byte* Alloc(uint aSize)
    {
        ulong pages = (aSize + PrefixBytes) / PageAllocator.PageSize + 1;
        void* ptr = PageAllocator.AllocPages(PageType.HeapMedium, pages, true);
        MediumHeapHeader* header = (MediumHeapHeader*)ptr;
        header->Size = (ushort)aSize;
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
