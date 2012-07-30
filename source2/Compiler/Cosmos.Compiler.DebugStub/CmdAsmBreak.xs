namespace DebugStub

// Location where INT3 has been injected.
// 0 if no INT3 is active.
var AsmBreakEIP

// Old byte before INT3 was injected.
// Only 1 byte is used.
var AsmOrigByte

function SetAsmBreak {
	ClearAsmBreak()

    ComReadEAX()
    // Save EIP of the break
    .AsmBreakEIP = EAX
    EDI = EAX

    // Save the old byte
    AL = EDI[0]
    .AsmOrigByte = AL

    // Inject INT3
	// Do in 2 steps to force a byte move to RAM (till X# can do byte in one step)
	AL = $CC
    EDI[0] = AL
}

function ClearAsmBreak {
    EDI = .AsmBreakEIP
    // If 0, we don't need to clear an older one.
    if EDI = 0 return
    
	// Clear old break point and set back to original opcode / partial opcode
    AL = .AsmOrigByte
    EDI[0] = AL

    .AsmBreakEIP = 0
}

