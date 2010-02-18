/*********************************************************************
 *                
 * Copyright (C) 2002-2004, 2007-2009,  Karlsruhe University
 *                
 * File path:     api/v4/ipc.cc
 * Description:   generic IPC path
 *                
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 *                
 * $Id: ipc.cc,v 1.65 2005/01/25 20:25:02 skoglund Exp $
 *                
 ********************************************************************/
#include <debug.h>
#include <kdb/tracepoints.h>

#define HANDLE_LOCAL_IDS

#include INC_API(tcb.h)
#include INC_API(schedule.h)
#include INC_API(ipc.h)
#include INC_API(interrupt.h)
#include INC_API(syscalls.h)
#include INC_API(smp.h)
#include INC_GLUE(syscalls.h)


DECLARE_TRACEPOINT(SYSCALL_IPC);

DECLARE_TRACEPOINT_DETAIL(IPC_TRANSFER);
DECLARE_TRACEPOINT_DETAIL(IPC_DETAILS);
DECLARE_TRACEPOINT_DETAIL(IPC_ERROR);
DECLARE_TRACEPOINT_DETAIL(IPC_XCPU_DETAILS);


INLINE bool transfer_message(tcb_t * src, tcb_t * dst, msg_tag_t tag)
{
    ASSERT(src);
    ASSERT(dst);
    TRACEPOINT (IPC_TRANSFER, "IPC transfer message: src=%t, dst=%t\n", src, dst);
    
    // clear all flags except propagation
    tag.clear_receive_flags();

    /* VU: this copy loop is safe - untyped items can never 
     * exceed the total number of message registers */
    if (tag.get_untyped())
	src->copy_mrs(dst, 1, tag.get_untyped());

    if (EXPECT_TRUE( !tag.get_typed() ))
    {
	// If we have only untyped we know there will be no error.
	// Allow for some gcc optimizations here.
	dst->set_tag(tag);

	if (EXPECT_FALSE (tag.is_propagated ()))
	{
	    // If propagated message transfer was successful we can
	    // set the ActualSender field of the destination, and also
	    // redirect the partner of the virtual sender.  However,
	    // we must make sure that we revalidate the VirtualSender
	    // value since this is a variable that is fetched from the
	    // UTCB (and might therefore have been changed in the
	    // mean time).

	fixup_propagation:

	    tcb_t * virt_sender = src->get_space ()->
		get_tcb (src->get_virtual_sender ());

	    if (src->get_virtual_sender () == virt_sender->get_global_id ()
		&& (src->get_space () == virt_sender->get_space () ||
		    src->get_space () == dst->get_space ())
		&& virt_sender->get_state ().is_waiting ()
		&& virt_sender->get_partner () == src->get_global_id ())
	    {
		virt_sender->set_partner (dst->get_global_id ());
	    }

	    dst->set_actual_sender (src->get_global_id ());
	}

	return true;
    }
    else
    {
	tag = extended_transfer(src, dst, tag);
	dst->set_tag(tag);

	if (EXPECT_FALSE (tag.is_propagated ()))
	{
	    if (EXPECT_TRUE (! tag.is_error ()))
		goto fixup_propagation;
	    dst->set_actual_sender (src->get_global_id ());
	}
	return (! tag.is_error ());
    }
}

#if defined(CONFIG_SMP)

/**********************************************************************
 *                          SMP handlers
 **********************************************************************/

// handler functions to kick remote transfers
static void do_xcpu_receive(cpu_mb_entry_t * entry)
{
    tcb_t * from_tcb = entry->tcb;
    tcb_t * to_tcb = (tcb_t*)entry->param[0];

    TRACE_XIPC_DETAILS("ipc xcpu %s from_tcb: %t (s=%s) to_tcb: %t (s=%s)",
		__func__, from_tcb, from_tcb->get_state().string(),
		to_tcb, to_tcb->get_state().string());

    // did the sender migrate meanwhile?
    if (!from_tcb->is_local_cpu())
	UNIMPLEMENTED();
    
    // still waiting for the IPC?
    if ( (from_tcb->get_state().is_polling() && (from_tcb->get_partner() == to_tcb->get_global_id()) ) ||
	 (to_tcb->get_state().is_locked_waiting() && (to_tcb->get_partner() == from_tcb->get_global_id())) )
    {
	// everything is fine -- now kick the thread
	from_tcb->set_state(thread_state_t::locked_running);
	get_current_scheduler()->enqueue_ready(from_tcb);
    }
    else
	UNIMPLEMENTED();
}

// reply function
static void do_xcpu_send_reply(cpu_mb_entry_t * entry)
{
    // the send operation can start now
    tcb_t * from_tcb = (tcb_t*)entry->tcb;
    TRACE_XIPC_DETAILS("ipc xcpu %s current %t from_tcb: %t (s=%s), result %x",
		       __func__, get_current_tcb(), entry->tcb, entry->tcb->get_state().string(), 
		       entry->param[0]);

    // we can let the thread run
    if (!from_tcb->is_local_cpu())
    {
	TRACE_XIPC_DETAILS("ipc xcpu %s from_tcb: %t (%s) migrated to cpu %d",
		    __func__, entry->tcb, entry->tcb->get_state().string(), from_tcb->get_cpu());
	
	// Forward request
	xcpu_request(from_tcb->get_cpu(), do_xcpu_send_reply, from_tcb, 0);
	return;
    }

    // store status of XCPU request in TCB
    from_tcb->xcpu_status = entry->param[0];

    // and re-activate the thread
    from_tcb->set_state(thread_state_t::locked_running);
    get_current_scheduler()->enqueue_ready(from_tcb);
}

static void do_xcpu_send(cpu_mb_entry_t * entry)
{
    tcb_t * to_tcb = entry->tcb;
    tcb_t * from_tcb = (tcb_t*)entry->param[0];
    threadid_t sender_id; 
    sender_id.set_raw(entry->param[1]);

    ASSERT(to_tcb);
    ASSERT(from_tcb);
    
    TRACE_XIPC_DETAILS("ipc xcpu %s to_tcb: %t (%s), from_tcb: %t (%s)",
		       __func__, to_tcb, to_tcb->get_state().string(), 
		       from_tcb, from_tcb->get_state().string());

    // did the receiver migrate meanwhile?
    if (!to_tcb->is_local_cpu())
    {
	TRACE_XIPC_DETAILS("ipc xcpu %s to_tcb: %t migrated to cpu %d",
			   __func__, to_tcb, to_tcb->get_cpu());
	xcpu_request(from_tcb->get_cpu(), do_xcpu_send_reply, from_tcb, 1);
	return;
    }

    if ( to_tcb->get_state().is_waiting() &&
	 ( to_tcb->get_partner() == sender_id || 
	   to_tcb->get_partner().is_anythread() ))
    {
	// ok, still waiting --> everything is fine
	get_current_scheduler()->cancel_timeout(to_tcb);
	to_tcb->set_state(thread_state_t::locked_waiting);
	to_tcb->set_partner(sender_id);

	// now let the other thread run again
	xcpu_request(from_tcb->get_cpu(), do_xcpu_send_reply, from_tcb, 0);
    }
    else if (to_tcb->get_state().is_locked_waiting() &&
	     to_tcb->get_partner() == sender_id)
    {
	// ok, we are locked_waiting -- means we already issued
	// a request packet (do_xcpu_receive) -- so don't bother
	TRACE_XIPC_DETAILS("ipc xcpu %s %t is locked_waiting for %t", 
			   __func__, to_tcb, TID(sender_id));
    }
    else
    {
	TRACE_XIPC_DETAILS("ipc xcpu %s (not waiting) to_tcb: %t (%s), from_tcb: %t",
		     __func__, to_tcb, to_tcb->get_state().string(), from_tcb);
	xcpu_request(from_tcb->get_cpu(), do_xcpu_send_reply, from_tcb, 1);
    }
}

static void do_xcpu_send_done(cpu_mb_entry_t * entry)
{
    tcb_t * to_tcb = entry->tcb;
    threadid_t sender_id; 
    sender_id.set_raw(entry->param[0]);
    
    TRACE_XIPC_DETAILS("ipc xcpu %s to_tcb: %t (%s)",
		       __func__, to_tcb, to_tcb->get_state().string());

    // did the receiver migrate meanwhile?
    if (!to_tcb->is_local_cpu())
    { 	
	TRACE_XIPC_DETAILS("ipc xcpu %s to_tcb: %t migrated to cpu %d",
		    __func__, to_tcb, to_tcb->get_cpu());
	// Forward request
	xcpu_request( to_tcb->get_cpu(), do_xcpu_send_done, to_tcb, sender_id.get_raw());
	return;
    }

    if ( to_tcb->get_state().is_locked_waiting() && 
	 to_tcb->get_partner() == sender_id )
    {
	msg_tag_t tag = to_tcb->get_tag();
	tag.set_xcpu();
	to_tcb->set_tag(tag);
	to_tcb->set_state(thread_state_t::running);
	get_current_scheduler()->enqueue_ready(to_tcb);
    }
    else
    {
	TRACEF("tcb=%t, state=%s, partner=%t, %t\n", 
	       TID(to_tcb->get_global_id()), to_tcb->get_state().string(),
	       TID(to_tcb->get_partner()), TID(sender_id));
	UNIMPLEMENTED();
    }
}

#endif /* CONFIG_SMP */


/**********************************************************************
 *
 *                          IPC syscall
 *
 **********************************************************************/

SYS_IPC (threadid_t to_tid, threadid_t from_tid, timeout_t timeout)
{
    tcb_t * to_tcb = NULL;
    tcb_t * from_tcb;
    tcb_t * current = get_current_tcb();
    scheduler_t * scheduler = get_current_scheduler();
    msg_tag_t tag = current->get_tag();

   
    TRACEPOINT (SYSCALL_IPC, 
		"SYS_IPC: %t->%t (<-%t), to: %x, t: %x (l=0x%x, u=%d, t=%d)",
		current, TID(to_tid), TID(from_tid), timeout.raw,
		current->get_tag().raw, current->get_tag().get_label(), 
		current->get_tag().get_untyped(), current->get_tag().get_typed());

    /* --- send phase --------------------------------------------------- */
#if defined(CONFIG_SMP)
send_path:
#endif

    if (! EXPECT_FALSE( to_tid.is_nilthread() ))
    {

	to_tcb = current->get_space()->get_tcb(to_tid);
	TRACE_IPC_DETAILS("ipc send phase curr=%t, to=%t", current, TID(to_tid));

	

	threadid_t sender_id = current->get_global_id();

	
	    // propagation only allowed within same address space
	 

#if defined(CONFIG_SMP)	
	/* VU: set the thread state before actually checking 
	 * the partner. Allows for concurrent checks on SMP */
	current->set_partner(to_tid);
	current->set_state(thread_state_t::polling);
	
	if ( to_tcb->lock_state.is_active() ) 
	    to_tcb->lock();
#endif

	// not waiting || (not waiting for me && not waiting for any && not waiting for anylocal)
	// optimized for receive and wait any
	if (EXPECT_FALSE(
		(!to_tcb->get_state().is_waiting())  ||
		(   // Not waiting for sender (may be virtual sender)?
		    to_tcb->get_partner() != sender_id &&
		    // Not open wait?
		    !to_tcb->get_partner().is_anythread() &&
		    // Not open local wait?
		    !(to_tcb->get_partner().is_anylocalthread() && 
		      to_tcb->get_space() == current->get_space()) &&
		    // Not waiting for actual sender (if propagating IPC)?
		    to_tcb->get_partner() != current->get_global_id()   )))
	{
	    TRACE_IPC_DETAILS("ipc blocking send (curr=%t, to=%t s=%s)", 
			      current, TID(to_tid), to_tcb->get_state().string());
	    //enter_kdebug("blocking send");

	    /* thread is not receiving */
	    if (EXPECT_FALSE( !timeout.get_snd().is_never() ))
	    {
		if (timeout.get_snd().is_zero())
		{
		    TRACE_IPC_ERROR("ipc zero send timeout (curr=%t, to=%t)", current, TID(to_tid));
		    /* VU: set thread state to running - in case we 
		     * had a long IPC. Not on the critical path */
		    current->set_state(thread_state_t::running);
		    current->set_tag(msg_tag_t::error_tag());
		    current->set_error_code(IPC_SND_ERROR(ERR_IPC_TIMEOUT));
		    to_tcb->unlock();
		    return_ipc(NILTHREAD);
		}
		TRACE_IPC_DETAILS("ipc setting timeout %dus", timeout.get_snd().get_microseconds());
		scheduler->set_timeout(current, timeout.get_snd());
	    }
#ifdef CONFIG_SMP
	    if (!to_tcb->lock_state.is_active()) 
		to_tcb->lock();
#endif
	    
	    current->enqueue_send(to_tcb);
	    to_tcb->unlock();
	    current->set_partner(to_tid);
	    current->set_state(thread_state_t::polling);
	    current->switch_to_idle();
	    
	    // got re-activated -- start IPC now
	    // make sure we dequeue ourselfs from the wakeup list
	    scheduler->cancel_timeout(current);
	    to_tcb->lock();	
	    current->dequeue_send(to_tcb); 
	    // we re-acquire the lock and need to release for sure
	}
#if defined(CONFIG_SMP)	
	else if (EXPECT_FALSE( !to_tcb->is_local_cpu() && !to_tcb->lock_state.is_enabled() ))
	{
    
	    TRACE_XIPC_DETAILS("ipc xcpu send %t:%d (%s) -> %t:%d (%s)", 
			       current, current->get_cpu(), current->get_state().string(),
			       to_tcb, to_tcb->get_cpu(), to_tcb->get_state().string());
	    
	    // receiver seems to be waiting -- try to send
	    xcpu_request( to_tcb->get_cpu(), do_xcpu_send, 
			  to_tcb, (word_t)current, sender_id.get_raw());

	    // at this stage we are already polling...
	    current->switch_to_idle();

	    // re-activated?
	    TRACE_XIPC_DETAILS("ipc xcpu got reactivated after waiting to send %t:%d (%s) -> %t:%d (%s) result %d", 
			       current, current->get_cpu(), current->get_state().string(),
			       to_tcb, to_tcb->get_cpu(), to_tcb->get_state().string(), current->xcpu_status);
	    
	    // something happened -- retry sending
	    if (current->xcpu_status != 0)
	    {
#if 0
		// zero-timeout XCPU don't retry -- should be more generic
		// use absolute timeout on start of first round and check when
		// coming back
		if ( timeout.get_snd().is_zero() )
		{
		    current->set_state(thread_state_t::running);
		    current->set_tag(msg_tag_t::error_tag());
		    current->set_error_code(IPC_SND_ERROR(ERR_IPC_TIMEOUT));
		    return_ipc(NILTHREAD);
		}
#endif
		TRACE_XIPC_DETAILS("ipc xcpu send failed, retry to send %t:%d (%s) -> %t:%d (%s)", 
				   current, current->get_cpu(), current->get_state().string(),
				   to_tcb, to_tcb->get_cpu(), to_tcb->get_state().string());
	    

		goto send_path;
	    }
	    // ... fall through and perform send
	}
#endif

	// The partner must be told who the IPC originated from.
	ASSERT(!tag.is_propagated() || sender_id.is_global());
	to_tcb->set_partner(sender_id);

	if (EXPECT_FALSE( !transfer_message(current, to_tcb, tag) ))
	{
	    /* error on transfer - activate the partner and return */
	    current->set_tag(to_tcb->get_tag());
	    current->set_state(thread_state_t::running);
	    to_tcb->set_state(thread_state_t::running);
	    to_tcb->unlock();

	    if (EXPECT_TRUE( to_tcb->is_local_cpu() ))
		scheduler->enqueue_ready(to_tcb);
#ifdef CONFIG_SMP
	    else 
		scheduler->remote_enqueue_ready(to_tcb);
#endif
	    return_ipc(to_tid);
	}

#if defined(CONFIG_SMP)
	if (EXPECT_FALSE( !to_tcb->is_local_cpu() ))
	{
	    if ( to_tcb->lock_state.is_enabled() )
	    {
		// lock-based remote IPC
		msg_tag_t tag = to_tcb->get_tag();
		tag.set_xcpu();
		to_tcb->set_tag(tag);
		
		if (to_tcb->get_saved_partner ().is_nilthread ())
		    to_tcb->set_state(thread_state_t::running);
		else
		    // Receiver had a nested IPC.
		    to_tcb->set_state(thread_state_t::locked_running_ipc_done);
		scheduler->remote_enqueue_ready(to_tcb);
	    } 
	    else 
	    {
		// RPC-based remote IPC
		/* VU: kick receiver and forget about him
		 * we have to transmit the sender id since it is
		 * going to change in the receive path!!! */
		TRACE_XIPC_DETAILS("ipc xcpu notify on send done %t:%d (%s) -> %t:%d (%s)",
				   current, current->get_cpu(), current->get_state().string(),
				   to_tcb, to_tcb->get_cpu(), to_tcb->get_state().string());
	    
		//UNIMPLEMENTED();
		xcpu_request( to_tcb->get_cpu(), do_xcpu_send_done, 
			      to_tcb, sender_id.get_raw());
	    }
	    to_tcb->unlock();
	    // make sure we are running before potentially exiting to user
	    current->set_state(thread_state_t::running);
	    to_tcb = NULL;
	} else
	    to_tcb->unlock();
#endif
    }

    /* --- send finished ------------------------------------------------ */
    TRACE_IPC_DETAILS("ipc send finished curr=%t to=%t from_tid %x", current, to_tcb, TID(from_tid));

    if (EXPECT_FALSE( from_tid.is_nilthread() ))
    {
	/* this case is entered on:
	 *   - send-only case
	 *   - both descriptors set to nil id 
	 * in the SMP case to_tcb is always NULL! */
	if (to_tcb != NULL)
	{
	    ASSERT(to_tcb->is_local_cpu());

	    if (to_tcb->get_saved_partner ().is_nilthread ())
		to_tcb->set_state(thread_state_t::running);
	    else
		// Receiver had a nested IPC.
		to_tcb->set_state(thread_state_t::locked_running_ipc_done);

	    current->set_state(thread_state_t::running);
	    if (scheduler->check_dispatch_thread (current, to_tcb) )
	    {
		scheduler->enqueue_ready(current);
		current->switch_to(to_tcb);
	    }
	    else
	    {
		// hack hack hack
		scheduler->cancel_timeout(to_tcb);
		scheduler->enqueue_ready(to_tcb);
	    }
	}
	return_ipc(from_tid);
    }
    /* --- receive phase ------------------------------------------------ */
    else /* ! from_tid.is_nilthread() */
    {
	TRACE_IPC_DETAILS("ipc receive phase curr=%t, from=%t", current, TID(from_tid));

#if defined(CONFIG_SMP)
	/* VU: set thread state early to catch races */
	current->set_partner(from_tid);
	current->set_state(thread_state_t::waiting_forever);
	if (current->lock_state.is_active())
	    current->lock();
#endif

	/* VU: optimize for common case -- any, closed, anylocal */
	if (from_tid.is_anythread())
	{
	    from_tcb = current->send_head;
	}
	else if (EXPECT_TRUE( !from_tid.is_anylocalthread() ))
	{
	    /* closed wait */
	    ASSERT(from_tid.is_global());
	    from_tcb = current->get_space()->get_tcb(from_tid);


	    TRACE_IPC_DETAILS("ipc closed wait from %t, current=%t", TID(from_tid), current);

	    if (EXPECT_FALSE( (from_tcb->get_global_id() != from_tid) &&
			      ( (from_tcb->get_space() != current->get_space()) ||
				(from_tcb->get_local_id() != from_tid) ) ))
	    {
		/* wrong receiver id */
		TRACE_IPC_ERROR("ipc invalid receiver id (curr=%t, from=%t)", current, TID(from_tid));
		current->set_tag(msg_tag_t::error_tag());
		current->set_error_code(IPC_RCV_ERROR(ERR_IPC_NON_EXISTING));
		ON_CONFIG_SMP(current->set_state(thread_state_t::running));
		current->unlock();
		return_ipc(NILTHREAD);
	    }
	}
	else
	{
	    /* anylocal */
	    tcb_t *head = current->send_head;
	    from_tcb = NULL;

#if defined(CONFIG_SMP)
	    TRACEF("from == anylocal requires SMP sanity check");
	    enter_kdebug("UNTESTED");
#endif
	    if (head)
	    {
	        tcb_t *tcb = head;
	  
	        do {
		    if (tcb->get_space () == current->get_space ())
		    {
		        from_tcb = tcb;
			break;
		    }
		    tcb = tcb->send_list.next;
		} while (tcb != head);
	    }
	}
	
	/*
	 * no partner || partner is not polling || 
	 * partner doesn't poll on me
	 */
	if( EXPECT_TRUE ( (from_tcb == NULL) || 
			  (!from_tcb->get_state().is_polling()) ||
			  ( (from_tcb->get_partner() != current->get_global_id()) &&
			    (from_tcb->get_partner() != current->myself_local) )) )
	{
	    TRACE_IPC_DETAILS("ipc blocking receive (curr=%t, from=%t)", current, TID(from_tid));

	    /* partner is not trying to send to me */
	    if (EXPECT_FALSE( !timeout.get_rcv().is_never() ))
	    {
		/* prepare the IPC error */
		current->set_error_code(IPC_RCV_ERROR(ERR_IPC_TIMEOUT));

		if ( timeout.get_rcv().is_zero() )
		{
		    TRACE_IPC_ERROR("ipc receive error (curr=%t, from=%t)", current, TID(from_tid));
		    current->set_tag(msg_tag_t::error_tag());
		    current->set_state(thread_state_t::running);
		    current->unlock();
		    /* zero timeout and partner not ready --> 
		     * we have to perform timeslice donation */
		    if (to_tcb != NULL)
		    {
			scheduler->enqueue_ready(current);
			current->switch_to(to_tcb);
		    }
		    return_ipc(NILTHREAD);
		}
		TRACE_IPC_DETAILS("ipc setting timeout %dus",  timeout.get_rcv().get_microseconds());
		scheduler->set_timeout(current, timeout.get_rcv());
		current->set_state(thread_state_t::waiting_timeout);
	    }
	    else
		current->set_state(thread_state_t::waiting_forever);
	    	    
	    /* VU: should we convert to a global id here??? */
	    current->set_partner(from_tid);

	    if (EXPECT_FALSE(to_tcb == NULL)) 
		to_tcb = get_idle_tcb();

#ifdef CONFIG_SMP
	    ASSERT(to_tcb->is_local_cpu());
#endif	
	    current->unlock();
	    current->switch_to(to_tcb);
	    
#if defined(HANDLE_LOCAL_IDS)
	    from_tcb = current->get_partner_tcb ();
#endif

	    /* VU: if a timeout occurs the wakeup handling will set
	     * the error bit accordingly. Hence, we can directly
	     * return from the IPC without additional checking
	     * here. */

	    TRACE_IPC_DETAILS("ipc %t received msg from %t (virtual %t)", current, current->get_partner_tcb());

	    /* XXX VU: restructure switching code so that dequeueing 
	     * from wakeup is removed from critical path */
	    scheduler->cancel_timeout(current);
	}
	else
	{
	    TRACE_IPC_DETAILS("ipc perform receive from %t",  from_tcb);
	    //enter_kdebug("do receive");

	    // both threads on the same CPU?
	    if (EXPECT_TRUE( from_tcb->is_local_cpu() ))
	    {
		/* partner is ready to send */
		from_tcb->set_state(thread_state_t::locked_running);
		current->set_state(thread_state_t::locked_waiting);
		current->unlock();

		/* Switch to waiting partner.
		 * If we do not switch to woken up we have to dequeue him
		 * from the wakeup queue to make sure the IPC does not 
		 * timeout meanwhile... 
		 */
		if ( to_tcb != NULL)
		{
		    // Preserve priorities when choosing threads to activate,
		    // not on the critical path.
		    if ( scheduler->check_dispatch_thread ( from_tcb, to_tcb ) )
		    {
			scheduler->cancel_timeout(from_tcb);
			scheduler->enqueue_ready(from_tcb);
			current->switch_to(to_tcb);
		    }
		    else
		    {
			to_tcb->set_state( thread_state_t::running );
			scheduler->enqueue_ready(to_tcb);
			current->switch_to(from_tcb);
		    }
		}
		else
		    current->switch_to(from_tcb);
	    }
#if defined(CONFIG_SMP)
	    else 
	    {
		TRACE_XIPC_DETAILS("ipc xcpu receive curr=%t:%d -> from=%t:%d",
				   current, current->get_cpu(), from_tcb, from_tcb->get_cpu());
		
		current->set_state(thread_state_t::locked_waiting);

		if (EXPECT_TRUE (current->lock_state.is_enabled())) 
		{
		    from_tcb->set_state(thread_state_t::locked_running);
		    // remote enqueue into ready
		    scheduler->remote_enqueue_ready(from_tcb);
		} 
		else
		    xcpu_request(from_tcb->get_cpu(), do_xcpu_receive, from_tcb, (word_t)current);
		
		current->unlock();

		if (!to_tcb) to_tcb = get_idle_tcb();
		    
		current->switch_to(to_tcb);
		TRACE_XIPC_DETAILS("ipc xcpu receive done (from=%t, curr=%t)\n", from_tcb, current);
	    }
#endif
	}
	current->set_state(thread_state_t::running);
#if defined(HANDLE_LOCAL_IDS)
	// only global TIDs are passed through propagation, hence the test
	// against from_tcb->get_global_id().
	if (current->get_space () == from_tcb->get_space ()
	    && current->get_partner() == from_tcb->get_global_id())
	{
	    return_ipc (from_tcb->get_local_id ());
	}
#endif
	return_ipc(current->get_partner());
    }

    // this case should never happen
    enter_kdebug("ipc fall-through - why?");
    spin_forever();
}
