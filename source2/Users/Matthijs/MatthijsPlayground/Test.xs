# From Entry.CS

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

# Get current ESP and add 32. This will skip over the PushAll and point us at the call data from Int3.
EBP = ESP
EBP + 32

# Caller EIP
EAX = EBP[0]

# EIP is pointer to op after our call. Int3 is 1 byte so we subtract 1.
# Note - when we used call it was 5 (the size of our call + address)
# so we get the EIP as IL2CPU records it. Its also useful for when we will
# be changing ops that call this stub.
EAX - 1

# Store it for later use.
CallerEIP = EAX
