Group DebugStub

# Location where INT3 has been injected.
# 0 if no INT3 is active.
var AsmBreakEIP

# Old byte before INT3 was injected.
# Only 1 byte is used.
var AsmOrigByte

procedure SetAsmBreak {
    Call .ComReadEAX
    EDI = EAX
    # Save the old byte
    EAX = EDI[0]
    .AsmOrigByte = EAX
    # Inject INT3
    EDI[0] = $CC
    # Save EIP of the break
    .AsmBreakEIP = EDI
}

procedure ClearAsmBreak {
    EDI = .AsmBreakEIP
    # If 0, we don't need to clear an older one.
    If (EDI = 0) exit
    
	# Clear old break point and set back to original opcode / partial opcode
    EAX = .AsmOrigByte
    EDI[0] = EAX
    .AsmOrigByte = 0
}

