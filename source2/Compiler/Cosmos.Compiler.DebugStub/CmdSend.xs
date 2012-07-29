namespace DebugStub

function SendRegisters {
	// Send the actual started signal
    AL = #Ds2Vs_Registers 
	ComWriteAL()

    ESI = .PushAllPtr
    ECX = 32
    ComWriteX()
    
	ESI = @.CallerESP
    ComWrite32()
    
	ESI = @.CallerEIP
    ComWrite32()
}

function SendFrame {
    AL = #Ds2Vs_Frame
    ComWriteAL()

    EAX = 32
    ComWriteAX()

    ESI = .CallerEBP
	// Dont transmit EIP or old EBP
    ESI + 8
    ECX = 32
    ComWriteX()
}

function SendStack {
    AL = #Ds2Vs_Stack
    ComWriteAL()

    // Send size of bytes
    ESI = .CallerESP
    EAX = .CallerEBP
    EAX - ESI
    ComWriteAX()

    // Send actual bytes
    //
    // Need to reload ESI, WriteAXToCompPort modifies it
    ESI = .CallerESP
    while ESI != .CallerEBP {
		ComWrite8()
	}
}

// sends a stack value
// Serial Params:
//  1: x32 - offset relative to EBP
//  2: x32 - size of data to send
function SendMethodContext {
	+All

    AL = #Ds2Vs_MethodContext
    ComWriteAL()

    // offset relative to ebp
    // size of data to send
    ComReadEAX()
    ECX = EAX
    ComReadEAX()

    // now ECX contains size of data (count)
    // EAX contains relative to EBP
    ESI = .CallerEBP
    ESI + EAX

	while ECX != 0 {
		ComWrite8()
		ECX--
	}

Exit:
	-All
}

// none
// saveregs
// frame
//
// sends a stack value
// Serial Params:
//  1: x32 - offset relative to EBP
//  2: x32 - size of data to send
function SendMemory {
	+All

    ComReadEAX()
    ECX = EAX
    AL = #Ds2Vs_MemoryData
    ComWriteAL()

    ComReadEAX()
    ESI = EAX

    // now ECX contains size of data (count)
    // ESI contains address
	while ECX != 0 {
		ComWrite8()
		ECX--
	}

Exit:
	-All
}

// Modifies: EAX, ESI
function SendTrace {
    AL = #Ds2Vs_BreakPoint
	// If we are running, its a tracepoint, not a breakpoint.
	// In future, maybe separate these into 2 methods
	if dword .DebugStatus = #Status_Run {
	    AL = #Ds2Vs_TracePoint
	}
    ComWriteAL()

    // Send Calling EIP.
    ESI = @.CallerEIP
    ComWrite32()
}

// Input: Stack
// Output: None
// Modifies: EAX, ECX, EDX, ESI
function SendText {
	// Write the type
    AL = #Ds2Vs_Message
    ComWriteAL()

    // Write Length
    ESI = EBP
    ESI + 12
    ECX = ESI[0]
    ComWrite16()

    // Address of string
    ESI = EBP[8]
WriteChar:
    if ECX = 0 return
    ComWrite8()
    ECX--
    // We are storing as 16 bits, but for now I will transmit 8 bits
    // So we inc again to skip the 0
    ESI++
    goto WriteChar
}

// Input: Stack
// Output: None
// Modifies: EAX, ECX, EDX, ESI
function SendPtr {
    // Write the type
    AL = #Ds2Vs_Pointer
    ComWriteAL()

    // pointer value
    ESI = EBP[8]
    ComWrite32()
}
