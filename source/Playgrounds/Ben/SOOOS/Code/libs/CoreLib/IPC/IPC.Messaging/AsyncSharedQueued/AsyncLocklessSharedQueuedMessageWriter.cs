using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CoreLib.Collections;
using CoreLib.ProcessModel;
using CoreLib.GC;


namespace CoreLib.IPC.IPC.Messaging.AsyncSharedQueued
{
    public class AsyncSharedQueuedMessageWriter : IIPCMessageWriter
    {
        Tube tube;
        ExpandableSpinQueue<IPCMessageHolder> queue;
        IAllocator destinationSTPAllocator;
        System.Threading.AutoResetEvent waitEvent; //TODO replace with ourd

        internal AsyncSharedQueuedMessageWriter(Tube tube, AsyncSharedQueuedMessageReader reader)
        {
            this.tube = tube;
            this.queue = reader.Queue;
            this.destinationSTPAllocator =  tube.Destination.Thread.Allocator;

            waitEvent = reader.WaitEvent; 

        }

        //TODO is size needed for the other methods ? 
        public void WriteStruct(ref IIPCMessage message)
        {
            IPCMessageHolder holder = new IPCMessageHolder(tube , ref message);

            // if managed by the same GC we dont need to copy etc.
            if (destinationSTPAllocator.Collector == STP.Current.Thread.Allocator.Collector)
            {
                queue.Enqueue(holder);
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
            
                CopyToQueue(ref holder); 
                
                // in case they are waiting
                waitEvent.Set();
              

            }
            finally //reset 
            {
                   STP.Current.Thread.Allocator = currentAllocator; 

            }
             
           // STP.Current.Thread.SetAllocator
               
           
 
        }

        // queue can expand ! 
      
       //TODO check for no security violationa


        /// <summary>
        /// This is tricky
        /// 
        /// this method is run in the context of the remote GC but the message is passed via the stack of the sender thread
        /// 
        /// when it is added to the queue it is copied into the array ( and the destination GC ) anyway that is the theory
        /// 
        /// if it doesnt work we can allocate it manually via the STP.Thread.Allocator which is on the destination GC and then add to the queue via the ref method
        /// 
        /// </summary>
        /// <param name="holder"></param>
        [Inline]
        [Trusted]
        private void CopyToQueue(ref IPCMessageHolder holder)
        {
           queue.Enqueue(holder);          

        }

        public void WriteImmutable(ref IIPCMessage message)
        {
            throw new NotImplementedException();
        }


        


        public Tube Tube
        {
            get { return tube; }
        }


        public IIPCMessageWriter Reader
        {
            get { throw new NotImplementedException(); }
        }

     
        public ICollection<Type> ValidTypes
        {
            get { throw new NotImplementedException(); }
        }
    }
}
