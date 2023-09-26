using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core.Memory
{
    /// <summary>
    /// Page containing Size Map Table
    /// </summary>
    public unsafe struct SMTPage
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

    public unsafe struct RootSMTBlock
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
    public unsafe struct SMTBlock
    {
        /// <summary>
        /// Pointer to the actual page, where the elements are stored
        /// </summary>
        public byte* PagePtr;
        /// <summary>
        /// How much space is left on the page, makes it easier to skip full pages
        /// </summary>
        public uint SpacesLeft;
        /// <summary>
        /// Pointer to the next block for this size
        /// </summary>
        public SMTBlock* NextBlock;
    }

    //TODO Remove empty pages as necessary
    /// <summary>
    /// HeapSmall class. Used to alloc and free small memory blocks on the heap.
    /// </summary>
    unsafe static public class HeapSmall
    {
        /// <summary>
        /// Number of prefix bytes for each item.
        /// We technically only need 2 but to keep it aligned we have two padding
        /// </summary>
        public const uint PrefixItemBytes = 2 * sizeof(ushort);
        /// <summary>
        /// Max item size in the heap.
        /// </summary>
        public static uint mMaxItemSize;

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
            SMTBlock* ptr = (SMTBlock*)aPage->First; // since both RootSMTBlock and SMTBlock have the same size (20) it doesnt matter if cast is wrong
            while (ptr->PagePtr != null) // this would check Size if its actually a RootSMTBlock, which is always non-zero
            {
                ptr += 1;
                if (ptr >= (byte*)aPage + RAT.PageSize - 8)
                {
                    return null;
                }
            }
            return ptr;

        }

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
        /// <param name="aSize"></param>
        /// <returns></returns>
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
        /// <param name="aSize"></param>
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
        /// <param name="aSize"></param>
        /// <returns>Null if no more space on this page</returns>
        private static SMTBlock* GetFirstWithSpace(SMTPage* aPage, uint aSize)
        {
            return GetFirstWithSpace(GetFirstBlock(aPage, aSize), aSize);
        }

        /// <summary>
        /// Get the first block for this size in this SMT block chain, which has space left to allocate to
        /// </summary>
        /// <param name="aRoot">The root node to start the search at</param>
        /// <param name="aSize"></param>
        /// <returns></returns>
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
        /// <param name="aSize">Size must be divisible by 2 otherwise Alloc breaks</param>
        private static void AddRootSMTBlock(SMTPage* aPage, uint aSize)
        {
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
                Debugger.SendKernelPanic(0x83);
                while (true) { }
            }

            if (ptr->Size == 0) // This is the first block to be allocated on the page
            {
                ptr->Size = aSize;
            }
            else
            {
                RootSMTBlock* block = (RootSMTBlock*)NextFreeBlock(aPage);    // we should actually check that this is not null
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
            var page = SMT;
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
        public static uint GetRoundedSize(uint aSize)
        {
            return GetFirstBlock(SMT, aSize)->Size;
        }

        #endregion

        /// <summary>
        /// Init small heap.
        /// </summary>
        /// <exception cref="Exception">Thrown on fatal error, contact support.</exception>
        static public void Init()
        {
            //TODO Adjust for new page and header sizes
            // 4 slots, ~1k ea
            uint xMaxItemSize = RAT.PageSize / 4 - PrefixItemBytes;
            // Word align it
            mMaxItemSize = xMaxItemSize / sizeof(uint) * sizeof(uint);

            SMT = InitSMTPage();
        }

        /// <summary>
        /// Allocates and initialise a page for the SMT table
        /// </summary>
        /// <returns></returns>
        private static SMTPage* InitSMTPage()
        {
            SMTPage* page = (SMTPage*)RAT.AllocPages(RAT.PageType.SMT, 1);
            page->First = (RootSMTBlock*)page + 1;

            // TODO Change these sizes after further study and also when page size changes.
            // SMT can be grown as needed. Also can adjust and create new ones dynamicaly as it runs.
            // The current algorithm only works if we create the inital pages in increasing order
            AddRootSMTBlock(page, 16);
            AddRootSMTBlock(page, 24);
            AddRootSMTBlock(page, 48);
            AddRootSMTBlock(page, 64);
            AddRootSMTBlock(page, 128);
            AddRootSMTBlock(page, 256);
            AddRootSMTBlock(page, 512);
            AddRootSMTBlock(page, mMaxItemSize);
            return page;
        }

        /// <summary>
        /// Create a page with the size of an item and try add it to the SMT at a certain page
        /// If the SMT page is full, it will be added to the first SMT page with space or a new SMT page is allocated
        /// </summary>
        /// <param name="aItemSize">Object size in bytes</param>
        /// <exception cref="Exception">Thrown if:
        /// <list type="bullet">
        /// <item>aItemSize is 0.</item>
        /// <item>aItemSize is not word aligned.</item>
        /// <item>SMT is not initialized.</item>
        /// <item>The item size is bigger then a small heap size.</item>
        /// </list>
        /// </exception>
        static void CreatePage(SMTPage* aPage, uint aItemSize)
        {
            byte* xPtr = (byte*)RAT.AllocPages(RAT.PageType.HeapSmall, 1);
            if (xPtr == null)
            {
                return; // we failed to create the page, Alloc should still handle this case
            }

            uint xSlotSize = aItemSize + PrefixItemBytes;
            uint xItemCount = RAT.PageSize / xSlotSize;
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
                    if(smtBlock != null)
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
                        Debugger.SendKernelPanic(0x93);
                        while (true) { };
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
        public static byte* Alloc(ushort aSize)
        {
            SMTBlock* pageBlock = GetFirstWithSpace(aSize);
            if (pageBlock == null) // This happens when the page is full and we need to allocate a new page for this size
            {
                CreatePage(SMT, GetRoundedSize(aSize)); // CreatePage will try add this page to any page of the SMT until it finds one with space
                pageBlock = GetFirstWithSpace(aSize);
                if (pageBlock == null)
                {
                    //this means that we cant allocate another page
                    Debugger.SendKernelPanic(0x121);
                }
            }

            //now find position in the block
            ushort* page = (ushort*)pageBlock->PagePtr;
            uint elementSize = GetRoundedSize(aSize) + PrefixItemBytes;
            uint positions = RAT.PageSize / elementSize;
            for (int i = 0; i < positions; i++)
            {
                if (page[i * elementSize / 2] == 0)
                {
                    // we have found an empty slot

                    // update SMT block info
                    pageBlock->SpacesLeft--;

                    // set info in page
                    ushort* heapObject = &page[i * elementSize / 2];
                    heapObject[0] = aSize; // size of actual object being allocated
                    heapObject[1] = 0; // gc status starts as 0

                    return (byte*)&heapObject[2];

                }
            }

            // if we get here, RAM is corrupted, since we know we had a space but it turns out we didnt
            Debugger.DoSendNumber((uint)pageBlock);
            Debugger.DoSendNumber(aSize);
            Debugger.SendKernelPanic(0x122);
            while (true) { }
        }

        /// <summary>
        /// Free a object
        /// </summary>
        /// <param name="aPtr">A pointer to the start object.</param>
        public static void Free(void* aPtr)
        {
            ushort* heapObject = (ushort*)aPtr;
            ushort size = heapObject[-2];
            if (size == 0)
            {
                // double free, this object has already been freed
                Debugger.DoBochsBreak();
                Debugger.DoSendNumber((uint)heapObject);
                Debugger.SendKernelPanic(0x99);
            }

            uint* allocated = (uint*)aPtr;
            allocated[-1] = 0; // zero both size and gc status at once

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
            for (int i = 0; i < bytes; i++)
            {
                allocated[i] = 0;
            }

            // need to increase count in SMT again
            // todo: store this info somewhere so this can be done in constant time
            byte* allocatedOnPage = RAT.GetPagePtr(aPtr);
            SMTPage* smtPage = SMT;
            SMTBlock* blockPtr = null;
            while (smtPage != null)
            {
                blockPtr = GetFirstBlock(smtPage, size)->First;
                while (blockPtr != null)
                {
                    if(blockPtr->PagePtr == allocatedOnPage)
                    {
                        blockPtr->SpacesLeft++;
                        return;
                    }
                    blockPtr = blockPtr->NextBlock;
                }
                smtPage = smtPage->Next;
            }

            // this shouldnt happen
            Debugger.DoSendNumber((uint)aPtr);
            Debugger.DoSendNumber((uint)SMT);
            Debugger.SendKernelPanic(0x98);
            while (true) { }
        }

        #region Statistics

        /// <summary>
        /// Counts how many elements are currently allocated
        /// </summary>
        public static int GetAllocatedObjectCount()
        {
            var ptr = SMT;
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
        /// <param name="aSize"></param>
        /// <returns></returns>
        private static int GetAllocatedObjectCount(SMTPage* aPage)
        {
            var ptr = aPage->First;
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
                count += (int)(RAT.PageSize / (size + PrefixItemBytes)) - (int)ptr->SpacesLeft;
                ptr = ptr->NextBlock;
            }

            return count;
        }

        #endregion
    }
}
