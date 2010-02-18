//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace CoreLib.IPC
//{

//    //Pipe 
//    //    ReceiveChannel
//    //    SendChannel 



//    public class IPCManager
//    {

//        IList<IMessagePipe> pipes; //incomming

//        IMessagePipe ReceiveMessage() // block receive
//        {

//            //Set Wait event 




//              // aquire STP IPC lock 
//                // if no messages waiting ( interlocked field) 

//            // release block 
//            throw new NotImplementedException(); 
//        }

//        IMessagePipe ReceiveAllAsynch() //non block receive
//        {
//            throw new NotImplementedException();
//        }





    
//        // maybe implement in future but replaced by using direct reply ..
//        // eg create an AutoResetWaitHandle which gets set when you timeout or the receiver sets it ( by doing the set value property) on the Reply message..

//        // if message is IReplyMessage 
//        // send message (We are Asynch remember) 
//        // set AutoResetWaitHandle
        
//        // message receiver calls AutoResetWaitHandle Repply delegate ( note its remote code ) when reply is called the system will copy the message back on top of the original message. 
        
//        // if ( waithandle timeout)
//            //TimeoutError
//        // We now have our value in the reply field of the message ( via Copy) 


//        // when sender calls reply() it will call send   
//            //SendAndWake ( ref KernelMessage send ,waithandle)
//            // adds reply message via normal send
//            // sets the wait handle!



//        //public void GetWOrkFromChannel(ref IPCMessageHolder holder , IMessagePipe pipe)
//        //{

//        //    pipe.GetResetEvent(); 
//        //    // As get work but will only check one Channel

//        //    // mark channel as busy  ( this will prevent a sender adding)

//        //    // check for work on queue

//        //    // mark Channel as waiting  safe for sender to add
//        //    // go to sleep
//        //}

//        /// <summary>
//        /// While not very CPU efficient 
//        /// any multiple producer will normally require a lock which is much more expensive 
//        /// this is a lockless and doesnt use an interlock in the most common cases.
//        /// 
//        /// it becomes more efficient the more work there is 
//        /// especially since we use no locks 
//        /// 
//        /// There are fareness issues here as apps lower in the pipes collections will get served more often 
//        /// this can act as a priority scheme. eg when changing the collection resort on priority
//        /// 
//        /// 
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        /// 

//        ///OPTION consider a single queue and measure results
//        public void  GetWork(out IPCMessageHolder holder)
//        {
        
          
           
//            // check for message 
//            foreach (var messagePipe in pipes)
//            {
//                var msg = messagePipe.Messages.Dequeue(); //lockless check

//                if (msg != null)
//                {

//                    holder.Channel = messagePipe;
//                    holder.Message = msg;
//                    return;                   
//                }
//            }
//            // how to handle message arrived here ! 

//            //no messages 
//            var pipe = ReceiveMessage(); // block waiting 
            
//            // get off pipe 

//        //    var msg = ReceiveAll(out channel); // wait for message
//          //  holder.Message = msg;
//            //holder.Channel = channel;  

        

//        }


//        //all send is on channel .

//        // if user waiting on that channel deliver else queue on that channel. 

//        // if waiting wake up - it knows that channel has the information.

//        // funnel all channels into a single source ?? Then we need to wrap each one .. Sounds good.


//        // so when sending a struct unboxed ( ie stack - copy it to a boxed object in a message holder and set the channel ( who did the send) ) 

//        // queus in stack , if not waiting just add to queue for that channel. 

//        //GetMessageFromQueue on Pipe can wrap it .. the OS just stores it bare in the pipe

//        // How to handle getAny...
//        // By answering true to isWaiting..  Iswaiting must be a bit more complex so it can be set for all queues. 

//        // where to pass the message to for an IsWaitignAny ... Kernel queue = 0 ?? ! 

//        // no need for queue for isWaiting any ? Can we lock ?
        
//        // waitany 
//            //when it hits returnes the pipe which the code can then read

//        //what about wait any then wait xx for the api.. 


//        //if pipe waiting 
//        //deliver direct 
//        //else
//            //add to queue
//            //if deliver  all set value with Interlock exchanged ( compare and exchannge) 
//            // push ref for  IPipe on the stack ? 


//        //after calling a waitany all messages shoudl be processed since wait any will check all queues. 

//        //non blocking receive...will block when there is now work?

////00300 
////00301   /* Check if 'dst' is blocked waiting for this message. The destination's 
////00302    * SENDING flag may be set when its SENDREC call blocked while sending.  
////00303    */
////00304   if ( (dst_ptr->p_rts_flags & (RECEIVING | SENDING)) == RECEIVING &&
////00305        (dst_ptr->p_getfrom_e == ANY
////00306          || dst_ptr->p_getfrom_e == caller_ptr->p_endpoint)) {
////00307         /* Destination is indeed waiting for this message. */
////00308         CopyMess(caller_ptr->p_nr, caller_ptr, m_ptr, dst_ptr,
////00309                  dst_ptr->p_messbuf);
////00310         if ((dst_ptr->p_rts_flags &= ~RECEIVING) == 0) enqueue(dst_ptr);
////00311   } else if ( ! (flags & NON_BLOCKING)) {
////00312         /* Destination is not waiting.  Block and dequeue caller. */
////00313         caller_ptr->p_messbuf = m_ptr;
////00314         if (caller_ptr->p_rts_flags == 0) dequeue(caller_ptr);
////00315         caller_ptr->p_rts_flags |= SENDING;
////00316         caller_ptr->p_sendto_e = dst_e;
////00317 
////00318         /* Process is now blocked.  Put in on the destination's queue. */
////00319         xpp = &dst_ptr->p_caller_q;             /* find end of list */
////00320         while (*xpp != NIL_PROC) xpp = &(*xpp)->p_q_link;       
////00321         *xpp = caller_ptr;                      /* add caller to end */
////00322         caller_ptr->p_q_link = NIL_PROC;        /* mark new end of list */
////00323   } else {
////00324         return(ENOTREADY);
////00325   }
////00326   return(OK);
////00327 }
////00328 



////00329 /*===========================================================================*
////00330  *                              mini_receive                                 * 
////00331  *===========================================================================*/
////00332 PRIVATE int mini_receive(caller_ptr, src_e, m_ptr, flags)
////00333 register struct proc *caller_ptr;       /* process trying to get message */
////00334 int src_e;                              /* which message source is wanted */
////00335 message *m_ptr;                         /* pointer to message buffer */
////00336 unsigned flags;                         /* system call flags */
////00337 {
////00338 /* A process or task wants to get a message.  If a message is already queued,
////00339  * acquire it and deblock the sender.  If no message from the desired source
////00340  * is available block the caller, unless the flags don't allow blocking.  
////00341  */
////00342   register struct proc **xpp;
////00343   register struct notification **ntf_q_pp;
////00344   message m;
////00345   int bit_nr;
////00346   sys_map_t *map;
////00347   bitchunk_t *chunk;
////00348   int i, src_id, src_proc_nr, src_p;
////00349 
////00350   if(src_e == ANY) src_p = ANY;
////00351   else
////00352   {
////00353         okendpt(src_e, &src_p);
////00354         if (proc_addr(src_p)->p_rts_flags & NO_ENDPOINT) return ESRCDIED;
////00355   }
////00356 
////00357 
////00358   /* Check to see if a message from desired source is already available.
////00359    * The caller's SENDING flag may be set if SENDREC couldn't send. If it is
////00360    * set, the process should be blocked.
////00361    */
////00362   if (!(caller_ptr->p_rts_flags & SENDING)) {
////00363 
////00364     /* Check if there are pending notifications, except for SENDREC. */
////00365     if (! (caller_ptr->p_misc_flags & REPLY_PENDING)) {
////00366 
////00367         map = &priv(caller_ptr)->s_notify_pending;
////00368         for (chunk=&map->chunk[0]; chunk<&map->chunk[NR_SYS_CHUNKS]; chunk++) {
////00369 
////00370             /* Find a pending notification from the requested source. */ 
////00371             if (! *chunk) continue;                     /* no bits in chunk */
////00372             for (i=0; ! (*chunk & (1<<i)); ++i) {}      /* look up the bit */
////00373             src_id = (chunk - &map->chunk[0]) * BITCHUNK_BITS + i;
////00374             if (src_id >= NR_SYS_PROCS) break;          /* out of range */
////00375             src_proc_nr = id_to_nr(src_id);             /* get source proc */
////00376 #if DEBUG_ENABLE_IPC_WARNINGS
////00377             if(src_proc_nr == NONE) {
////00378                 kprintf("mini_receive: sending notify from NONE\n");
////00379             }
////00380 #endif
////00381             if (src_e!=ANY && src_p != src_proc_nr) continue;/* source not ok */
////00382             *chunk &= ~(1 << i);                        /* no longer pending */
////00383 
////00384             /* Found a suitable source, deliver the notification message. */
////00385             BuildMess(&m, src_proc_nr, caller_ptr);     /* assemble message */
////00386             CopyMess(src_proc_nr, proc_addr(HARDWARE), &m, caller_ptr, m_ptr);
////00387             return(OK);                                 /* report success */
////00388         }
////00389     }
////00390 
////00391     /* Check caller queue. Use pointer pointers to keep code simple. */
////00392     xpp = &caller_ptr->p_caller_q;
////00393     while (*xpp != NIL_PROC) {
////00394         if (src_e == ANY || src_p == proc_nr(*xpp)) {
////00395 #if 0
////00396             if ((*xpp)->p_rts_flags & SLOT_FREE)
////00397             {
////00398                 kprintf("listening to the dead?!?\n");
////00399                 return EINVAL;
////00400             }
////00401 #endif
////00402 
////00403             /* Found acceptable message. Copy it and update status. */
////00404             CopyMess((*xpp)->p_nr, *xpp, (*xpp)->p_messbuf, caller_ptr, m_ptr);
////00405             if (((*xpp)->p_rts_flags &= ~SENDING) == 0) enqueue(*xpp);
////00406             *xpp = (*xpp)->p_q_link;            /* remove from queue */
////00407             return(OK);                         /* report success */
////00408         }
////00409         xpp = &(*xpp)->p_q_link;                /* proceed to next */
////00410     }
////00411   }
////00412 
////00413   /* No suitable message is available or the caller couldn't send in SENDREC. 
////00414    * Block the process trying to receive, unless the flags tell otherwise.
////00415    */
////00416   if ( ! (flags & NON_BLOCKING)) {
////00417       caller_ptr->p_getfrom_e = src_e;          
////00418       caller_ptr->p_messbuf = m_ptr;
////00419       if (caller_ptr->p_rts_flags == 0) dequeue(caller_ptr);
////00420       caller_ptr->p_rts_flags |= RECEIVING;             
////00421       return(OK);
////00422   } else {
////00423       return(ENOTREADY);
////00424   }
////00425 }
////00426 


//  //      112 PRIVATE void get_work(m_ptr)
//  //113 message *m_ptr;                         /* message buffer */
//  //114 {
//  //115     int status = 0;
//  //116     status = receive(ANY, m_ptr);   /* this blocks until message arrives */
//  //117     if (OK != status)
//  //118         panic("DS","failed to receive message!", status);
//  //119     who = m_ptr->m_source;        /* message arrived! set sender */
//  //120     callnr = m_ptr->m_type;       /* set function call number */
//  //121 }


//  //   message m;
//  // 36   int result;                 
//  // 37   sigset_t sigset;
//  // 38 
//  // 39   /* Initialize the server, then go to work. */
//  // 40   init_server(argc, argv);
//  // 41 
//  // 42   /* Main loop - get work and do it, forever. */         
//  // 43   while (TRUE) {              
//  // 44 
//  // 45       /* Wait for incoming message, sets 'callnr' and 'who'. */
//  // 46       get_work(&m);
//  // 47 
//  // 48       switch (callnr) {
//  // 49       case SYS_SIG:
//  // 50           sigset = (sigset_t) m.NOTIFY_ARG;
//  // 51           if (sigismember(&sigset,SIGTERM) || sigismember(&sigset,SIGKSTOP)) {
//  // 52               exit_server();
//  // 53           }
//  // 54           continue;
//  // 55       case DS_PUBLISH:
//  // 56           result = do_publish(&m);
//  // 57           break;
//  // 58       case DS_RETRIEVE:
//  // 59           result = do_retrieve(&m);
//  // 60           break;
//  // 61       case DS_SUBSCRIBE:
//  // 62           result = do_subscribe(&m);
//  // 63           break;
//  // 64       case GETSYSINFO:
//  // 65           result = do_getsysinfo(&m);
//  // 66           break;
//  // 67       default: 
//  // 68           report("DS","warning, got illegal request from:", m.m_source);
//  // 69           result = EINVAL;
//  // 70       }
//  // 71 
//  // 72       /* Finally send reply message, unless disabled. */
//  // 73       if (result != EDONTREPLY) {
//  // 74           m.m_type = result;            /* build reply message */
//  // 75           reply(who, &m);               /* send it away */
//  // 76       }
//  // 77   }
//  // 78   return(OK);      

//    }
//}
