using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Debug;

namespace Cosmos.Kernel.Core.Memory.Heap;

/// <summary>
/// a basic Heap that uses PageAllocator
/// </summary>
internal static unsafe class Heap
{
    /// <summary>
    /// Re-allocates or "re-sizes" data asigned to a pointer.
    /// The pointer specified must be the start of an allocated block in the heap.
    /// This shouldn't be used with objects as a new address is given when realocating memory.
    /// </summary>
    /// <param name="aPtr">Existing pointer</param>
    /// <param name="newSize">Size to extend to</param>
    /// <returns>New pointer with specified size while maintaining old data.</returns>
    public static byte* Realloc(byte* aPtr, uint newSize)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            PageType currentType = PageAllocator.GetPageType(aPtr);

#if COSMOSDEBUG
            if (currentType != PageType.HeapSmall && currentType != PageType.HeapMedium &&
                currentType != PageType.HeapLarge)
            {
                Debugger.DoSendNumber(newSize);
                Debugger.DoSendNumber((uint)aPtr);
                Debugger.SendKernelPanic(Panics.NonManagedPage);
            }
#endif

            byte* result;

            if (newSize > MediumHeap.MinSize && newSize <= MediumHeap.MaxSize)
            {
                if (currentType == PageType.HeapMedium)
                {
                    result = MediumHeap.Realloc(aPtr, newSize);
                    return result;
                }

                int oldSize = 0;
                if (currentType == PageType.HeapLarge)
                {
                    LargeHeapHeader* header = LargeHeap.GetHeader(aPtr);
                    oldSize = (int)header->Size;
                }
                else if (currentType == PageType.HeapSmall)
                {
                    SmallHeapHeader* header = SmallHeap.GetHeader(aPtr);
                    oldSize = (int)header->Size;
                }
                else
                {
                    throw new Exception();
                }

                byte* newPtr = MediumHeap.Alloc(newSize);
                // Copy the smaller of oldSize and newSize to avoid buffer overread
                int copySize = oldSize < (int)newSize ? oldSize : (int)newSize;
                MemoryOp.MemCopy(newPtr, aPtr, copySize);

                // Free the old allocation
                if (currentType == PageType.HeapLarge)
                {
                    LargeHeap.Free(aPtr);
                }
                else if (currentType == PageType.HeapSmall)
                {
                    SmallHeap.Free(aPtr);
                }

                return newPtr;
            }

            if (newSize >= LargeHeap.MinSize)
            {
                result = LargeHeap.Alloc(newSize);
                return result;
            }
        }

        return (byte*)0;
    }

    /// <summary>
    /// Alloc memory block, of a given size.
    /// </summary>
    /// <param name="aSize">A size of block to alloc, in bytes.</param>
    /// <returns>Byte pointer to the start of the block.</returns>
    public static byte* Alloc(uint aSize)
    {
        byte* result;

        using (InternalCpu.DisableInterruptsScope())
        {
            if (aSize > MediumHeap.MinSize && aSize <= MediumHeap.MaxSize)
            {
                result = MediumHeap.Alloc(aSize);
            }
            else if (aSize > LargeHeap.MinSize)
            {
                result = LargeHeap.Alloc(aSize);
            }
            else
            {
                result = SmallHeap.Alloc(aSize);
            }
        }

        return result;
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
        using (InternalCpu.DisableInterruptsScope())
        {
            PageType currentType = PageAllocator.GetPageType(aPtr);

            switch (currentType)
            {
                case PageType.HeapLarge:
                    LargeHeap.Free(aPtr);
                    break;
                case PageType.HeapMedium:
                    MediumHeap.Free(aPtr);
                    break;
                case PageType.HeapSmall:
                    SmallHeap.Free(aPtr);
                    break;
                default:
                    Debugger.SendKernelPanic(Panics.NonManagedPage);
                    throw new NotSupportedException("This is not a managed page");
            }
        }
    }

    /// <summary>
    /// Collects all unreferenced objects after identifying them first.
    /// Uses the mark-and-sweep garbage collector to identify unreachable objects.
    /// </summary>
    /// <returns>Number of objects freed</returns>
    public static int Collect()
    {
        int result;

        using (InternalCpu.DisableInterruptsScope())
        {
            // Run GC to identify and free unreachable objects
            result = GarbageCollector.GarbageCollector.Collect();

            // Also prune empty SMT pages
            result += SmallHeap.PruneSMT();
        }

        return result;
    }
}
