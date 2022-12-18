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
        /// Find the next free block in the smt
        /// </summary>
        /// <returns>Pointer to the start of block in the SMT. null if all SMT pages are full</returns>
        private static SMTBlock* NextFreeBlock()
        {
            var page = SMT;
            SMTBlock* pos = null;
            while (page != null && pos == null)
            {
                pos = NextFreeBlock(page);
                page = page->Next;
            }
            return pos;
        }


        /// <summary>
        /// Find the next free block in a page
        /// </summary>
        /// <returns>Pointer to the start of block in the SMT. null if all SMT pages are full</returns>
        private static SMTBlock* NextFreeBlock(SMTPage* aPage)
        {
            var ptr = (SMTBlock*)aPage->First; // since both RootSMTBlock and SMTBlock have the same size (20) it doesnt matter if cast is wrong
            while (ptr->PagePtr != null) // this would check Size if its actually a RootSMTBlock
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
        /// Gets the last block in the SMT for objects of this size
        /// </summary>
        /// <param name="aSize"></param>
        /// <returns></returns>
        private static SMTBlock* GetLastBlock(uint aSize)
        {
            var page = GetLastPage();
            return GetLastBlock(page, aSize);
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

        private static SMTBlock* GetFirstWithSpace(uint aSize)
        {
            var page = SMT;
            SMTBlock* block = null;
            do
            {
                block = GetFirstWithSpace(page, aSize);
                page = page->Next;
            } while (block == null && page != null);
            return block;
        }

        /// <summary>
        /// Get the first block for this size, which has space left to allocate to
        /// </summary>
        /// <param name="aSize"></param>
        /// <returns></returns>
        private static SMTBlock* GetFirstWithSpace(SMTPage* aPage, uint aSize)
        {
            return GetFirstWithSpace(aSize, GetFirstBlock(aPage, aSize));
        }

        /// <summary>
        /// Get the first block for this size, which has space left to allocate to
        /// </summary>
        /// <param name="aSize"></param>
        /// <param name="root">The root node to start the search at</param>
        /// <returns></returns>
        private static SMTBlock* GetFirstWithSpace(uint aSize, RootSMTBlock* root)
        {
            SMTBlock* ptr = root->First;
            if (ptr == null)
            {
                return null;
            }
            var lptr = ptr;
            while (ptr->SpacesLeft == 0 && ptr->NextBlock != null)
            {
                lptr = ptr;
                ptr = ptr->NextBlock;
            }
            if (ptr->SpacesLeft == 0 && ptr->NextBlock == null)
            {
                return null;
            }
            return ptr;
        }

        /// <summary>
        /// Add a new root block for a certain size
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

            if (ptr->Size == 0)
            {
                ptr->Size = aSize;
            }
            else
            {
                var block = (RootSMTBlock*)NextFreeBlock(aPage);    // we should actually check that this is not null
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
        private static SMTPage* GetLastPage()
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
            xMaxItemSize = xMaxItemSize / sizeof(uint) * sizeof(uint);
            InitSMT(xMaxItemSize);
        }

        /// <summary>
        /// Init SMT (Size Map Table).
        /// </summary>
        /// <param name="aMaxItemSize">A max item size.</param>
        static void InitSMT(uint aMaxItemSize)
        {
            mMaxItemSize = aMaxItemSize;
            var page = InitSMTPage();
            SMT = page;
        }

        /// <summary>
        /// Allocates and initialise a page for the SMT table
        /// </summary>
        /// <returns></returns>
        private static SMTPage* InitSMTPage()
        {
            var page = (SMTPage*)RAT.AllocPages(RAT.PageType.SMT, 1);
            page->First = (RootSMTBlock*)(page + 1);

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
        /// Create a page with the size of an item and add it to the SMT at a certain page
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
            var xPtr = (byte*)RAT.AllocPages(RAT.PageType.HeapSmall, 1);
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
            var parent = GetLastBlock(aPage, aItemSize);
            var smtBlock = NextFreeBlock(aPage); //get the next free block in the smt

            if (smtBlock == null) // we could not allocate a new block since the SMT table is all full
            {
                // we need to expand the SMT table by a page
                var last = SMT;
                while (last->Next != null)
                {
                    last = last->Next;
                }
                last->Next = InitSMTPage();
                parent = GetLastBlock(last->Next, aItemSize);
                smtBlock = NextFreeBlock();
                if (smtBlock == null)
                {
                    Debugger.SendKernelPanic(0x93);
                    while (true) { };
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
                var root = GetFirstBlock(aPage, aItemSize);
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
            var pageBlock = GetFirstWithSpace(aSize);
            if (pageBlock == null) // This happens when the page is full and we need to allocate a new page for this size
            {
                CreatePage(GetLastPage(), GetRoundedSize(aSize));
                pageBlock = GetFirstWithSpace(aSize);
                if (pageBlock == null)
                {
                    //this means that we cant allocate another page
                    Debugger.SendKernelPanic(0x121);
                }
            }

            //now find position in the block
            var page = (ushort*)pageBlock->PagePtr;
            var elementSize = GetRoundedSize(aSize) + PrefixItemBytes;
            var positions = RAT.PageSize / elementSize;
            for (int i = 0; i < positions; i++)
            {
                if (page[i * elementSize / 2] == 0)
                {
                    // we have found an empty slot

                    // update SMT block info
                    pageBlock->SpacesLeft--;

                    // set info in page
                    var heapObject = &page[i * elementSize / 2];
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
            var heapObject = (ushort*)aPtr;
            ushort size = heapObject[-2];
            if (size == 0)
            {
                // double free, this object has already been freed
                Debugger.DoBochsBreak();
                Debugger.DoSendNumber((uint)heapObject);
                Debugger.SendKernelPanic(0x99);
            }

            var allocated = (uint*)aPtr;
            allocated[-1] = 0; // zero both size and gc status at once

            // now zero the object so its ready for next allocation
            if (size < 4) // so we dont actually forget to clean up too small items
            {
                size = 4;
            }
            var bytes = size / 4;
            if (size % 4 != 0)
            {
                bytes += 1;
            }
            for (int i = 0; i < bytes; i++)
            {
                allocated[i] = 0;
            }

            // need to increase count in SMT again
            var allocatedOnPage = RAT.GetPagePtr(aPtr);
            var smtPage = SMT;
            SMTBlock* blockPtr = null;
            while (smtPage != null)
            {
                blockPtr = GetFirstBlock(smtPage, size)->First;
                while (blockPtr != null && blockPtr->PagePtr != allocatedOnPage)
                {
                    blockPtr = blockPtr->NextBlock;
                }
                if(blockPtr->PagePtr == allocatedOnPage)
                {
                    break;
                }
                smtPage = smtPage->Next;
            }

            if (blockPtr == null)
            {
                // this shouldnt happen
                Debugger.DoSendNumber((uint)aPtr);
                Debugger.DoSendNumber((uint)SMT);
                Debugger.SendKernelPanic(0x98);
                while (true) { }
            }
            blockPtr->SpacesLeft++;
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
            var root = GetFirstBlock(aPage, aSize);
            var ptr = root->First;

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
