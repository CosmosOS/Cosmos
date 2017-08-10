namespace DebugStub

// Location where INT3 has been injected.
// 0 if no INT3 is active.
var AsmBreakEIP

// Old byte before INT3 was injected.
// Only 1 byte is used.
var AsmOrigByte

function DoAsmBreak {
	// Since our Int3 is temp, we need to adjust return EIP to return to it, not after it.
	ESI = .CallerESP
	EAX = .AsmBreakEIP
	ESI[-12] = EAX

	ClearAsmBreak()
  Break()
}

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

function SetINT1_TrapFLAG {
	//Push EAX to make sure whatever we do below doesn't affect code afterwards
	+EBP
	+EAX

	//Set base pointer to the caller ESP
	EBP = .CallerESP
	
	//Set the Trap Flag (http://en.wikipedia.org/wiki/Trap_flag)
	//For EFLAGS we want - the interrupt frame = ESP + 12
	//					 - The interrupt frame - 8 for correct byte = ESP + 12 - 8 = ESP + 4
	//					 - Therefore, ESP - 4 to get to the correct position
	EBP - 4
	EAX = [EBP]
	EAX | $0100
	[EBP] = EAX

	//Restore the base pointer
	
	//Pop EAX - see +EAX at start of method
	-EAX
	-EBP
}

function ResetINT1_TrapFLAG {
	//Push EAX to make sure whatever we do below doesn't affect code afterwards
	+EBP
	+EAX

	//Set base pointer to the caller ESP
	EBP = .CallerESP
	
	//Clear the Trap Flag (http://en.wikipedia.org/wiki/Trap_flag)
	//See comment in SetINT1_TrapFlag
	EBP - 4
	EAX = [EBP]
	EAX & $FEFF
	[EBP] = EAX
	
	//Pop EAX - see +EAX at start of method
	-EAX
	-EBP
}