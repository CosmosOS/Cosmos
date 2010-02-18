using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MM.LargeBlock
{
    /// <summary>
    /// Basically a Linked list but we save nodes so the GC is called as few times as possible. 
    /// We want to avoid the situation that the Kernel GC is out of memory ( for even a LinkedListNode) and we get a page request 
    /// From any GC. 
    /// 
    /// Be carefull here as not all methods reuse nodes
    /// 
    /// TODO move so Util lib with tests
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedListWithNodeMemoryReuse<T> : LinkedList<T>
    {
        private List<LinkedListNode<T>> reusedNodeList;

        public LinkedListWithNodeMemoryReuse()
            : this(10)
        {

        }

        public LinkedListWithNodeMemoryReuse(uint reservedCount)
        {

            reusedNodeList = new List<LinkedListNode<T>>((int) reservedCount);
            AllocateMemoryForNodes(reservedCount);
        }

        //TODO if there is an allocation fail the GC must collect and we rerun this.
        private void AllocateMemoryForNodes(uint reservedCount)
        {
            for (int i = 0; i < reservedCount; i++)
            {
                T op = default(T); 
                reusedNodeList.Add(new LinkedListNode<T>(op));
            }
        }

        private LinkedListNode<T> GetFreeNode(T entry)
        {

            if (reusedNodeList.Count ==  0)
            {
                // increase count by 20% 
                AllocateMemoryForNodes((uint) base.Count / 5);
                return GetFreeNode(entry);
            }


            var result = reusedNodeList[0];
            reusedNodeList.RemoveAt(0);
            result.Value = entry;

            return result; 
        }

        public new LinkedListNode<T> AddFirst(T entry)
        {
            var node = GetFreeNode(entry);
            base.AddFirst(node);
            return node; 

        }

        public new void  AddFirst(LinkedListNode<T> node)
        {
            base.AddFirst(node);

        }

        public new void AddLast(LinkedListNode<T> node)
        {
            base.AddFirst(node);

        }




        public new LinkedListNode<T> AddBefore(LinkedListNode<T> beforeNode, T region)
        {
            var node = GetFreeNode(region);
            base.AddBefore(beforeNode, node);
            return node; 

        }

        public new LinkedListNode<T> AddLast(T region)
        {
            var node = GetFreeNode(region);
            base.AddLast(node);
            return node; 
        }


        /// <summary>
        /// Note the Node belongs to the list and will be reused !
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool RemoveWithReuse(T val)
        {
            // get node 
            var node = base.Find(val);

            if (node == null)
                return false; 

            RemoveWithReuse(node);
            return true; 
        }

        //public new void RemoveLast()
        //{
        //    Remove
        //}

        //public new void RemoveFirst()
        //{

        //}

        
        //note node is added to available list ..
        // if it were public we could reuse the node in the list as well as the caller. 
        private new void RemoveWithReuse(LinkedListNode<T> node)
        {


            base.Remove(node);
            reusedNodeList.Add(node); 
        }
    } //LinkedListWIthNodeMemoryReuse<T>
}
