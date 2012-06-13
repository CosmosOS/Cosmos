# From Entry.CS

# We need to make sure Int3 can never run more than one instance at a time.
# We are not threaded yet, when we are we have to change stuff to thread vars and a lot of other stuff.
# Two Int3 calls currently can never happen at the same time normally, but IRQs can happen while the DebugStub is
# running. We also need to make sure IRQs are allowed to run during DebugStub as DebugStub can wait for
# a long time on commands.
# So we need to disable interrupts immediately and set a flag, then reenable interrupts if they were enabled
# when we disabled them. Later this can be replaced by some kind of critical section / lock around this code.
# Currently IRQs are disabled - we need to fix DS before we can reenable them and add support for critical sections / locks here.
# -http:#www.codemaestro.com/reviews/8
# -http:#en.wikipedia.org/wiki/Spinlock - Uses a register which is a problem for us
# -http:#wiki.osdev.org/Spinlock
#   -Looks good and also allows testing intead of waiting
#   -Wont require us to disable / enable IRQs

Group DebugStub

InterruptHandler TracerEntry {
	# This code is temporarily disabled as IRQs are not enabled right now.
	# LockOrExit

	# EBP is restored by PopAll, but SendFrame uses it. Could
	# get it from the PushAll data, but this is easier.
	CallerEBP = EBP

	# Could get ESP from PushAll but this is easier.
	# Also allows us to use the stack before PushAll if we ever need it.

	# We cant modify any registers since we havent done PushAll yet 
	# Maybe we could do a sub(4) on memory direct.. 
	# But for now we remove from ESP which the Int3 produces,
	# store ESP, then restore ESP so we don't cause stack corruption.
	# 12 bytes for EFLAGS, CS, EIP
	ESP + 12
	CallerESP = ESP
	ESP - 12

	PushAll

	# Save current ESP so we can look at the results of PushAll later
	PushAllPtr = ESP

	# Get current ESP and add 32. This will skip over the PushAll and point
	# us at the call data from Int3.
	EBP = ESP
	EBP + 32

	# Caller EIP
	EAX = [EBP + 0]

	# EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.
	# Note - when we used call it was 5 (size of our call + address)
	# so we get the EIP as IL2CPU records it. Its also useful for when we
	# wil be changing ops that call this stub.
	EAX - 1

	# Store it for later use.
	CallerEIP = EAX

	Call .Executing

	PopAll

	# Temp disabled, see comment on LockOrExit above
	# Unlock

#IRet
}
