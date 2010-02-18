using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.ProcessModel;
using CoreLib.GC;
using CoreLib.Locking;

namespace CoreLib.IPC.IPC.Messaging.SyncNonQueued
{


    /// <summary>
    ///
    /// 
    /// 
    /// This one supports 1 reader to 1 writer ( one reader multiple writers is possible and should be tried via interlocked) 
    /// </summary>
    public class SyncNonQueuedMessageWriter : IIPCMessageWriter
    {
        IIPCMessage message = null;
        Tube tube;
        SyncNonQueuedMessageReader reader;
        CoreLib.GC.IAllocator destinationSTPAllocator;
      

        System.Threading.EventWaitHandle waitHandle;  //yes directly changing the scheduler is faster but we will see.

        public SyncNonQueuedMessageWriter(Tube tube, SyncNonQueuedMessageReader reader)
        {
            this.tube = tube;
            this.reader = reader;
            destinationSTPAllocator = tube.Destination.Thread.Allocator;
            waitHandle = reader.WaitHandle;


        }


        /// <summary>
        /// Send and Receive in 1 call not that important here 
        /// </summary>
        /// <param name="message">message will be overwritted by the repply </param>
        [Inline]
        public void SendAndReceiveStruct(ref IIPCMessage message)
        {
            SendStructSync(ref message);
            reader.ReceiveMessage(out message);  // obviously an exception will cause a jump
        }

        public void WriteStruct(ref IIPCMessage message)
        {
            SendStructSync(ref message);
        }


        [Inline]
        internal bool IsMessageEmpty()
        {
            return message.messageId == 0;
        }


        [Inline]
        public void SendStructSync(ref IIPCMessage message)
        {
            while (true)
            {
                if (IsMessageEmpty()) 
                //if (Interlocked.CompareExchange(ref tube.Destination.Thread.State, (int)ThreadState.Receiving, (int)ThreadState.Waiting) == (int)ThreadState.Waiting)
                {
                     this.message = message;
                     return;
                }
                else
                {
                    ; // we didnt wait on wait handle 
                    waitHandle.WaitOne(); 
                }
            }
         
        }






        //[Inline]
        //void CopyMessageToRemoteGC(ref IIPCMessage message)
        //{
        //    CopyMessageToRemoteGC(ref message, false);
        //}


        /// <summary>
        /// TODO would be nice in to pass in the destination but this is a bit painfull
        /// </summary>
        /// <param name="message"></param>
        /// <param name="enqueue"></param>
        //[Inline]
        //void CopyMessageToRemoteGC(ref IIPCMessage message, bool enqueue)
        //{
        //    if (STP.Current.Thread.Allocator.Collector.State == GC.CollectionState.PendingCollect)
        //    {
        //        //todo wait on collect and retry.
        //        ;
        //    }

        //    IAllocator currentAllocator = STP.Current.Thread.Allocator;
        //    try
        //    {
        //        STP.Current.Thread.Allocator = destinationSTPAllocator;

        //        // copy on remote GC
        //        reader.Message = message; 
            



        //    }
        //    finally //reset 
        //    {
        //        STP.Current.Thread.Allocator = currentAllocator;

        //    }


        //}



        public void WriteImmutable(ref IIPCMessage message, int size)
        {
            throw new NotImplementedException();
        }

        public Tube Tube
        {
            get { return tube; }
        }


         internal void GetMessage( out IIPCMessage msg)
         {
             msg = message;
         }

        public IIPCMessage Message
        {
            get { return message; }
            set { message = value; }
        } 



        public void WriteImmutable(ref IIPCMessage message)
        {
            throw new NotImplementedException();
        }

        public ICollection<Type> ValidTypes
        {
            get { throw new NotImplementedException(); }
        }
    }
}

