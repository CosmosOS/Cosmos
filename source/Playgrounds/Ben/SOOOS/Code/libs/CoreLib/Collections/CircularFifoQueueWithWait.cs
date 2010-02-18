using System;
using System.Threading;


#pragma warning disable 0420

namespace CoreLib.Collections
{


    /// <summary>
    /// thread safe for single producer and consumer 
    /// 
    /// uses a spare item to tell dif between full and empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 


    //TODO verify with CHESS.
    //TODO evaluate cache size vs performance / maybe option fast or small since were adding 128 bytes per channel for 1000 chanells thats 128K
    //TODO need to use struct with expliocity layout.
    //TODO wrap IQueue ! 
    public class CircularFifoQueueWithWait<T> : CoreLib.Collections.IQueueWithWait<T> //: IQueue<T>
    {
        const int CACHE_SIZE = 32;



        //ORDER is important here since volatile flushes the cahce 
        volatile int m_consumerIndex;
        // for 32 byte cache size since volatile invalidates 

        volatile int m_producerIndex;
        //byte[] cacheDummy2;

        volatile int m_producerWaiting;
        //byte[] cacheDummy3;

        volatile int m_consumerWaiting;
        //byte[] cacheDummy4;

        private T[] m_buffer;
        private AutoResetEvent m_consumerEvent;
        private AutoResetEvent m_producerEvent;


        public CircularFifoQueueWithWait(uint capacity)
        {
            if (capacity < 1) 
                throw new ArgumentOutOfRangeException("capacity");

            m_buffer = new T[capacity+1];
            m_consumerEvent = new AutoResetEvent(false);
            m_producerEvent = new AutoResetEvent(false);

        }


        public CircularFifoQueueWithWait(int capacity, bool fastCache)
        {

            if (capacity < 2) 
                throw new ArgumentOutOfRangeException("capacity");

            m_buffer = new T[capacity];
            m_consumerEvent = new AutoResetEvent(false);
            m_producerEvent = new AutoResetEvent(false);

         
        }



        private int Capacity
        {
            get { return m_buffer.Length; }
        }



        private bool IsEmpty
        {
            get { return (m_consumerIndex == m_producerIndex); }
        }



        private bool IsFull
        {
            get { return (((m_producerIndex + 1) % Capacity) == m_consumerIndex); }
        }



        public void Enqueue(ref T value)
        {
            if (IsFull)
            {
                WaitUntilNonFull();
            }
            m_buffer[m_producerIndex] = value;
            Interlocked.Exchange(ref m_producerIndex, (m_producerIndex + 1) % Capacity);

            if (m_consumerWaiting == 1)
            {
                m_consumerEvent.Set();
            }
        }


        private void WaitUntilNonFull()
        {
            Interlocked.Exchange(ref m_producerWaiting, 1);

            try
            {
                while (IsFull)
                {
                    m_producerEvent.WaitOne();
                }
            }

            finally
            {
                m_producerWaiting = 0;
            }
        }



        public void Dequeue(ref T outVal)
        {
            if (IsEmpty)
            {
                WaitUntilNonEmpty();
            }

            T value = m_buffer[m_consumerIndex];
            m_buffer[m_consumerIndex] = default(T);
            Interlocked.Exchange(ref m_consumerIndex, (m_consumerIndex + 1) % Capacity);

            if (m_producerWaiting == 1)
            {
                m_producerEvent.Set();
            }

            outVal = value;
        }



        private void WaitUntilNonEmpty()
        {
            Interlocked.Exchange(ref m_consumerWaiting, 1);
            try
            {
                while (IsEmpty)
                {
                    m_consumerEvent.WaitOne();
                }

            }
            finally
            {
                m_consumerWaiting = 0;
            }
        }


    }  // CircularFifoQueue
}

