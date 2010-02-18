using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using Syslib.Diagnostics;
using CoreLib;
using System.Diagnostics;

namespace MM.Simple
{
    // simple free block list MM
    // kept in increasing size order so quick blocks are calculated fastest

    // requires startup order 
    // mm static constructor called from boot
    // mm static init called from boot 

    // ipc started
    // mm service create  
    // mm service gets reference to mm
    // gcs can now call Memory MNager

    // note we can force start a GC with memory and then take the memory from the GC when we start


    /// <summary>
    /// Algorithm allocates exact amount everythign else is policy  ( eg 1Meg block allocations , factor of 2 allocations etc)
    /// thos policies will make the algortihm behave batter.
    /// 
    /// Note buddy works really well with a lot of GCs as they can allocate factors of 2.
    /// 
    /// O(1/2n) allocations ( small allocations fast , large slow) 
    /// O(1/2n)+ O(2n) merge  free
    /// 
    /// this structure allows us to easily chhange to have 
    /// - Dfferent queus for different sizes ( shift to find queue
    /// - Merge using ref on the node to buddies
    /// 
    /// On the other hand this algorithm has some advanatges if GCs use fixed amounts. I supose since GCs request memory infrequesntly performance is not so big an issue.
    /// 
    /// fast allocate slow free ( works well with GC until i can write some nodes with pointers to adjacent regions)
    /// </summary>
    internal unsafe static class MemoryManager
    {
        static ulong bytesAllocated;
        static ulong bytesManaged;

        static StaticNodeList availableNodesList;
        static SortedNodeList sortedNodelist;

        static internal unsafe UIntPtr Allocate(uint amountOfBytes)
        {
            Debug.WriteLine("Allocate");
            //HoleNode *hp = null;
            HoleNode* prev_ptr = null;

            UIntPtr old_base;
            var currentNode = Head;

            while (currentNode != null)
            {
                //is the hole big enough
                if (currentNode->Length >= amountOfBytes)
                {
                    old_base = currentNode->MemAddress;  // remember where it started 

                    //Its the next bigest size take a piece and reduce the hole
                    currentNode->MemAddress = UIntPtr.Add(currentNode->MemAddress, (int)amountOfBytes);
                    currentNode->Length -= amountOfBytes;

                    if (currentNode->Length == 0)
                    {
                        sortedNodelist.RemoveNode(prev_ptr, currentNode);
                        Debug.Write("Remove Node");
                    }
                    // merge will correct list on next free this will do unless we implement an index or a tree

                    bytesAllocated += amountOfBytes;
                    return old_base;
                }

                prev_ptr = currentNode;
                currentNode = currentNode->Next;

            }  // while 

            // none found caller should except 
            return UIntPtr.Zero;
        }

        // we check the size released against the original.
        static internal void Free(UIntPtr address, uint size)
        {
            if (size == 0)  //TODO static log
                return;

            Debug.WriteLine("Free");

            if (availableNodesList.NumberOfAvailableNodes == 0)
                Panic.Invoke("MM Out of nodes!");

            // get a node
            var node = availableNodesList.Remove();
            node->Length = size;
            node->MemAddress = address;

            bytesAllocated -= size;

            sortedNodelist.AddInLengthOrder(node);

            sortedNodelist.Merge(); 

            // if smaller than head become head 
            //if ( size <= nodelist.head->Length)



        }


        internal static void Init(UIntPtr baseAddress, ulong bufSize)
        {



            if (bufSize > uint.MaxValue)
                Panic.Invoke("Simple only support 4Gig single allocations break up and submit as regions");

            MemoryRegion reg = new MemoryRegion(baseAddress, (uint)bufSize);
            Init(new MemoryRegion[] { reg });

        }


        internal static void Init(MemoryRegion[] validRegions)
        {
            BootWaypoints.Started(BootWayPoint.MemoryManagerInit);

            // allows wiping earlier mm good for testing
            // if (bytesManaged != 0)
            //    Panic.Invoke("Simple MM does not support extending the memory managed");

            bytesManaged = 0;
            availableNodesList.Init();
            sortedNodelist.head = null;

            foreach (var region in validRegions)
            {
                var node = availableNodesList.Remove();
                node->Length = region.Size;
                node->MemAddress = region.Address;

                sortedNodelist.AddInLengthOrder(node);
                //Free(region.Address, region.Size);
                bytesManaged += region.Size;

            }

            bytesAllocated = 0;

            BootWaypoints.Finished(BootWayPoint.MemoryManagerInit);
        }

        public static ulong BytesAllocated
        {
            get { return MemoryManager.bytesAllocated; }
        }


        public static ulong BytesManaged
        {
            get { return MemoryManager.bytesManaged; }

        }


        internal static HoleNode* Head
        {
            get { return MemoryManager.sortedNodelist.head; }

        }



        struct SortedNodeList
        {
            internal HoleNode* head; // sorted from small to big


            // length order
            internal void AddInLengthOrder(HoleNode* nodeToInsert)
            {
                // boundary conditions
                if (head == null || head->Length >= nodeToInsert->Length)
                {
                    Debug.WriteLine("First item");
                    nodeToInsert->Next = head;
                    head = nodeToInsert;

                    return;
                }

                var current_node = head;
                HoleNode* last_node = null;

                int count = 0;

                while (current_node != null)
                {
                    if (current_node->Length >= nodeToInsert->Length)  // insert here
                    {

                        Debug.WriteLine("inserting at " + count);
                        var save_ptr = last_node->Next;
                        last_node->Next = nodeToInsert;

                        nodeToInsert->Next = current_node;
                        return;
                    }

                    last_node = current_node;
                    current_node = current_node->Next;
                    count++;

                }

                // insert at end  current is null
                nodeToInsert->Next = null;
                last_node->Next = nodeToInsert;

            } // add 

            [Inline]
            internal void RemoveNode(HoleNode* prevNode, HoleNode* currentNode)
            {
                if (prevNode == null || prevNode->Next == null)
                    throw new NullReferenceException("passed in null");

                if (currentNode == Head)
                    sortedNodelist.head = currentNode->Next;
                else
                    prevNode->Next = currentNode->Next;

                currentNode->MemAddress = UIntPtr.Zero;
                currentNode->Length = 0;

                availableNodesList.Add(currentNode); 


            }

            [Inline]
            ///this can be much more efficient but this is the simple one!
            internal void Merge()
            {
                if (head == null)
                    return;

                //create sorted list
                NodeListOrderedByAddress sortedByAddress;
                sortedByAddress.head = null;

                while (head != null) // remove from head 
                {
                    var next = head->Next;
                    head->Next = null; // detach  

                    sortedByAddress.AddByAddressOrder(head);
                    head = next;
                }

                MergeAdjacentList(sortedByAddress.head);

                // set head to null and re-add everyting to rebuild in size Order.
                head = null;

                var currentNode = sortedByAddress.head;
                while (currentNode != null)
                {
                    var next = currentNode->Next;
                    currentNode->Next = null; //detach from mem order
                    AddInLengthOrder(currentNode);
                    currentNode = next;
                }


            }



            /// <summary>
            /// returns number merged
            /// </summary>
            /// <returns></returns>
            private void MergeAdjacentList(HoleNode* head)
            {



                // join the pieces 
                var currentNode = head;
                while (currentNode != null)
                {
                    if (currentNode->Next != null)
                    {
                        UIntPtr nextAddr = UIntPtr.Add(currentNode->MemAddress, (int)currentNode->Length);
                        if (currentNode->Next->MemAddress == nextAddr)
                        {
                            var toRelease = currentNode->Next;
                            NodeListOrderedByAddress.MergeAdjacent(currentNode);
                            availableNodesList.Add(toRelease);

                          //  Debug.WriteLine(" merging node " + 

                            continue; // repeat logic on same expanded node
                        }
                    }
                    currentNode = currentNode->Next;
                }


            } //MErge

            //[Inline]
            //private void AddOrderByAddress(HoleNode* nodeToInsert, HoleNode* list)
            //{
            //    //// boundary conditions
            //    //if (list == null)
            //    //{
            //    //    list = nodeToInsert;
            //    //    return list;
            //    //}

            //    if ((byte*)list->MemAddress >= (byte*)nodeToInsert->MemAddress) // needs to go to head
            //    {
            //        nodeToInsert->Next = list;
            //        list = nodeToInsert;
            //    }


            //    var current_node = list;
            //    HoleNode* last_node = null;


            //    while (current_node != null)
            //    {
            //        if ((byte * ) current_node->MemAddress>= (byte *) nodeToInsert->MemAddress)  // insert here
            //        {
            //            var save_ptr = last_node->Next;
            //            last_node->Next = nodeToInsert;

            //            nodeToInsert->Next = current_node;

            //        }

            //        last_node = current_node;
            //        current_node = current_node->Next;

            //    }

            //    // insert at end 
            //    nodeToInsert->Next = null;
            //    last_node->Next = nodeToInsert;


            //} // add 


        }  //






    }
}
