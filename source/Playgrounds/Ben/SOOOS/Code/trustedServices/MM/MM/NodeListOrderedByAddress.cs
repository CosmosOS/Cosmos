using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;
using System.Diagnostics;

namespace MM.Simple
{
        internal unsafe struct NodeListOrderedByAddress
        {
            internal HoleNode* head; // sorted from small to big


            // memory order
            internal void AddByAddressOrder(HoleNode* nodeToInsert)
            {
                // boundary conditions

                // boundary conditions
                if (head == null || (byte*)head->MemAddress >= (byte*)nodeToInsert->MemAddress)
                {
                    Debug.WriteLine("First item");
                    nodeToInsert->Next = head;
                    head = nodeToInsert;

                    return;
                }





                var current_node = head;
                HoleNode* last_node = null;

                
                while ( current_node != null) 
                {
                    if ((byte*)current_node->MemAddress >= (byte*)nodeToInsert->MemAddress)   // insert here
                    {
                        var save_ptr = last_node->Next;
                        last_node->Next = nodeToInsert;

                        nodeToInsert->Next = current_node;
                        return;
                    }                                                                    

                    last_node = current_node;
                    current_node = current_node->Next;

                } 

                // insert at end 
                nodeToInsert->Next = null;
                last_node->Next = nodeToInsert;
            } // add 


            static internal void MergeAdjacent(HoleNode* currentNode)
            {
                // remove next node 
                var next = currentNode->Next;

                currentNode->Length += next->Length; // combine mem
                currentNode->Next = next->Next;

            }



         

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


        }
}
