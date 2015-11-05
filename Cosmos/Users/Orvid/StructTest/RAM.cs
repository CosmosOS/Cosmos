using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core
{
    public unsafe static class RAM
    {
        private static StructTest.Logger log = new StructTest.Logger(2);
        private const uint CONVENTIONAL_MEMORY_SIZE = 640 * 1024;
        private const ulong DEFAULT_SMALLEST_PAGE_SIZE = 4096;

        private static byte[] TreeData = null; // replace with below direct pointer when allocated manually

        private static ulong SmallestPageSize = DEFAULT_SMALLEST_PAGE_SIZE;
        private static int OrderToSmallestPage = 0;
        private static byte* Tree = null;
        private static ulong TreeSize = 0;
        private static byte* Pages = null;
        private static ulong PagesSize = 0;

        public static void Initialize()
        {
            MultiBoot.Header* header = (MultiBoot.Header*)CPU.GetMultiBootInfo();
            MultiBoot.Memory* memory = (MultiBoot.Memory*)header->mmap_address;
            uint memoryEntrySize = 0;
            uint memoryEntryCount = 0;
            if (header->mmap_length > 0)
            {
                memoryEntrySize = memory->size + 4;
                memoryEntryCount = header->mmap_length / memoryEntrySize;
            }
            // if (memoryEntrySize == 0 || memoryEntryCount == 0) throw some kind of kernel error, no memory map
            log.WriteLine("RAM > MMAP Entry: Size=" + memoryEntrySize + ", Count=" + memoryEntryCount);
            ulong endOfKernel = CPU.GetEndOfKernel();

            ulong kernelSize = 0;
            ulong availableAddress = 0;
            ulong availableSize = 0;
            MultiBoot.Memory* memoryIterator = memory;
            for (uint memoryEntry = 0; memoryEntry < memoryEntryCount; memoryEntry++, memoryIterator++)
            {
                // Don't need these checks, if we're only grabbing the region the kernel is allocated in for now
                // if (memoryIterator->address + memoryIterator->length <= CONVENTIONAL_MEMORY_SIZE) continue;
                // if (memoryIterator->type != MultiBoot.Memory.TYPE_AVAILABLE) continue;
                if (endOfKernel >= memoryIterator->address)
                {
                    if (endOfKernel <= (memoryIterator->address + memoryIterator->length))
                    {
                        availableAddress = memoryIterator->address;
                        availableSize = memoryIterator->length;
                        kernelSize = endOfKernel - memoryIterator->address;
                        availableAddress += kernelSize;
                        availableSize -= kernelSize;
                        break;
                    }
                }
            }
            // if (availableSize == 0) throw some kind of kernel error, should not happen... kernel not found in available memory?
            // if (availableSize < PageSize + 1) throw some kind of kernel error, there isn't at least 1 smallest size page (plus tree byte) available after kernel
            log.WriteLine("RAM > MMAP Available: Address=" + ToHex(availableAddress, 64) + ", Length=" + ToHex(availableSize, 64) + ", Kernel=" + ToHex(kernelSize, 64));
            ulong shiftMask = SmallestPageSize;
            log.WriteLine("ShiftMask BeforeLoop: " + shiftMask.ToString());
            byte shiftMaskBit = 0;
            while ((shiftMask & ((ulong)1 << shiftMaskBit)) == 0) 
            {
                log.WriteLine("ShiftMask: " + shiftMask.ToString());
                shiftMask |= ((ulong)1 << shiftMaskBit); 
                shiftMaskBit++;
            }
            ulong availableShift = availableSize & (~shiftMask);
            log.WriteLine("AvailableShift: " + availableShift.ToString());
            PagesSize = SmallestPageSize;
            while (availableShift != 0)
            {
                availableShift >>= 1;
                availableShift &= ~shiftMask;
                TreeSize <<= 1;
                TreeSize |= 0x01;
                PagesSize <<= 1;
                OrderToSmallestPage += 1;
            }
            log.WriteLine("out:" + TreeSize.ToString() + ":" + PagesSize.ToString());
            while ((TreeSize + PagesSize) > availableSize)
            {
                TreeSize >>= 1;
                PagesSize >>= 1;
                --OrderToSmallestPage;
            }
            log.WriteLine("RAM > Binary Heap: Order=" + ToHex((byte)OrderToSmallestPage, 8) + ", Size=" + ToHex(TreeSize, 64) + ", Paged=" + ToHex(PagesSize, 64));
            TreeData = new byte[TreeSize]; // Replaced by putting directly in memory before paged memory, init to 0's to initialize correctly
            fixed (byte* treePtr = TreeData) Tree = treePtr;
            Pages = Tree + TreeSize;
            // After this point, the allocation tree is ready for use, slightly CPU inefficient initialization is okay, but alloc/dealloc must be fast
            // Code after this needs to be replaced with optimized ASM plugs

            log.WriteLine("RAM> Starts: Tree=" + ToHex((ulong)Tree, 64) + ", Pages=" + ToHex((ulong)Pages, 64));

            // Let's find a page to handle 4000 bytes, should be 1 page, of the smallest size (4096)
            // For testing timing, make sure to remove all console outputs (averages around 100 per second unoptimized)
            int rtcSec = Cosmos.Hardware.RTC.Second;
            ulong offset = 0;
            byte* allocatedPage = null;
            log.WriteLine("RAM> Smallest Page Allocs Per Second: ");
            while (rtcSec == Cosmos.Hardware.RTC.Second) ;
            rtcSec = Cosmos.Hardware.RTC.Second;
            log.WriteLine("RAM> Smallest Page Allocs Per Second: ");
            int allocsPerSecond = 0;
            while (rtcSec == Cosmos.Hardware.RTC.Second)
            {
                offset = 0;
                allocatedPage = RequestPage(1, OrderToSmallestPage, Tree, ref offset);
                offset = 0;
                ReleasePage(allocatedPage, OrderToSmallestPage, Tree, ref offset);
                allocsPerSecond++;
            }
            log.WriteLine("RAM> Smallest Page Allocs Per Second: " + allocsPerSecond);

            /*
            ulong offset1 = 0;
            byte* allocatedPage1 = RequestPage(4000, OrderToSmallestPage, Tree, ref offset1);
            ulong allocatedPageAddress1 = (ulong)allocatedPage1;
            Console.WriteLine("Allocated 4000 bytes @ " + ToHex(allocatedPageAddress1, 64));

            ulong offset2 = 0;
            byte* allocatedPage2 = RequestPage(6000, OrderToSmallestPage, Tree, ref offset2);
            ulong allocatedPageAddress2 = (ulong)allocatedPage2;
            Console.WriteLine("Allocated 6000 bytes @ " + ToHex(allocatedPageAddress2, 64));

            ulong offset3 = 0;
            byte* allocatedPage3 = RequestPage(4000, OrderToSmallestPage, Tree, ref offset3);
            ulong allocatedPageAddress3 = (ulong)allocatedPage3;
            Console.WriteLine("Allocated 4000 bytes @ " + ToHex(allocatedPageAddress3, 64));

            offset2 = 0;
            if (ReleasePage(allocatedPage2, OrderToSmallestPage, Tree, ref offset2)) Console.WriteLine("Released 6000 bytes");

            offset1 = 0;
            if (ReleasePage(allocatedPage1, OrderToSmallestPage, Tree, ref offset1)) Console.WriteLine("Released 4000 bytes");

            offset3 = 0;
            if (ReleasePage(allocatedPage3, OrderToSmallestPage, Tree, ref offset3)) Console.WriteLine("Released 4000 bytes");
            */
        }

        public static byte* RequestPage(ulong pSize, int pOrder, byte* pTreeNode, ref ulong pOffset)
        {
            if ((*pTreeNode & 0x01) != 0) return null; // If this node is allocated already, it is fully used, return null
            if (pOrder > 0 && pSize <= (SmallestPageSize << (pOrder - 1))) // If this occurs, we are gaurenteed to NOT be on the last order, and can use a smaller order
            {
                *pTreeNode |= 0x02; // If not already split, it is now, we checked if whole page is allocated already, so we can split it here
                byte* allocatedPage = RequestPage(pSize, pOrder - 1, pTreeNode + 1, ref pOffset); // Call recursively, to work with right child node
                if (allocatedPage == null) // If nothing is available in the right, we'll check the left (this needs to be adjusted for single branching support later)
                {
                    pOffset += (SmallestPageSize << (pOrder - 1)); // Add to offset, by half of current order size, to get half way through for split
                    allocatedPage = RequestPage(pSize, pOrder - 1, pTreeNode + ((ulong)2 << (pOrder - 1)), ref pOffset); // Call recursively, to work with left child node
                    if (allocatedPage == null) pOffset -= (SmallestPageSize << (pOrder - 1)); // If no page is found, we need to subtract from offset what we added earlier
                }
                return allocatedPage; // Whether we find a page or not, this side of the branch has been checked, so step backward
            }
            if ((*pTreeNode & 0x02) != 0) return null; // If we get here, we had right size, but it is already split for another allocation, step backward
            //Console.WriteLine("RAM> RequestPage: Size=" + ToHex(pSize, 32) + ", Order: " + ToHex((byte)pOrder, 8) + ", Node=" + ToHex((ulong)pTreeNode, 64) + ", Page=" + ToHex((ulong)(Pages + pOffset), 64));
            *pTreeNode |= 0x01; // Getting here means we've fulfilled requirements and found a page somewhere, so we're going to mark it allocated now
            return Pages + pOffset; // Return the physical address based on the offset we kept track of, this beats using a mini stack
        }
        public static bool ReleasePage(byte* pPage, int pOrder, byte* pTreeNode, ref ulong pOffset)
        {
            if (*pTreeNode == 0) return false;
            if ((*pTreeNode & 0x01) != 0) // This node is allocated
            {
                if (pPage == Pages + pOffset) // If this is the page for the allocation, set the node unallocated and return true
                {
                    //Console.WriteLine("RAM> ReleasePage: Size=" + ToHex((SmallestPageSize << pOrder), 32) + ", Order: " + ToHex((byte)pOrder, 8) + ", Node=" + ToHex((ulong)pTreeNode, 64));
                    *pTreeNode ^= 0x01;
                    return true;
                }
                return false; // Node is allocated, but it's not this page, should not happen
            }
            if (pOrder == 0) return false; // should not happen
            // If we get here, this node must be split, first check if it's down the right branch
            byte* rightSiblingNode = pTreeNode + 1;
            byte* leftSiblingNode = pTreeNode + ((ulong)2 << (pOrder - 1));
            if (pPage < (Pages + pOffset + (SmallestPageSize << (pOrder - 1))))
            {
                if (ReleasePage(pPage, pOrder - 1, rightSiblingNode, ref pOffset))
                {
                    // This should always happen, we already confirmed the page is in the right branch somewhere
                    // Time to compact if our left branch sibling is also unused
                    if (*rightSiblingNode == 0 && *leftSiblingNode == 0)
                    {
                        //Console.WriteLine("RAM> CompactPage: Size=" + ToHex((SmallestPageSize << pOrder), 32) + ", Order: " + ToHex((byte)pOrder, 8));
                        *pTreeNode ^= 0x02; // Compact by removing split, if the sibling is also unused
                    }
                    return true;
                }
                // We should not get here, this means the page could not be found, possibly bad page pointer?
                // At this point we have verified that the page address is somewhere in the left side, but not at a proper page address
                return false;
            }
            pOffset += (SmallestPageSize << (pOrder - 1)); // If we get here, it's following the left, so we gotta add half a node size to the offset
            if (!ReleasePage(pPage, pOrder - 1, leftSiblingNode, ref pOffset))
            {
                pOffset -= (SmallestPageSize << (pOrder - 1)); // shouldn't happen
                return false;
            }
            if (*rightSiblingNode == 0 && *leftSiblingNode == 0)
            {
                //Console.WriteLine("RAM> CompactPage: Size=" + ToHex((SmallestPageSize << pOrder), 32) + ", Order: " + ToHex((byte)pOrder, 8));
                *pTreeNode ^= 0x02; // Compact by removing split, if the sibling is also unused
            }
            return true;
        }

        

        public static string ToHex(ulong aNumber, byte aBits)
        {
            string ret = "";
            ulong xValue = aNumber;
            byte xCurrentBits = aBits;
            ret += "0x";
            while (xCurrentBits >= 4)
            {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit)
                {
                    case 0:
                        xDigitString = "0";
                        goto default;
                    case 1:
                        xDigitString = "1";
                        goto default;
                    case 2:
                        xDigitString = "2";
                        goto default;
                    case 3:
                        xDigitString = "3";
                        goto default;
                    case 4:
                        xDigitString = "4";
                        goto default;
                    case 5:
                        xDigitString = "5";
                        goto default;
                    case 6:
                        xDigitString = "6";
                        goto default;
                    case 7:
                        xDigitString = "7";
                        goto default;
                    case 8:
                        xDigitString = "8";
                        goto default;
                    case 9:
                        xDigitString = "9";
                        goto default;
                    case 10:
                        xDigitString = "A";
                        goto default;
                    case 11:
                        xDigitString = "B";
                        goto default;
                    case 12:
                        xDigitString = "C";
                        goto default;
                    case 13:
                        xDigitString = "D";
                        goto default;
                    case 14:
                        xDigitString = "E";
                        goto default;
                    case 15:
                        xDigitString = "F";
                        goto default;
                    default:
                        ret += xDigitString;
                        break;
                }
            }
            return ret;
        }
    }
}
