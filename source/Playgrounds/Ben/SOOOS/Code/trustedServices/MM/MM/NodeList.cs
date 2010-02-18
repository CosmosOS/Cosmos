using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MM
{


    //Reserve some fixed memory and use that classes MUST be static (including Init - Remove from interface) 


    public class FlatPages
    {

        // WARNING: don't initialize any static fields in this class
        // without manually running the class constructor at startup!
        //private const uint    PageMask = MemoryManager.PageSize - 1;
        private const uint SmallSize = 1024;// MemoryManager.PageSize;

        private static UIntPtr lowerLimit;
        private static UIntPtr upperLimit;
        private static UIntPtr pageBase;    // Lowest page.
        private static UIntPtr pageCount;   // Number of pages.
        private static Object pageLock;
        private static UIntPtr baseAddress;
        private static ulong size;

        private static unsafe uint* pageTable;

        // We keep two lists of free pages:
        // The freeList has pages that can be used at any moment.
        // The saveList has pages that were reserved for growing a region.

        private static FreeNode freeListTail;
        private static FreeNode saveListTail;
        private static unsafe FreeNode* freeList;
        private static unsafe FreeNode* saveList;

        //
        // This represents the real time count of available memory
        //
        private static UIntPtr availableMemory;

    [StructLayout(LayoutKind.Sequential)]
    private struct LastNode
    {
        internal const uint Signature = 0xaa2222aa;
        internal const uint Removed = 0xee1111ee;

        internal uint signature;
        internal unsafe FreeNode* node;

       
        internal static unsafe LastNode* Create(UIntPtr addr, FreeNode* node)
        {
            LastNode* last = (LastNode*)addr;
            last->signature = LastNode.Signature;
            last->node = node;
            node->last = last;
#if VERBOSE
                Tracing.Log(Tracing.Debug, "addr={0:x8}, node={1:x8}",
                            addr, (UIntPtr) last->node);
#endif
            return last;
        }

       
        internal static unsafe void Remove(LastNode* last)
        {
            last->signature = Removed;
            last->node = null;
        }

       
        internal static unsafe void PrintLastNode(UIntPtr addr)
        {
            LastNode* last = (LastNode*)addr;
            DebugStub.WriteLine("ln.{1:x8}  ", __arglist((UIntPtr)last->node));
        }

    }


    //////////////////////////////////////////////////////////////////////
    //
    [StructLayout(LayoutKind.Sequential)]
    private struct FreeNode
    {
        internal const uint Signature = 0x22aaaa22;
        internal const uint Removed = 0x11eeee11;

        internal uint signature;
        internal unsafe FreeNode* prev;
        internal unsafe FreeNode* next;
        internal unsafe LastNode* last;
        internal UIntPtr bytes;
        internal bool isSave;

       
        internal static unsafe void Init(FreeNode* list, bool isSave)
        {
            list->signature = Signature;
            list->prev = list;
            list->next = list;
            list->last = null;
            list->bytes = 0;
            list->isSave = isSave;
        }

       
        internal static unsafe bool Remove(FreeNode* node)
        {
            FreeNode* prev;
            FreeNode* next;

            availableMemory -= node->bytes;
            DeRegisterFreeNode(node->bytes);

            UIntPtr page = MemoryManager.PageFromAddr((UIntPtr)node);
            *(pageTable + page) = MemoryManager.PageFree;

            next = node->next;
            prev = node->prev;
            prev->next = next;
            next->prev = prev;

            if (node->last != null)
            {
                LastNode.Remove(node->last);
            }
            node->signature = Removed;

            return (next == prev);
        }

       
        private static unsafe void InsertAsPrev(FreeNode* list, FreeNode* node)
        {
            FreeNode* prev;

            prev = list->prev;
            node->next = list;
            node->prev = prev;
            prev->next = node;
            list->prev = node;
        }

       
        private static unsafe void InsertAsNext(FreeNode* list, FreeNode* node)
        {
            FreeNode* next;

            next = list->next;
            node->prev = list;
            node->next = next;
            next->prev = node;
            list->next = node;
        }

       
        private static unsafe void InsertBySize(FreeNode* list, FreeNode* node)
        {
#if ALLOW_BOOT_ARGLIST
                DebugStub.Assert(node->bytes > 0,
                                 "FlatPages.InsertBySize node->bytes={0}",
                                 __arglist(node->bytes));
#endif
            if (node->bytes <= SmallSize)
            {
                // If the size is pretty small, we insert from the back of the list...
                for (FreeNode* step = list->prev; step != list; step = step->prev)
                {
                    if (step->bytes >= node->bytes)
                    {
                        InsertAsNext(step, node);
                        return;
                    }
                }
                InsertAsNext(list, node);
            }
            else
            {
                // Insert a region into the list by size.
                for (FreeNode* step = list; step->next != list; step = step->next)
                {
                    if (step->next->bytes <= node->bytes)
                    {
                        InsertAsNext(step, node);
                        return;
                    }
                }
                InsertAsPrev(list, node);
            }
        }

        ///////////////////////////////////////////////////////////
        // FreeNode's new routines start here

        internal static unsafe void PrintFreeList(FreeNode* list)
        {
            //int count = 0;
            //DebugStub.WriteLine
            //    ("        PRINT FREE LIST  (tail.{0:x8}  prev.{1:x8}  next.{2:x8})",
            //     __arglist((UIntPtr)(list),
            //               (UIntPtr)list->prev,
            //               (UIntPtr)list->next));
            //DebugStub.WriteLine("        ---------------------------------------------------");

            //for (FreeNode* node = list->next;
            //     node != list; node = node->next)
            //{
            //    DebugStub.Print
            //        ("        [{0}] b.{1:x8} e.{2:x8} {3,8}KB p.{4:x8} n.{5:x8} l.{6:x8}  --  ",
            //         __arglist(
            //                   count,
            //                   (UIntPtr)node, (UIntPtr)node + node->bytes,
            //                   node->bytes / (1024),
            //                   (UIntPtr)node->prev,
            //                   (UIntPtr)node->next,
            //                   (UIntPtr)node->last));
            //    if (node->last != null)
            //    {
            //        LastNode.PrintLastNode((UIntPtr)(node->last));
            //    }
            //    else
            //    {
            //        DebugStub.WriteLine();
            //    }
            //    if (count++ > 20)
            //    {
            //        DebugStub.WriteLine("\n        **** ERROR INFINITE LIST ****\n");
            //        DebugStub.Break();
            //    }
            //}
        }

        internal static unsafe UIntPtr GetFreeListTotalSize(FreeNode* list)
        {
            UIntPtr size = 0;
            for (FreeNode* node = list->next;
                 node != list; node = node->next)
            {
                size += node->bytes;
            }
            return size;
        }

       
        internal static unsafe FreeNode* GetFreeNodeAtBreakAddr(FreeNode* list, UIntPtr breakAddr)
        {
            int count = 0;

            for (FreeNode* node = list->next;
                 node != list; node = node->next)
            {

                if ((UIntPtr)node <= breakAddr
                    && breakAddr < ((UIntPtr)node + node->bytes))
                {
                    return node;
                }
                if (count++ > 20)
                {
                //    DebugStub.WriteLine("  WARNING: Can't GetFreeNode ListTail.{0:x8} at {1:x8} after 20 iterations",
                //                        __arglist((UIntPtr)list, breakAddr));
                //    DebugStub.Break();
                //
                }

            }
            return null;
        }


       
        internal static unsafe FreeNode* FindGoodFit(FreeNode* list,
                                                      UIntPtr bytes, UIntPtr alignment)
        {
#if ALLOW_BOOT_ARGLIST
                DebugStub.Assert(bytes > 0,
                                 "FlatPages.FindGoodFit bytes={0}",
                                 __arglist(bytes));
#endif
            // If it is a small allocation, we try to accelerate the search.
            if (bytes <= SmallSize && alignment <= MemoryManager.PageSize)
            {
                for (FreeNode* node = list->prev; node != list; node = node->prev)
                {
                    if (node->bytes >= bytes)
                    {
                        Remove(node);
                        return node;
                    }
                }
                return null;
            }
            else
            {
                // First try to find a region closest in size to bytes...
                FreeNode* best = null;
                for (FreeNode* node = list->next; node != list; node = node->next)
                {
                    if (bytes <= node->bytes)
                    {
                        UIntPtr full = SpaceToAlign((UIntPtr)node, alignment) + bytes;
                        if (full <= node->bytes)
                        {
                            // If we find a candidate, remember it.
                            best = node;
                            if (full == node->bytes)
                            {
                                // Stop if it is the ideal region.
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Stop if we have a candidate and we've reach smaller regions.
                        if (best != null)
                        {
                            break;
                        }
                    }
                }
                if (best != null)
                {
                    Remove(best);
                }
                return best;
            }
        }

       
        internal static unsafe FreeNode* FindBelow(UIntPtr limit, FreeNode* list,
                                                    UIntPtr bytes, UIntPtr alignment)
        {
            //DebugStub.Assert(bytes > 0,
            //                 "FlatPages.FindBelow bytes={0}",
            //                 __arglist(bytes));

            // Try to find the first region below the limit address.
            for (FreeNode* node = list->next; node != list; node = node->next)
            {
                if ((UIntPtr)node + bytes < limit && node->bytes >= bytes)
                {
                    UIntPtr full = SpaceToAlign((UIntPtr)node, alignment) + bytes;
                    if ((UIntPtr)node + full < limit && node->bytes >= full)
                    {
                        Remove(node);
                        return node;
                    }
                }
            }
            return null;
        }

       
        internal static unsafe FreeNode* GetNodeAt(UIntPtr addr)
        {
            UIntPtr page = MemoryManager.PageFromAddr(addr);

            if (*(pageTable + page) == MemoryManager.PageFreeFirst)
            {
                return (FreeNode*)addr;
            }
            return null;
        }

       
        internal static unsafe FreeNode* GetNodeFromLast(UIntPtr addr)
        {
            UIntPtr page = MemoryManager.PageFromAddr(addr);

            if (*(pageTable + page) == MemoryManager.PageFree &&
                *(pageTable + page + 1) != MemoryManager.PageFree)
            {

                return ((LastNode*)addr)->node;
            }
            if (*(pageTable + page) == MemoryManager.PageFreeFirst)
            {
                return (FreeNode*)addr;
            }
            return null;
        }

       
        internal static unsafe FreeNode* Create(UIntPtr addr, UIntPtr bytes, bool isSave)
        {
            // Mark a page as a node in the free list, initialize the node struct.
            FreeNode* node = (FreeNode*)addr;

#if VERY_VERBOSE
                Tracing.Log(Tracing.Debug,
                            isSave ?
                            "{0:x8}..{1:x8}, last={4:x8}" :
                            "{0:x8}..{1:x8}, last={4:x8}",
                            addr, addr+bytes, addr + bytes - MemoryManager.PageSize);
#endif

            UIntPtr page = MemoryManager.PageFromAddr(addr);
            *(pageTable + page) = MemoryManager.PageFreeFirst;

            node->signature = FreeNode.Signature;
            node->bytes = bytes;
            node->isSave = isSave;
            node->prev = null;
            node->next = null;
            node->last = null;

            if (bytes > MemoryManager.PageSize)
            {
                LastNode.Create(addr + bytes - MemoryManager.PageSize, node);
            }
            return node;
        }

       
        internal static unsafe void CreateAndInsert(FreeNode* list,
                                                    UIntPtr addr,
                                                    UIntPtr bytes)
        {
            FreeNode* node = Create(addr, bytes, list->isSave);
            //
            // This memory is available on the freeList, or the
            // SaveList.
            //
            // Upper level memory lock protects access to this field.
            //
            availableMemory += bytes;
            RegisterFreeNode(bytes);
#if VERBOSE
                Tracing.Log(Tracing.Debug,
                            list->isSave ?
                            "({0:x8}, {1:x8}, true), prev={3:x8}, next={4:x8}, last={5:x8}" :
                            "({0:x8}, {1:x8}, false), prev={3:x8}, next={4:x8}, last={5:x8}",
                            addr, bytes, (UIntPtr) node->prev,
                            (UIntPtr) node->next, (UIntPtr) node->last);
#endif


#if ALLOW_BOOT_ARGLIST
                DebugStub.Assert((bytes & MemoryManager.PageMask) == 0,
                                 "FlatPages.CreateAndInsert bytes={0}",
                                 __arglist(bytes));
                DebugStub.Assert((node->bytes & MemoryManager.PageMask) == 0,
                                 "FlatPages.CreateAndInsert node->bytes={0}",
                                 __arglist(node->bytes));
#endif
            InsertBySize(list, node);
        }

       
        internal static unsafe void ReturnExtraAbove(FreeNode* list,
                                                     UIntPtr addr,
                                                     ref UIntPtr found,
                                                     UIntPtr keep)
        {
            CreateAndInsert(list, addr + keep, found - keep);
            found = keep;
        }

       
        internal static unsafe void ReturnExtraBelow(FreeNode* list,
                                                     ref UIntPtr addr,
                                                     ref UIntPtr found,
                                                     UIntPtr keep)
        {
            CreateAndInsert(list, addr, found - keep);
            addr = addr + found - keep;
            found = keep;
        }
    }
}
}
