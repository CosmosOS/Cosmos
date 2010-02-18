using System;
using System.Threading;


#pragma warning disable 0420

namespace CoreLib.Collections
{


    /// <summary>
    /// thread safe for single producer and consumer 
    /// 
    /// For all these queues teh struct is copied to the struct then copied to the array 
    /// a pointer based queue would be significantly faster
    /// 
    /// uses a spare item to tell dif between full and empty
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// 


    //TODO use a cheaper varient on single core that instead of interlock just disables interupts  note for Single thread CPUs you can improve it further by removing the Interlock
    //   hence   1 Thread CPU , interupts off , no interlock 
    //           2+ Thread shared cache CPU Interlock , no  volatile  (LockFreeQueue)
    //           2+ Thread seperat cache  Interlock + Volatile
    //    this will require serious testing and a verification if the perf is worth it .

    //TODO verify with CHESS.
    //TODO evaluate cache size vs performance / maybe option fast or small since were adding 128 bytes per channel for 1000 chanells thats 128K
    //TODO need to use struct with explicit layout. 
    public class SafeCircularFifoQueue<T> : IQueue<T>
    {
        //ORDER is important here since volatile flushes the cahce 
        //TODO add cache line buffers.
        volatile int m_consumerIndex;
   
      
        
        private T[] m_buffer;
        volatile int m_producerIndex;


        public SafeCircularFifoQueue(uint capacity)
        {
            if (capacity < 1) 
                throw new ArgumentOutOfRangeException("capacity");

            m_buffer = new T[capacity+1];
         

        }





        private int Capacity
        {
            get { return m_buffer.Length; }
        }



        public bool IsEmpty
        {
            get { return (m_consumerIndex == m_producerIndex); }
        }



        public bool IsFull
        {
            get { return (((m_producerIndex + 1) % Capacity) == m_consumerIndex); }
        }



        public bool Enqueue(T value)
        {
            if (IsFull)
            {
                return false;
            }
            m_buffer[m_producerIndex] = value;
            Interlocked.Exchange(ref m_producerIndex, (m_producerIndex + 1) % Capacity);

            return true;
        }


       


        public bool Dequeue(out T outVal)
        {
            if (IsEmpty)
            {
                outVal = default(T); 
                return false;
            }

            T value = m_buffer[m_consumerIndex];
            m_buffer[m_consumerIndex] = default(T);
            Interlocked.Exchange(ref m_consumerIndex, (m_consumerIndex + 1) % Capacity);

         

            outVal = value;
            return true;
        }


        /// <summary>
        ///  Use with structs where you want to copy by val out. 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public T Dequeue(out bool result)
        {
       
            if (IsEmpty)
            {
                result = false; 
                return default(T);
            }

            T value = m_buffer[m_consumerIndex];
            m_buffer[m_consumerIndex] = default(T);
            Interlocked.Exchange(ref m_consumerIndex, (m_consumerIndex + 1) % Capacity);


            result = true; 
            return value;
        }


        

    }  // CircularFifoQueue
}

