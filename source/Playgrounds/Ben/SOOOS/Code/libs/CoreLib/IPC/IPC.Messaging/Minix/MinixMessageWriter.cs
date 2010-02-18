using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib.ProcessModel;
using CoreLib.GC;

namespace CoreLib.IPC.IPC.Messaging.Minix
{
    /// <summary>
    /// Minix style 
    /// 
    /// Much closer interaction with scheduler , lock /interlock free on single thread CPUs
    /// 
    /// NOTE only works on single threaded CPU like minix. 
    /// 
    /// 
    /// Not fully asynch esp if the destination is slow or lower priority
    /// </summary>
    public class MinixMessageWriter : IIPCMessageWriter
    {
        Tube tube;
        MinixMessageReader reader;
        MinixIPCExtention destinationSTP;
        IAllocator destinationSTPAllocator;

        System.Threading.EventWaitHandle waitHandle;  //yes directly changing the scheduler is faster but we will see.

        public MinixMessageWriter( Tube tube , MinixMessageReader reader)
        {
            this.tube = tube;
            this.reader = reader;
            tube.Destination.IPCSTPExtention = new MinixIPCExtention(tube.Destination); //TODO set in reader 
            destinationSTPAllocator = tube.Destination.Thread.Allocator;
            destinationSTP = tube.Destination.IPCSTPExtention as MinixIPCExtention; 

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

        public void WriteStruct(ref IIPCMessage message, int size)
        {
            SendStructSync(ref message);
        }


        [Inline]
        public void SendStructSync(ref IIPCMessage message)
        {
            if (DestinationIsWaiting())
            {
                CopyMessageToRemoteGC(ref message);
                destinationSTP.ScheduleDestination();                 
            }
            else
            {

                var callerExtention = this.tube.Source.IPCSTPExtention as MinixIPCExtention;
                callerExtention.State = IPCState.SENDING;

             

                CopyMessageToRemoteGC(ref message, true);

                if (waitHandle.WaitOne(3000) == false)
                {
                    ; 
                    //TODO panic ? log possible deadlock
                }

                // Destination is not waiting.  Block and dequeue caller. 
                //        caller_ptr.p_messbuf = m_ptr;
                //        if (caller_ptr.p_rts_flags == 0)
                //            dequeue(caller_ptr);
                //        caller_ptr.p_rts_flags |= DefineConstantsProc.SENDING;
                //        caller_ptr.p_sendto = dst;



                //process is blocked waiting  on the receiver to clear all messages so 


                //        // Process is now blocked.  Put in on the destination's queue. 
                //        xpp = &dst_ptr.p_caller_q; // find end of list 
                //        while (*xpp != (proc) 0)
                //            xpp = &(*xpp).p_q_link;
                //        *xpp = caller_ptr; // add caller to end 
                //        caller_ptr.p_q_link = (proc) 0; // mark new end of list 
            }

        }


        [Inline]
        bool DestinationIsWaiting()
        {
            if (destinationSTP.State == IPCState.RECEIVING)
                return true;
            else
                return false; 
        }



        /// <summary>
        /// Async via retries true for success  else false
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Inline]
        public bool SendStructAsync(ref IIPCMessage message)
        {
            if (DestinationIsWaiting())
            {
                CopyMessageToRemoteGC(ref message);
                destinationSTP.ScheduleDestination(); 
                return true;
            }
            else
            {
                return false; 
            }
        }



        [Inline]
        void CopyMessageToRemoteGC(ref IIPCMessage message)
        {
            CopyMessageToRemoteGC(ref message, false);
        }


        /// <summary>
        /// TODO would be nice in to pass in the destination but this is a bit painfull
        /// </summary>
        /// <param name="message"></param>
        /// <param name="enqueue"></param>
        [Inline]
        void CopyMessageToRemoteGC(ref IIPCMessage message , bool enqueue)
        {
            if (STP.Current.Thread.Allocator.Collector.State == GC.CollectionState.PendingCollect)
            {
                //todo wait on collect and retry.
                ;
            }

            IAllocator currentAllocator = STP.Current.Thread.Allocator;
            try
            {
                STP.Current.Thread.Allocator = destinationSTPAllocator;

                // We are on the destinations GC this will copy the values in 
                //TODO check assembly
                var holder =  new IPCMessageHolder(tube , message);


                if (enqueue)
                    destinationSTP.Message = holder;
                else
                    destinationSTP.ReceiveQueue.Enqueue(holder); //TODO passes by copy
             


            }
            finally //reset 
            {
                STP.Current.Thread.Allocator = currentAllocator;

            }


        }


  //       case SENDREC:
  //    /* A flag is set so that notifications cannot interrupt SENDREC. */
  //    priv(caller_ptr)->s_flags |= SENDREC_BUSY;
  //    /* fall through */
  //case SEND:			
  //    result = mini_send(caller_ptr, src_dst, m_ptr, flags);
  //    if (function == SEND || result != OK) {	
  //        break;				/* done, or SEND failed */
  //    }						/* fall through for SENDREC */
  //case RECEIVE:			
  //    if (function == RECEIVE)
  //        priv(caller_ptr)->s_flags &= ~SENDREC_BUSY;
  //    result = mini_receive(caller_ptr, src_dst, m_ptr, flags);
  //    break;

        
//    //===========================================================================*
//    // *				mini_send				     * 
//    // *===========================================================================
////C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//    //static int mini_send(caller_ptr NamelessParameter1, dst NamelessParameter2, m_ptr NamelessParameter3, flags NamelessParameter4) register struct proc *caller_ptr; // who is trying to send a message? 
//    public static int dst; // to whom is message being sent? 
//    public static message m_ptr; // pointer to message buffer 
//    public static uint flags; // system call flags 
//    {
//    // Send a message from 'caller_ptr' to 'dst'. If 'dst' is blocked waiting
//    // * for this message, copy the message to it and unblock 'dst'. If 'dst' is
//    // * not waiting at all, or is waiting for another source, queue 'caller_ptr'.
//    // 
//      public static register struct proc *dst_ptr = (pproc_addr + DefineConstantsProc.NR_TASKS)[(dst)];
//      public static register struct proc **xpp = new register();

//    //   Check if 'dst' is blocked waiting for this message. The destination's 
//    //   * SENDING flag may be set when its SENDREC call blocked while sending.  
//    //   
////C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
//      if ((dst_ptr.p_rts_flags & (DefineConstantsProc.RECEIVING | DefineConstantsProc.SENDING)) == DefineConstantsProc.RECEIVING && (dst_ptr.p_getfrom == DefineConstantsProc.ANY || dst_ptr.p_getfrom == caller_ptr.p_nr))
//      {
//        // Destination is indeed waiting for this message. 
//        cp_mess(caller_ptr.p_nr, (caller_ptr).p_memmap[DefineConstantsProc.D].mem_phys, (uint)m_ptr, (dst_ptr).p_memmap[DefineConstantsProc.D].mem_phys, (uint)dst_ptr.p_messbuf);
//        if ((dst_ptr.p_rts_flags &= ~DefineConstantsProc.RECEIVING) == 0)
//            enqueue(dst_ptr);
//      }
////C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
//      else if (! (flags & NON_BLOCKING))
//      {
//        // Destination is not waiting.  Block and dequeue caller. 
//        caller_ptr.p_messbuf = m_ptr;
//        if (caller_ptr.p_rts_flags == 0)
//            dequeue(caller_ptr);
//        caller_ptr.p_rts_flags |= DefineConstantsProc.SENDING;
//        caller_ptr.p_sendto = dst;

//        // Process is now blocked.  Put in on the destination's queue. 
//        xpp = &dst_ptr.p_caller_q; // find end of list 
//        while (*xpp != (proc) 0)
//            xpp = &(*xpp).p_q_link;
//        *xpp = caller_ptr; // add caller to end 
//        caller_ptr.p_q_link = (proc) 0; // mark new end of list 
//      }
//      else
//      {
//        return(ENOTREADY);
//      }
//      return(OK);
//    }

        public Tube Tube
        {
            get { throw new NotImplementedException(); }
        }


    
        public ICollection<Type> ValidTypes
        {
            get { throw new NotImplementedException(); }
        }

        public void WriteStruct(ref IIPCMessage message)
        {
            throw new NotImplementedException();
        }

        public void WriteImmutable(ref IIPCMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
