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
    EDI = EAX
    // Save EIP of the break
    .AsmBreakEIP = EDI

    // Save the old byte
    AX = EDI[0]
    .AsmOrigByte = AX

    // Inject INT3
	// Do in 2 steps to force a byte move to RAM (till X# can do byte in one step)
	AX = $CC
    EDI[0] = AX
}

function ClearAsmBreak {
    EDI = .AsmBreakEIP
    // If 0, we don't need to clear an older one.
    if EDI = 0 return
    
	// Clear old break point and set back to original opcode / partial opcode
    AX = .AsmOrigByte
    EDI[0] = AX

    .AsmBreakEIP = 0
}

