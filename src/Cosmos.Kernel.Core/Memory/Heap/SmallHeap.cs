// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Debug;

namespace Cosmos.Kernel.Core.Memory.Heap;

/// <summary>
/// Page containing Size Map Table
/// </summary>
internal unsafe struct SMTPage
{
    /// <summary>
    /// Pointer to the next page
    /// </summary>
    public SMTPage* Next;

    /// <summary>
    /// Pointer to the root of the smallest block stored on this page
    /// </summary>
    public RootSMTBlock* First;
}

internal unsafe struct RootSMTBlock
{
    /// <summary>
    /// Elements stored in the page have a size less or equal to this
    /// Does not include the prefix bytes
    /// </summary>
    public uint Size;

    /// <summary>
    /// Pointer to the first block for this size
    /// </summary>
    public SMTBlock* First;

    /// <summary>
    /// Next larger size root block
    /// </summary>
    public RootSMTBlock* LargerSize;
}

// Changing the ordering will break SMTBlock* NextFreeBlock(SMTPage* aPage)
internal unsafe struct SMTBlock
{
    /// <summary>
    /// Pointer to the actual page, where the elements are stored
    /// </summary>
    public byte* PagePtr;

    /// <summary>
    /// How much space is left on the page, makes it easier to skip full pages
    /// </summary>
    public ulong SpacesLeft;

    /// <summary>
    /// Pointer to the next block for this size
    /// </summary>
    public SMTBlock* NextBlock;
}

internal static unsafe class SmallHeap
{
    public static ulong MaxSize => PageAllocator.PageSize / 2 - 1;

    /// <summary>
    /// Number of prefix bytes for each item.
    /// We technically only need 2 but to keep it aligned we have two padding.
    /// ARM64 requires 8-byte alignment for atomic operations, so we use 8 bytes there.
    /// </summary>
#if ARCH_ARM64
    public const ulong PrefixBytes = 8;  // 8-byte alignment required for ARM64 atomic ops (LDAR/STLR)
#else
    public const ulong PrefixBytes = 2 * sizeof(ushort);  // 4 bytes for x64
#endif

    /// <summary>
    /// Max item size in the heap.
    /// </summary>
    public static ulong mMaxItemSize;

    /// <summary>
    /// Size map table
    /// </summary>
    public static SMTPage* SMT;

    #region SMT

    /// <summary>
    /// Find the next free block in a page
    /// </summary>
    /// <returns>Pointer to the start of block in the SMT. null if all SMT pages are full</returns>
    private static SMTBlock* NextFreeBlock(SMTPage* aPage)
    {
        SMTBlock*
            ptr = (SMTBlock*)aPage->First; // since both RootSMTBlock and SMTBlock have the same size (20) it doesnt matter if cast is wrong
        while (ptr->PagePtr != null) // this would check Size if its actually a RootSMTBlock, which is always non-zero
        {
            ptr += 1;
            if (ptr >= (byte*)aPage + PageAllocator.PageSize - 8)
            {
                return null;
            }
        }

        return ptr;
    }

    public static SmallHeapHeader* GetHeader(byte* ptr) => (SmallHeapHeader*)(ptr - PrefixBytes);

    /// <summary>
    /// Gets the root block in the SMT for objects of this size
    /// </summary>
    /// <param name="aSize">Size of allocated block</param>
    /// <param name="aPage">Page to seach in</param>
    /// <returns>Pointer of block in SMT.</returns>
    private static RootSMTBlock* GetFirstBlock(SMTPage* aPage, uint aSize)
    {
        RootSMTBlock* ptr = aPage->First;
        uint curSize = ptr->Size;
        while (curSize < aSize)
        {
            ptr = ptr->LargerSize;
            if (ptr == null)
            {
                return null;
            }

            curSize = ptr->Size;
        }

        return ptr;
    }

    /// <summary>
    /// Gets the last block on a certain page for objects of this size
    /// </summary>
    /// <param name="page">Page to search</param>
    /// <param name="aSize">Object size in bytes the block chain is keyed on.</param>
    /// <returns>The last block in the chain for that size on that page.</returns>
    private static SMTBlock* GetLastBlock(SMTPage* page, uint aSize)
    {
        SMTBlock* ptr = GetFirstBlock(page, aSize)->First;
        if (ptr == null)
        {
            return null;
        }

        while (ptr->NextBlock != null)
        {
            ptr = ptr->NextBlock;
        }

        return ptr;
    }

    /// <summary>
    /// Get the first block for this size on any SMT page, which has space left to allocate to
    /// </summary>
    /// <param name="aSize">Object size in bytes the block chain is keyed on.</param>
    /// <returns>Null if no more space on any block of this size</returns>
    private static SMTBlock* GetFirstWithSpace(uint aSize)
    {
        SMTPage* page = SMT;
        SMTBlock* block = null;
        do
        {
            block = GetFirstWithSpace(page, aSize);
            page = page->Next;
        } while (block == null && page != null);

        return block;
    }

    /// <summary>
    /// Get the first block for this size on this SMT page, which has space left to allocate to
    /// </summary>
    /// <param name="aPage">Page to search.</param>
    /// <param name="aSize">Object size in bytes the block chain is keyed on.</param>
    /// <returns>Null if no more space on this page</returns>
    private static SMTBlock* GetFirstWithSpace(SMTPage* aPage, uint aSize) =>
        GetFirstWithSpace(GetFirstBlock(aPage, aSize), aSize);

    /// <summary>
    /// Get the first block for this size in this SMT block chain, which has space left to allocate to
    /// </summary>
    /// <param name="aRoot">The root node to start the search at</param>
    /// <param name="aSize">Object size in bytes the block chain is keyed on.</param>
    /// <returns>Null if no block in the chain has space left.</returns>
    private static SMTBlock* GetFirstWithSpace(RootSMTBlock* aRoot, uint aSize)
    {
        SMTBlock* ptr = aRoot->First;
        if (ptr == null) // Can this ever happen?
        {
            return null;
        }

        while (ptr->SpacesLeft == 0)
        {
            ptr = ptr->NextBlock;
            if (ptr == null)
            {
                return null;
            }
        }

        return ptr;
    }

    /// <summary>
    /// Add a new root block for a certain size to a certain SMT page
    /// </summary>
    /// <param name="aPage">Page to add the root block to.</param>
    /// <param name="aSize">Size must be divisible by 2 otherwise Alloc breaks</param>
    private static void AddRootSMTBlock(SMTPage* aPage, uint aSize)
    {
        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] AddRootSMTBlock - size: ");
        Cosmos.Kernel.Core.IO.Serial.WriteNumber(aSize);
        Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

        RootSMTBlock* ptr = aPage->First;
        while (ptr->LargerSize != null)
        {
            ptr = ptr->LargerSize;
        }

        if (aSize < ptr->Size)
        {
            // we cant later add a block with a size smaller than an earlier block. That would break the algorithm
            Debugger.DoSendNumber(aSize);
            Debugger.DoSendNumber(ptr->Size);
            Debugger.SendKernelPanic(Panics.SmallHeap.AddRootSmtBlockOrder);
        }

        if (ptr->Size == 0) // This is the first block to be allocated on the page
        {
            ptr->Size = aSize;
        }
        else
        {
            RootSMTBlock* block = (RootSMTBlock*)NextFreeBlock(aPage); // we should actually check that this is not null
            //but we should also only call this code right at the beginning so it should be fine
            block->Size = aSize;
            ptr->LargerSize = block;
        }

        CreatePage(aPage, aSize);
    }

    /// <summary>
    /// Get the Last Page of the SMT
    /// </summary>
    /// <returns></returns>
    private static SMTPage* GetSMTLastPage()
    {
        SMTPage* page = SMT;
        while (page->Next != null)
        {
            page = page->Next;
        }

        return page;
    }

    /// <summary>
    /// Return the size a certain element will be allocated as
    /// </summary>
    /// <returns></returns>
    public static uint GetRoundedSize(uint aSize) => GetFirstBlock(SMT, aSize)->Size;

    #endregion

    /// <summary>
    /// Init small heap.
    /// </summary>
    /// <exception cref="Exception">Thrown on fatal error, contact support.</exception>
    public static void Init()
    {
        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] Init started\n");

        // Later Adjust for new page and header sizes
        // 4 slots, ~1k ea
        ulong xMaxItemSize = PageAllocator.PageSize / 4 - PrefixBytes;
        // Word align it
        mMaxItemSize = xMaxItemSize / sizeof(uint) * sizeof(uint);

        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] Max item size: ");
        Cosmos.Kernel.Core.IO.Serial.WriteNumber(mMaxItemSize);
        Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

        SMT = InitSMTPage();

        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] Init complete, SMT at: 0x");
        Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)SMT);
        Cosmos.Kernel.Core.IO.Serial.WriteString("\n");
    }

    /// <summary>
    /// Allocates and initialise a page for the SMT table
    /// </summary>
    /// <returns></returns>
    private static SMTPage* InitSMTPage()
    {
        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] InitSMTPage - Allocating SMT page\n");
        SMTPage* page = (SMTPage*)PageAllocator.AllocPages(PageType.SMT, 1, zero: true);

        if (page == null)
        {
            Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] ERROR: Failed to allocate SMT page!\n");
            return null;
        }

        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] SMT page allocated at: 0x");
        Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)page);
        Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

        page->Next = null;
        page->First = (RootSMTBlock*)((byte*)page + sizeof(SMTPage));

        // Initialize the first RootSMTBlock
        RootSMTBlock* firstBlock = page->First;
        firstBlock->Size = 0;
        firstBlock->First = null;
        firstBlock->LargerSize = null;

        // Later Change these sizes after further study and also when page size changes.
        // SMT can be grown as needed. Also can adjust and create new ones dynamicaly as it runs.
        // The current algorithm only works if we create the inital pages in increasing order
        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] Adding root SMT blocks\n");
        AddRootSMTBlock(page, 16);
        AddRootSMTBlock(page, 24);
        AddRootSMTBlock(page, 48);
        AddRootSMTBlock(page, 64);
        AddRootSMTBlock(page, 128);
        AddRootSMTBlock(page, 256);
        AddRootSMTBlock(page, 512);
        AddRootSMTBlock(page, 1024);
        AddRootSMTBlock(page, 2048);
        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] InitSMTPage complete\n");
        return page;
    }

    /// <summary>
    /// Create a page with the size of an item and try add it to the SMT at a certain page
    /// If the SMT page is full, it will be added to the first SMT page with space or a new SMT page is allocated
    /// </summary>
    /// <param name="aPage">SMT page the new page is added to, or the first
    /// page with space when it is full.</param>
    /// <param name="aItemSize">Object size in bytes</param>
    /// <exception cref="Exception">Thrown if:
    /// <list type="bullet">
    /// <item>aItemSize is 0.</item>
    /// <item>aItemSize is not word aligned.</item>
    /// <item>SMT is not initialized.</item>
    /// <item>The item size is bigger then a small heap size.</item>
    /// </list>
    /// </exception>
    private static void CreatePage(SMTPage* aPage, uint aItemSize)
    {
        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] CreatePage - itemSize: ");
        Cosmos.Kernel.Core.IO.Serial.WriteNumber(aItemSize);
        Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

        byte* xPtr = (byte*)PageAllocator.AllocPages(PageType.HeapSmall, 1, zero: true);
        if (xPtr == null)
        {
            Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] ERROR: Failed to allocate HeapSmall page!\n");
            return; // we failed to create the page, Alloc should still handle this case
        }

        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] HeapSmall page allocated at: 0x");
        Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)xPtr);
        Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

        ulong xSlotSize = aItemSize + PrefixBytes;
        ulong xItemCount = PageAllocator.PageSize / xSlotSize;
        for (uint i = 0; i < xItemCount; i++)
        {
            byte* xSlotPtr = xPtr + i * xSlotSize;
            ushort* xMetaDataPtr = (ushort*)xSlotPtr;
            xMetaDataPtr[0] = 0; // Actual data size. 0 is empty.
            xMetaDataPtr[1] = 0; // Ref count
        }

        //now add it to the smt
        SMTBlock* parent = GetLastBlock(aPage, aItemSize);
        SMTBlock* smtBlock = NextFreeBlock(aPage); //get the next free block in the smt

        if (smtBlock == null) // we could not allocate a new block since the SMT table is all full on this page
        {
            // we now have two options:
            // 1. there exists a later page in the chain, which has space
            // 2. all SMT Pages are full and we need to allocate a new one

            // first, check if we find a later page with space
            SMTPage* currentSMTPage = aPage->Next;
            while (currentSMTPage != null)
            {
                smtBlock = NextFreeBlock(currentSMTPage);
                if (smtBlock != null)
                {
                    break;
                }

                currentSMTPage = currentSMTPage->Next;
            }

            if (smtBlock == null)
            {
                // we need to expand the SMT table by a page
                SMTPage* last = GetSMTLastPage();
                last->Next = InitSMTPage();
                aPage = last->Next;
                parent = GetLastBlock(aPage, aItemSize);
                smtBlock = NextFreeBlock(aPage);

                if (smtBlock == null)
                {
                    Debugger.SendKernelPanic(Panics.SmallHeap.AddPage);
                }
            }
            else
            {
                aPage = currentSMTPage;
                parent = GetLastBlock(aPage, aItemSize);
                // we have already found the smt block above
            }
        }

        if (parent != null)
        {
            // there is already a block for the same size on the same page
            parent->NextBlock = smtBlock;
        }
        else
        {
            // in this case this is the first block of the size, so we can link it to root
            RootSMTBlock* root = GetFirstBlock(aPage, aItemSize);
            root->First = smtBlock;
        }

        smtBlock->SpacesLeft = xItemCount;
        smtBlock->PagePtr = xPtr;
    }

    /// <summary>
    /// Alloc memory block, of a given size.
    /// </summary>
    /// <param name="aSize">A size of block to alloc, in bytes.</param>
    /// <returns>Byte pointer to the start of the block.</returns>
    public static byte* Alloc(uint aSize)
    {
        // Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] Alloc - size: ");
        // Cosmos.Kernel.Core.IO.Serial.WriteNumber(aSize);
        // Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

        SMTBlock* pageBlock = GetFirstWithSpace(aSize);
        if (pageBlock == null) // This happens when the page is full and we need to allocate a new page for this size
        {
            // Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] No space found, creating new page\n");
            CreatePage(SMT,
                GetRoundedSize(
                    aSize)); // CreatePage will try add this page to any page of the SMT until it finds one with space
            pageBlock = GetFirstWithSpace(aSize);
            if (pageBlock == null)
            {
                //this means that we cant allocate another page
                InternalCpu.EnableInterrupts();
                Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] ERROR: Failed to allocate new page!\n");
                Debugger.SendKernelPanic(Panics.SmallHeap.AddPage);
            }
        }

        //now find position in the block
        ushort* page = (ushort*)pageBlock->PagePtr;
        uint roundedSize = GetRoundedSize(aSize);
        ulong elementSize = roundedSize + PrefixBytes;
        ulong positions = PageAllocator.PageSize / elementSize;

        // Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] PagePtr: 0x");
        // Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)pageBlock->PagePtr);
        // Cosmos.Kernel.Core.IO.Serial.WriteString(", RoundedSize: ");
        // Cosmos.Kernel.Core.IO.Serial.WriteNumber(roundedSize);
        // Cosmos.Kernel.Core.IO.Serial.WriteString(", ElementSize: ");
        // Cosmos.Kernel.Core.IO.Serial.WriteNumber(elementSize);
        // Cosmos.Kernel.Core.IO.Serial.WriteString(", PageSize: ");
        // Cosmos.Kernel.Core.IO.Serial.WriteNumber(PageAllocator.PageSize);
        // Cosmos.Kernel.Core.IO.Serial.WriteString(", Positions: ");
        // Cosmos.Kernel.Core.IO.Serial.WriteNumber(positions);
        // Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

        for (ulong i = 0; i < positions; i++)
        {
            if (page[i * elementSize / 2] == 0)
            {
                // we have found an empty slot
                // Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] Found free slot at position ");
                // Cosmos.Kernel.Core.IO.Serial.WriteNumber(i);
                // Cosmos.Kernel.Core.IO.Serial.WriteString(", allocating at 0x");

                // update SMT block info
                pageBlock->SpacesLeft--;

                // set info in page
                byte* slotPtr = (byte*)&page[i * elementSize / 2];
                ushort* heapObject = (ushort*)slotPtr;
                heapObject[0] = (ushort)aSize; // size of actual object being allocated

                // Return pointer after prefix bytes (8-byte aligned on ARM64, 4-byte on x64)
                byte* result = slotPtr + PrefixBytes;
                // Cosmos.Kernel.Core.IO.Serial.WriteHex((ulong)result);
                // Cosmos.Kernel.Core.IO.Serial.WriteString("\n");

                return result;
            }
        }

        // if we get here, RAM is corrupted, since we know we had a space but it turns out we didnt
        Cosmos.Kernel.Core.IO.Serial.WriteString("[SmallHeap] ERROR: RAM corrupted - no free slot found!\n");
        Debugger.DoSendNumber((uint)pageBlock);
        Debugger.DoSendNumber(aSize);
        Debugger.SendKernelPanic(Panics.SmallHeap.RamCorrupted);
        while (true)
        {
        }
    }

    /// <summary>
    /// Free a object
    /// </summary>
    /// <param name="aPtr">A pointer to the start object.</param>
    public static void Free(void* aPtr)
    {
        // Get header at PrefixBytes offset before the allocation
        byte* slotPtr = (byte*)aPtr - PrefixBytes;
        ushort* heapObject = (ushort*)slotPtr;
        ushort size = heapObject[0];
        if (size == 0)
        {
            // double free, this object has already been freed
            InternalCpu.EnableInterrupts();
            Debugger.DoBochsBreak();
            Debugger.DoSendNumber((uint)aPtr);
            Debugger.SendKernelPanic(Panics.SmallHeap.DoubleFree);
        }

        // Zero the header (size)
        heapObject[0] = 0;

        // now zero the object so its ready for next allocation
        if (size < 4) // so we dont actually forget to clean up too small items
        {
            size = 4;
        }

        int bytes = size / 4;
        if (size % 4 != 0)
        {
            bytes += 1;
        }

        uint* allocated = (uint*)aPtr;
        for (int i = 0; i < bytes; i++)
        {
            allocated[i] = 0;
        }

        // need to increase count in SMT again
        // Later: store this info somewhere so this can be done in constant time
        byte* allocatedOnPage = PageAllocator.GetPagePtr(aPtr);
        SMTPage* smtPage = SMT;
        SMTBlock* blockPtr = null;
        while (smtPage != null)
        {
            blockPtr = GetFirstBlock(smtPage, size)->First;
            while (blockPtr != null)
            {
                if (blockPtr->PagePtr == allocatedOnPage)
                {
                    blockPtr->SpacesLeft++;
                    InternalCpu.EnableInterrupts();
                    return;
                }

                blockPtr = blockPtr->NextBlock;
            }

            smtPage = smtPage->Next;
        }

        // this shouldnt happen
        InternalCpu.EnableInterrupts();
        Debugger.DoSendNumber((uint)aPtr);
        Debugger.DoSendNumber((uint)SMT);
        Debugger.SendKernelPanic(Panics.SmallHeap.FailedFree);
    }

    #region Statistics

    /// <summary>
    /// Counts how many elements are currently allocated
    /// </summary>
    public static int GetAllocatedObjectCount()
    {
        SMTPage* ptr = SMT;
        int count = 0;
        do
        {
            count += GetAllocatedObjectCount(ptr);
            ptr = ptr->Next;
        } while (ptr != null);

        return count;
    }

    /// <summary>
    /// Counts how many elements are currently allocated on a certain page
    /// </summary>
    /// <param name="aPage">Page to count on.</param>
    /// <returns>The number of allocated objects across every block chain on the page.</returns>
    private static int GetAllocatedObjectCount(SMTPage* aPage)
    {
        RootSMTBlock* ptr = aPage->First;
        int count = 0;
        while (ptr != null)
        {
            count += GetAllocatedObjectCount(aPage, ptr->Size);
            ptr = ptr->LargerSize;
        }

        return count;
    }

    /// <summary>
    /// Counts how many elements are currently allocated in this category on a certain page
    /// </summary>
    /// <param name="aPage"></param>
    /// <param name="aSize"></param>
    /// <returns></returns>
    private static int GetAllocatedObjectCount(SMTPage* aPage, uint aSize)
    {
        RootSMTBlock* root = GetFirstBlock(aPage, aSize);
        SMTBlock* ptr = root->First;

        uint size = root->Size;
        int count = 0;

        while (ptr != null)
        {
            count += (int)(PageAllocator.PageSize / (size + PrefixBytes)) - (int)ptr->SpacesLeft;
            ptr = ptr->NextBlock;
        }

        return count;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// This function will free all pages allocated for small objects which are emnpty
    /// </summary>
    /// <returns>Number of pages freed</returns>
    public static int PruneSMT()
    {
        int freed = 0;
        SMTPage* page = SMT;
        while (page != null)
        {
            freed += PruneSMT(page);
            page = page->Next;
        }

        return freed;
    }

    /// <summary>
    /// Prune all empty pages allocated on a certain page
    /// </summary>
    /// <param name="aPage"></param>
    /// <returns></returns>
    private static int PruneSMT(SMTPage* aPage)
    {
        int freed = 0;
        RootSMTBlock*
            ptr = (RootSMTBlock*)aPage->First; // since both RootSMTBlock and SMTBlock have the same size (20) it doesnt matter if cast is wrong
        while (ptr != null)
        {
            freed += PruneSMT(ptr, ptr->Size);
            ptr = ptr->LargerSize;
        }

        return freed;
    }

    /// <summary>
    /// Prune all empty pages which are linked to root block for a certain size
    /// The root block or first one following it will not be removed!
    /// </summary>
    /// <param name="aBlock"></param>
    /// <param name="aSize"></param>
    /// <returns></returns>
    private static int PruneSMT(RootSMTBlock* aBlock, uint aSize)
    {
        int freed = 0;
        ulong maxElements = PageAllocator.PageSize / (aSize + PrefixBytes);
        SMTBlock* prev = aBlock->First;
        SMTBlock* block = prev->NextBlock;
        while (block != null)
        {
            if (block->SpacesLeft == maxElements)
            {
                // This block is currently empty so free it
                prev->NextBlock = block->NextBlock;
                PageAllocator.Free(block->PagePtr);

                uint* toCleanUp = (uint*)block;
                block = prev->NextBlock;

                toCleanUp[0] = 0;
                toCleanUp[1] = 0;
                toCleanUp[2] = 0;

                freed++;
            }
            else
            {
                prev = block;
                block = block->NextBlock;
            }
        }

        return freed;
    }

    #endregion
}
