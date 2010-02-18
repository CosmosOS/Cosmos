using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoreLib.IPC.IPC.Messaging.Minix
{

    /// <summary>
    /// Minix doesnt translate well since its single threaded and hence has no locks etc durring iPC 
    /// 
    /// Still its mature and using IPC with states is worth a try
    /// </summary>
    public class MinixMessageReader : IIPCMessageReader
    {


//        //===========================================================================*
//    // *				mini_receive				     * 
//    // *===========================================================================
////C++ TO C# CONVERTER NOTE: 'register' variable declarations are not supported in C#:
////ORIGINAL LINE: static int mini_receive(caller_ptr, src, m_ptr, flags) register struct proc *caller_ptr;
//    static int mini_receive(caller_ptr, src, m_ptr, flags) struct proc *caller_ptr = new static(); // process trying to get message 
//    int src; // which message source is wanted 
//    message m_ptr; // pointer to message buffer 
//    uint flags; // system call flags 
//    {
//    // A process or task wants to get a message.  If a message is already queued,
//    // * acquire it and deblock the sender.  If no message from the desired source
//    // * is available block the caller, unless the flags don't allow blocking.  
//    // 
////C++ TO C# CONVERTER TODO TASK: C# does not allow declaring types within methods:
//    // register struct proc **xpp;
//    //  register struct notification **ntf_q_pp;
//    //  message m;
//    //  int bit_nr;
//    //  sys_map_t *map;
//    //  ushort *chunk;
//    //  int i, src_id, src_proc_nr;
//    //
//    ////   Check to see if a message from desired source is already available.
//    ////   * The caller's SENDING flag may be set if SENDREC couldn't send. If it is
//    ////   * set, the process should be blocked.
//    ////   
//    //  if (!(caller_ptr->p_rts_flags & DefineConstantsProc.SENDING))
//    //  {
//    //
//    //	// Check if there are pending notifications, except for SENDREC. 
//    //	if (! (((caller_ptr)->p_priv)->s_flags & DefineConstantsProc.SENDREC_BUSY))
//    //	{
//    //
//    //		map = &((caller_ptr)->p_priv)->s_notify_pending;
//    //		for (chunk=&map->chunk[0]; chunk<&map->chunk[NR_SYS_CHUNKS]; chunk++)
//    //		{
//    //
//    //			// Find a pending notification from the requested source.  
//    //			if (! *chunk) // no bits in chunk 
//    //				continue;
//    //			for (i=0; ! (*chunk & (1<<i)); ++i) // look up the bit 
//    //			{
//    //			}
//    //			src_id = (chunk - &map->chunk[0]) * BITCHUNK_BITS + i;
//    //			if (src_id >= DefineConstantsProc._NR_SYS_PROCS) // out of range 
//    //				break;
//    //			src_proc_nr = (ppriv_addr)[(src_id)]->s_proc_nr;

//    //			if (src!=DefineConstantsProc.ANY && src!=src_proc_nr) // source not ok 
//    //				continue;
//    //			*chunk &= ~(1 << i); // no longer pending 
//    //
//    //			// Found a suitable source, deliver the notification message. 
//    //		(&m)->m_source = (src_proc_nr);
//    //		(&m)->m_type = (DefineConstantsProc.NOTIFY_MESSAGE | ((src_proc_nr) + DefineConstantsProc.NR_TASKS));
//    //		(&m)->m_u.m_m2.m2l2 = get_uptime();
//    //		switch (src_proc_nr)
//    //		{
//    //			case -1:
//    //				(&m)->m_u.m_m2.m2l1 = ((caller_ptr)->p_priv)->s_int_pending;
//    //				((caller_ptr)->p_priv)->s_int_pending = 0;
//    //				break;
//    //				case -2:
//    //					(&m)->m_u.m_m2.m2l1 = ((caller_ptr)->p_priv)->s_sig_pending;
//    //					((caller_ptr)->p_priv)->s_sig_pending = 0;
//    //					break;
//    //		};
//    //			cp_mess(src_proc_nr, ((pproc_addr + DefineConstantsProc.NR_TASKS)[(-1)])->p_memmap[DefineConstantsProc.D].mem_phys, (uint)&m, (caller_ptr)->p_memmap[DefineConstantsProc.D].mem_phys, (uint)m_ptr);
//    //			return(OK); // report success 
//    //		}
//    //	}
//    //
//    //	// Check caller queue. Use pointer pointers to keep code simple. 
//    //	xpp = &caller_ptr->p_caller_q;
//    //	while (*xpp != (struct proc *) 0)
//    //	{
//    //		if (src == DefineConstantsProc.ANY || src == ((*xpp)->p_nr))
//    //		{
//    //		// Found acceptable message. Copy it and update status. 
//    //		cp_mess((*xpp)->p_nr, (*xpp)->p_memmap[DefineConstantsProc.D].mem_phys, (uint)(*xpp)->p_messbuf, (caller_ptr)->p_memmap[DefineConstantsProc.D].mem_phys, (uint)m_ptr);
//    //			if (((*xpp)->p_rts_flags &= ~DefineConstantsProc.SENDING) == 0)
//    //				enqueue(*xpp);
//    //			*xpp = (*xpp)->p_q_link; // remove from queue 
//    //			return(OK); // report success 
//    //	}
//    //	xpp = &(*xpp)->p_q_link; // proceed to next 
//    //	}
//    //  }

//    //   No suitable message is available or the caller couldn't send in SENDREC. 
//    //   * Block the process trying to receive, unless the flags tell otherwise.
//    //   
//      if (! (flags & NON_BLOCKING))
//      {
//          caller_ptr.p_getfrom = src;
//          caller_ptr.p_messbuf = m_ptr;
//          if (caller_ptr.p_rts_flags == 0)
//              dequeue(caller_ptr);
//          caller_ptr.p_rts_flags |= DefineConstantsProc.RECEIVING;
//          return(OK);
//      }
//      else
//      {
//          return(ENOTREADY);
//      }
//    }


        Tube tube;
        MinixMessageWriter writer;
        MinixIPCExtention destinationSTP;
       // IAllocator destinationSTPAllocator;

        public MinixMessageReader(Tube tube)
        {
            this.tube = tube;
            this.writer = new MinixMessageWriter(tube , this);
            tube.Destination.IPCSTPExtention = new MinixIPCExtention(tube.Destination); //TODO set in reader 
         //   destinationSTPAllocator = tube.Destination.Thread.Allocator;
            destinationSTP = tube.Destination.IPCSTPExtention as MinixIPCExtention;

        }


        public ProcessModel.STP SenderEndPoint
        {
            get { throw new NotImplementedException(); }
        }

        internal IIPCMessageWriter Writer
        {
            get { return writer; }
        }

      


        //IIPCMessageWriter IIPCMessageReader.Writer
        //{
        //    get { throw new NotImplementedException(); }
        //}


        public void ReceiveMessage(out IIPCMessage message)
        {
            IPCMessageHolder holder; 
            ReceiveMessage(out holder);
            message = holder.Message;
        }


        //All foobared
        public void ReceiveMessage(out IPCMessageHolder message)
        {
           // if (destinationSTP.Message.Sender != null)
            {
                message = destinationSTP.Message;
            }

            // pop queue into message
            try
            {
            destinationSTP.Message = destinationSTP.ReceiveQueue.Dequeue();
          
            }
            catch (Exception ex) 
            {
                //TODO type do nothing if queue empty
            }

            destinationSTP.State = IPCState.BUSY; 

        }
    }
}
