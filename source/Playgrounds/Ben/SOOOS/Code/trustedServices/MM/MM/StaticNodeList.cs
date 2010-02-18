using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;
using System.Diagnostics;

namespace MM
{
    /// <summary>
    /// List with fixed amount of nodes
    /// </summary>
    internal unsafe struct StaticNodeList
    {
        const uint NumberOfNodesManaged = 25600;

        internal uint NumberOfAvailableNodes;

        //HoleNode* freeNodeSlots;
        fixed byte nodes[HoleNode.Size * (int)NumberOfNodesManaged];  //64Bit Hack

        internal HoleNode* head;
        //TODO  HoleNode* tail;

        // head 
        internal void Init()
        {
          //  if (NumberOfAvailableNodes != 0
            //    && NumberOfAvailableNodes != NumberOfNodesManaged)
              //  throw new InvalidOperationException("outstanding nodes cant init");
            NumberOfAvailableNodes = NumberOfNodesManaged;

            fixed (byte* buffer = nodes)
            {

                HoleNode* prevNode = (HoleNode*)&buffer[0];
                head = prevNode;

                for (int i = 0; i < NumberOfNodesManaged; i++)
                {
                    HoleNode* ptr = (HoleNode*)&(buffer[i * HoleNode.Size]);
                    prevNode->Next = ptr;
                    prevNode = ptr; 

                }
            }

        }

       
        [Inline]
        internal unsafe HoleNode* Remove()
        {
            if (head == null)
                throw new InvalidOperationException("No nodes left"); 

            var result = head;

            head = head->Next;
            result->Next = null; 
            NumberOfAvailableNodes--;

            Debug.WriteLine("Nodes: "+ NumberOfAvailableNodes);

            return result;

        }

        [Inline]
        internal void Add(HoleNode* node)
        {
            //check null
            if (node == null)
                throw new InvalidOperationException("null");

            if (node == head)
                throw new InvalidOperationException("adding the head");

            if (NumberOfAvailableNodes == NumberOfNodesManaged)
                throw new InvalidOperationException("already full");

            //Add to head.
            node->Next = head;
            head = node;

            NumberOfAvailableNodes++; 
        }




    }
}
