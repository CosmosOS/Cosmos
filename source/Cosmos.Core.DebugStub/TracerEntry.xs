// We need to make sure Int3 can never run more than one instance at a time.
// We are not threaded yet, when we are we have to change stuff to thread vars and a lot of other stuff.
// Two Int3 calls currently can never happen at the same time normally, but IRQs can happen while the DebugStub is
// running. We also need to make sure IRQs are allowed to run during DebugStub as DebugStub can wait for
// a long time on commands.
// So we need to disable interrupts immediately and set a flag, then reenable interrupts if they were enabled
// when we disabled them. Later this can be replaced by some kind of critical section / lock around this code.
// Currently IRQs are disabled - we need to fix DS before we can reenable them and add support for critical sections / locks here.
// -http://www.codemaestro.com/reviews/8
// -http://en.wikipedia.org/wiki/Spinlock - Uses a register which is a problem for us
// -http://wiki.osdev.org/Spinlock
//   -Looks good and also allows testing intead of waiting
//   -Wont require us to disable / enable IRQs

//This method also handles INT1

namespace DebugStub

Interrupt TracerEntry {
// This code is temporarily disabled as IRQs are not enabled right now.
// LockOrExit

// First, disable interrupts, so debugging is much more stable
! cli


	+All
// Save current ESP so we can look at the results of PushAll later
.PushAllPtr = ESP
.CallerEBP = EBP

// Get current ESP and add 32. This will skip over the PushAll and point
// us at the call data from Int3.
EBP = ESP
EBP + 32
// Caller EIP
EAX = EBP[0]

// 12 bytes for EFLAGS, CS, EIP
EBP + 12
.CallerESP = EBP

// EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.
// Note - when we used call it was 5 (size of our call + address)
// so we get the EIP as IL2CPU records it. Its also useful for when we
// wil be changing ops that call this stub.

//Check whether this call is result of (i.e. after) INT1. If so, don't subtract 1!
EBX = EAX
! MOV EAX, DR6
EAX & $4000
if EAX != $4000 {
	EBX--
}
EAX = EBX

// Store it for later use.
.CallerEIP = EAX

	Executing()

-All

// restore interupts
! sti

// Temp disabled, see comment on LockOrExit above
// Unlock
}
