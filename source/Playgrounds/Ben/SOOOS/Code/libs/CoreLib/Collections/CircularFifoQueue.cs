using System;
using System.Threading;

namespace CoreLib.Collections
{


    /// <summary>
    /// thread safe for single producer and consumer provided all CPUS share the same cache 
    /// 
    /// uses a spare item to tell dif between full and empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 


    //TODO verify with CHESS.
    public class CircularFifoQueue<T> : CoreLib.Collections.IQueue<T>
    {
        

        volatile uint head; // output index
        byte[] dummy = new byte[28];  // for 32 byte cache size since volatile invalidates
        T[] array;
        volatile uint tail; // inputindex
        uint capacity;
        byte[] dummy2 = new byte[28]; // for 32 byte cache size since volatile invalidates


        //TODO capacioty must be more than 1 !
        public CircularFifoQueue(uint _capacity)
        {
            if (_capacity < 2 )
                throw new ArgumentOutOfRangeException("0 size is invalid"); 
            array = new T[_capacity+1];
            this.capacity =(uint) array.Length; // n+1 compare 
            tail = 0;
            head = 0;
        }


        /// <summary>
        /// Producer only: Adds item to the circular queue. 
        /// If queue is full at 'push' operation no update/overwrite
        /// will happen, it is up to the caller to handle this case
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="?"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        public bool Enqueue(T item)
        {
            var nextTail = Increment(tail);
            if (nextTail != head)
            {
                array[tail] = item;
                tail = nextTail;
                return true;
            }

            // queue was full
            return false;
        }


        /// <summary>
        /// Consumer only: Removes and returns item from the queue
        /// If queue is empty at 'pop' operation no retrieve will happen
        /// It is up to the caller to handle this case
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Dequeue(out T item)
        {
            if (head == tail)
            {
                item = default(T); 
                return false; // empty queue
            }

            item = array[head];
            head = Increment(head);
            return true;
        }


        /// <summary>
        /// Useful for testinng and Consumer check of status 
        /// Remember that the 'empty' status can change quickly as the Producer adds more items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="?"></typeparam>
        /// <param name="?"></param>
        /// <returns></returns>
        //public bool IsEmpty()
        //{
        //    return (head == tail);
        //}

  public bool IsEmpty
        {
            get { return (head == tail); }
        }


        /// <summary>
        ///  Useful for testing and Producer check of status
        ///  Remember that the 'full' status can change quickly
        ///  as the Consumer catches up.
        ///  
        /// </summary>
        /// <returns></returns>
        //public bool IsFull()
        //{
        //    var tailCheck = (tail + 1) % capacity;
        //    return (tailCheck == head);
        //}

      


        public bool IsFull
        {
            get { return (((tail + 1) % capacity) == head); }
        }


        /// <summary>
        /// Increment helper function for index of the circular queue
        /// index is inremented or wrappe
        /// </summary>
        /// <param name="idx_"></param>
        /// <returns></returns>
        [Inline]
        public uint Increment(uint idx_)
        {
            // increment or wrap
            // =================
            //    index++;
            //    if(index == array.lenght) -> index = 0;
            //
            //or as written below:   
            //    index = (index+1) % array.length
           
            
           // idx_ = (idx_ + 1) % capacity;

            idx_++; 
             if(idx_ == array.Length) 
                 idx_ = 0;
            return idx_;
        }


    }  // CircularFifoQueue
}

