namespace DebugStub

// Caller's Registers
var CallerEBP
var CallerEIP
var CallerESP

// Tracing: 0=Off, 1=On 
var TraceMode
// enum Status
var DebugStatus
// Pointer to the push all data. It points to the bottom after PushAll.
// Walk up to find the 8 x 32 bit registers.
var PushAllPtr
// If set non 0, on next trace a break will occur
var DebugBreakOnNextTrace
// For step out and over this is used to determine where the initial request was made
// EBP is logged when the trace is started and can be used to determine 
// what level we are "at" relative to the original step start location.
var BreakEBP
// Command ID of last command received
var CommandID

// Sets a breakpoint
// Serial Params:
//   1: x32 - EIP to break on, or 0 to disable breakpoint.
function BreakOnAddress {
	+All
    // BP Address
    ComReadEAX()
    ECX = EAX

    // BP ID Number
    // BP ID Number is sent after BP Address, because
    // reading BP address uses AL (EAX).
    EAX = 0
    ComReadAL()

    // Calculate location in table
    // Mov [EBX + EAX * 4], ECX would be better, but our X# doesn't handle this yet
    EBX = @.DebugBPs
    EAX << 2
    EBX + EAX

    EBX[0] = ECX
Exit:
	-All
}

function Executing {
	// This is the secondary stub routine. After the primary has decided we should do some debug
	// activities, this one is called.
	// Each of these checks a flag, and if it processes then it jumps to .Normal.

	//Check whether this call is result of (i.e. after) INT1
	 ! MOV EAX, DR6
	 EAX & $4000
	 if EAX = $4000 {
	   //This was INT1

	   //Reset the debug register
	   EAX ^ $4000
	   ! MOV DR6, EAX
	   
	   ResetINT1_TrapFLAG()

	   Break()
	   goto Normal
	 }

    // CheckForAsmBreak must come before CheckForBreakpoint. They could exist for the same EIP.
	// Check for asm break
    EAX = .CallerEIP
    // AsmBreakEIP is 0 when disabled, but EIP can never be 0 so we dont need a separate check.
	if EAX = .AsmBreakEIP {
		DoAsmBreak()
  		goto Normal
	}

	// Check for breakpoint
    // Look for a possible matching BP
    // TODO: This is slow on every Int3...
    //   -Find a faster way - a list of 256 straight compares and code modifation?
    //   -Count BPs and modify ECX since we usually dont have 256 of them?
    //   -Move this scan earlier - Have to set a global flag when anything (StepTriggers, etc below) is going on at all
    //     A selective disable of the DS
    //   -If there are 0 BPs, skip scan - easy and should have a good increase
    EAX = .CallerEIP
    EDI = @.DebugBPs
    ECX = 256
	! repne scasd
	if = {
		Break()
		goto Normal
	}

    // Only one of the following can be active at a time (F10, F11, ShiftF11)

	// F11 - Must check first
	// If F11, stop on next C# line that executes.
    if dword .DebugBreakOnNextTrace = #StepTrigger_Into {
		Break()
		goto Normal
	}

	// .CallerEBP is the stack on method entry.
	EAX = .CallerEBP
	
	// F10
    if dword .DebugBreakOnNextTrace = #StepTrigger_Over {
		// If EAX = .BreakEBP then we are in same method.
		// If EAX > .BreakEBP then our method has returned and we are in the caller.
		if EAX >= .BreakEBP {
			Break()
		}
		goto Normal
	}

	// Shift-F11
    if dword .DebugBreakOnNextTrace = #StepTrigger_Out {
		// If EAX > .BreakEBP then our method has returned and we are in the caller.
		if EAX > .BreakEBP {
			Break()
		}
		goto Normal
	}

Normal:
    // If tracing is on, send a trace message.
    // Tracing isnt really used any more, was used by the old stand alone debugger. Might be upgraded
    // and resused in the future.
	if dword .TraceMode = #Tracing_On {
		SendTrace()
	}

    // Is there a new incoming command? We dont want to wait for one
    // if there isn't one already here. This is a non blocking check.
CheckForCmd:
	DX = .ComAddr
	DX + 5
    AL = Port[DX]
    AL ?& 1
    // If a command is waiting, process it and then check for another.
    // If no command waiting, break from loop.
	if !0 {
		ProcessCommand()
		// See if there are more commands waiting
		goto CheckForCmd
	}
}

function Break {
    // Should only be called internally by DebugStub. Has a lot of preconditions.
    // Externals should use BreakOnNextTrace instead.
	//
    // Reset request in case we are currently responding to one or we hit a fixed breakpoint
    // before our request could be serviced (if one existed)
    .DebugBreakOnNextTrace = #StepTrigger_None
    .BreakEBP = 0
    // Set break status
    .DebugStatus = #Status_Break
    SendTrace()

    // Wait for a command
WaitCmd:
    // Check for common commands first
    ProcessCommand()

    // Now check for commands that are only valid in break state or commands that require special handling while in break state.

    if AL = #Vs2Ds_Continue goto Done

	//If Asm step into, we need to continue execution
	if AL = #Vs2Ds_AsmStepInto {
		goto Done
	}

    if AL = #Vs2Ds_SetAsmBreak {
        SetAsmBreak()
	    AckCommand()
	    goto WaitCmd
	}
	
    if AL = #Vs2Ds_StepInto {
        .DebugBreakOnNextTrace = #StepTrigger_Into
		// Not used, but set for consistency
        .BreakEBP = EAX
	    goto Done
	}

    if AL = #Vs2Ds_StepOver {
        .DebugBreakOnNextTrace = #StepTrigger_Over
        EAX = .CallerEBP
        .BreakEBP = EAX
	    goto Done
	}

    if AL = #Vs2Ds_StepOut {
        .DebugBreakOnNextTrace = #StepTrigger_Out
        EAX = .CallerEBP
        .BreakEBP = EAX
	    goto Done
	}

    // Loop around and wait for another command
    goto WaitCmd

Done:
    AckCommand()
    .DebugStatus = #Status_Run
}

