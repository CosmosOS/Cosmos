namespace DebugStub

// Location where INT3 has been injected.
// 0 if no INT3 is active.
var AsmBreakEIP

// Old byte before INT3 was injected.
// Only 1 byte is used.
var AsmOrigByte

function SetAsmBreak {
    ComReadEAX()
    EDI = EAX
    // Save the old byte (as dword for now)
    EAX = EDI[0]
    .AsmOrigByte = EAX
    // Inject INT3
    EDI[0] = $CC
    // Save EIP of the break
    .AsmBreakEIP = EDI
}

function ClearAsmBreak {
    EDI = .AsmBreakEIP
    // If 0, we don't need to clear an older one.
    if EDI = 0 return
    
	// Clear old break point and set back to original opcode / partial opcode
    EAX = .AsmOrigByte
    EDI[0] = EAX
    .AsmOrigByte = 0
}

