using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.Collections;
using CoreLib.ProcessModel;
using CoreLib.GC;

namespace CoreLib.IPC.IPC.Messaging.AsyncLocklessQueued
{
    /// <summary>
    /// queue is not managed by own GC!. 
    /// 
    /// 
    /// 
    /// </summary>
    [Trusted]
    public class AsyncLocklessQueuedMessageWriter : IIPCMessageWriter , IDisposable
    {
        Tube tube;
        SafeCircularFifoQueue<IIPCMessage> queue;
        IAllocator destinationSTPAllocator;
        System.Threading.AutoResetEvent produceEvent;
        System.Threading.AutoResetEvent consumeEvent; 


        internal AsyncLocklessQueuedMessageWriter(SafeCircularFifoQueue<IIPCMessage> queue , Tube tube 
            , System.Threading.AutoResetEvent produceEvent
            , System.Threading.AutoResetEvent consumeEvent)
        {
            this.queue = queue;
            this.tube = tube;
            destinationSTPAllocator = tube.Destination.Thread.Allocator; 
            this.produceEvent = produceEvent;
            this.consumeEvent = consumeEvent; 

        }

        internal AsyncLocklessQueuedMessageWriter(Tube tube)
        {
            this.tube = tube; 
        }

        public void WriteStruct(ref IIPCMessage message)
        {
       

            // if managed by the same GC we dont need to copy etc.
            if (destinationSTPAllocator.Collector == STP.Current.Thread.Allocator.Collector)
            {
                queue.Enqueue(message);
                return;
            }

            // Thread crap .

            //If a Collect is called in between a copy to the GC it would be a problem 
            if (STP.Current.Thread.Allocator.Collector.State == GC.CollectionState.PendingCollect)
            {
                //todo wait on collect and retry.
                ;
            }

            IAllocator currentAllocator = STP.Current.Thread.Allocator;
            try
            {
                STP.Current.Thread.Allocator = this.destinationSTPAllocator;

                while (queue.Enqueue(message) == false) // via copy 
                {
                    //queue full wait ...
                    consumeEvent.WaitOne(); 
                }

                // in case they are waiting
                produceEvent.Set();


            }
            finally //reset 
            {
                STP.Current.Thread.Allocator = currentAllocator;

            }
             
        }

        public void WriteImmutable(ref IIPCMessage message, int size)
        {
            throw new NotImplementedException();
        }


        public Tube Tube
        {
            get { return tube; }
        }

        

        public void WriteImmutable(ref IIPCMessage message)
        {
            throw new NotImplementedException();
        }

        public ICollection<Type> ValidTypes
        {
            get { throw new NotImplementedException(); }
        }

        public void Dispose()
        {
            queue = null; 
        }
    }
}
