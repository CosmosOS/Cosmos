using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;

namespace MM
{
    internal unsafe struct StaticNodeSortedList
    {
        internal HoleNode* head;



        //[Inline]
        //internal unsafe void  Remove(HoleNode* nodeToRemove)
        //{

        //}

        [Inline]
        internal unsafe HoleNode* FindAndRemove(uint length)
        {
            //if (head == null)
            //    return null; 

            HoleNode* current_node = head;

            while (current_node != null)
            {
                if (current_node->Length >= length)
                {

                }

            }


            var resultPrev = FindSmaller(length);
            if (resultPrev == null && head->Length > length)
            {
                head = null;
                return head;
            }

            var result = resultPrev->Next;
            resultPrev->Next = result->Next;
            result->Next = null; 
            
            return result; 
        }





        // wont find the head
        [Inline]
        private unsafe HoleNode* FindSmaller(uint length)
        {
      

            HoleNode* ptr = head;
            while (ptr != null)
            {
                if (ptr->Next == null)   //not found
                    return null; 

                if (ptr->Next->Length >= length)
                    return ptr; 
                
                ptr = ptr->Next;

            }

            return null; 
        }


        //[Inline]
        //internal unsafe HoleNode* Remove()
        //{
        //    if (head == null)
        //        throw new InvalidOperationException("No nodes left");

        //    var result = head;

        //    head = head->Next;
        //  //  NumberOfAvailableNodes--;

        //    return result;

        //}

         //AddAndMerge()

        [Inline]
        internal void Add(HoleNode* node)
        {
            //check null
            if (node == null)
                throw new InvalidOperationException("null");

            if (node == head)
                throw new InvalidOperationException("adding the head");

            if (node->Length == 0)
                throw new InvalidOperationException("0 length");

            if (head == null)
            {
                head = node;
                return;

            }


            // internal FInd 
            var result = FindSmaller(node->Length);

            var nextNode = result->Next;
            result->Next = node;
            node->Next = nextNode; 
   

        }
    } // StaticNodeSortedList
}
