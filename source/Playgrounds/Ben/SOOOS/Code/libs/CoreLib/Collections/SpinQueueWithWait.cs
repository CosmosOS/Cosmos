//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using CoreLib.Threading;

//namespace CoreLib.Collections
//{
//    /// <summary>
//    /// TODO compare to just locking around the head and tail variables ... 
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    public class SpinQueueWithWait<T> : IWaitQueue<T>
//    {
//        CheckedSpinLock sLock = new CheckedSpinLock();

//        uint head;
//        T[] array;
//        uint tail;
//        readonly uint capacity;


//        //TODO capacioty must be more than 1 !
//        public SpinQueueWithWait(uint _capacity)
//        {
//            if (_capacity < 2)
//                throw new ArgumentOutOfRangeException("0 size is invalid");
//            array = new T[_capacity + 1];
//            this.capacity = (uint)array.Length; // n+1 compare 
//            tail = 0;
//            head = 0;
//        }


//        /// <summary>
//        /// Producer only: Adds item to the circular queue. 
//        /// If queue is full at 'push' operation no update/overwrite
//        /// will happen, it is up to the caller to handle this case
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <typeparam name="?"></typeparam>
//        /// <param name="?"></param>
//        /// <returns></returns>
//        public void Enqueue(ref T item)
//        {

//            if (IsEmpty)
//            {
//                WaitUntilNonEmpty();
//            }

//            // we have the 2nd lock

//            T value = m_buffer[m_consumerIndex];
//            m_buffer[m_consumerIndex] = default(T);
//            Interlocked.Exchange(ref m_consumerIndex, (m_consumerIndex + 1) % Capacity);

//            if (m_producerWaiting == 1)
//            {
//                m_producerEvent.Set();
//            }

//            outVal = value;






//            sLock.Acquire();
//            try
//            {
//                var nextTail = Increment(tail);
//                if (nextTail != head)
//                {
//                    array[tail] = item;
//                    tail = nextTail;
//                    return true;
//                }

//                // queue was full
//                return false;
//            }
//            finally
//            {
//                sLock.Release();
//            }
//        }


//        /// <summary>
//        /// Consumer only: Removes and returns item from the queue
//        /// If queue is empty at 'pop' operation no retrieve will happen
//        /// It is up to the caller to handle this case
//        /// 
//        /// </summary>
//        /// <param name="item"></param>
//        /// <returns></returns>
//        public bool Dequeue(out T item)
//        {
//            sLock.Acquire();
//            try
//            {

//                if (head == tail)
//                {
//                    item = default(T); 
//                    return false; // empty queue
//                }
//                item = array[head];
//                head = Increment(head);
//                return true;
//            }
//            finally
//            {
//                sLock.Release();
//            }
//        }


//        /// <summary>
//        /// Useful for testinng and Consumer check of status 
//        /// Remember that the 'empty' status can change quickly as the Producer adds more items
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <typeparam name="?"></typeparam>
//        /// <param name="?"></param>
//        /// <returns></returns>
//        //public bool IsEmpty()
//        //{
//        //    return (head == tail);
//        //}

//        private bool IsEmpty
//        {
//            get
//            {
//                sLock.Acquire();
//                try
//                {

//                    return
//                        (head == tail);

//                }
//                finally
//                {
//                    sLock.Release();
//                }
//            }
//        }


//        /// <summary>
//        ///  Useful for testing and Producer check of status
//        ///  Remember that the 'full' status can change quickly
//        ///  as the Consumer catches up.
//        ///  
//        /// </summary>
//        /// <returns></returns>
//        //public bool IsFull()
//        //{
//        //    var tailCheck = (tail + 1) % capacity;
//        //    return (tailCheck == head);
//        //}




//        private bool IsFull
//        {
//            get
//            {
//                    return (((tail + 1) % capacity) == head);
              

//            }
//        }


//        private void WaitUntilNonFull()
//        {

//                while (IsFull)
//                {
//                    m_producerEvent.WaitOne();
//                }
//        }


//        private void WaitUntilNonEmpty()
//        {
//            Interlocked.Exchange(ref m_consumerWaiting, 1);
//            try
//            {
//                while (IsEmpty)
//                {
//                    m_consumerEvent.WaitOne();
//                }

//            }
//            finally
//            {
//                m_consumerWaiting = 0;
//            }
//        }


//        /// <summary>
//        /// Increment helper function for index of the circular queue
//        /// index is inremented or wrappe
//        /// </summary>
//        /// <param name="idx_"></param>
//        /// <returns></returns>
//        [Inline]
//        private uint Increment(uint idx_)
//        {
//            // increment or wrap
//            // =================
//            //    index++;
//            //    if(index == array.lenght) -> index = 0;
//            //
//            //or as written below:   
//            //    index = (index+1) % array.length


//            // idx_ = (idx_ + 1) % capacity;

//            idx_++;
//            if (idx_ == array.Length)
//                idx_ = 0;
//            return idx_;
//        }


//        void IWaitQueue<T>.Dequeue(out T item)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
