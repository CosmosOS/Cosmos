Group DebugStub

// Caller's Registers
//var CallerEBP
//var CallerEIP
//var CallerESP

// Tracing: 0=Off, 1=On
//var TraceMode
// enum Status
//var DebugStatus
// Pointer to the push all data. It points to the bottom after PushAll.
// Walk up to find the 8 x 32 bit registers.
//var PushAllPtr
// If set non 0, on next trace a break will occur
//var DebugBreakOnNextTrace
// For step out and over this is used to determine where the initial request was made
// EBP is logged when the trace is started and can be used to determine 
// what level we are "at" relative to the original step start location.
//var BreakEBP
// Command ID of last command received
//var CommandID

// Sets a breakpoint
// Serial Params:
//   1: x32 - EIP to break on, or 0 to disable breakpoint.
function BreakOnAddress {
	+All
    // BP Address
    ComReadEAX()
    ECX = EAX

    // BP ID Number
    // BP ID Number is sent after BP Address, becuase
    // reading BP address uses AL (EAX).
    EAX = 0
    ComReadAL()

    // Calculate location in table
    // Mov [EBX + EAX * 4], ECX would be better, but our X# doesn't handle this yet
    EBX = @..DebugBPs
    EAX << 2
    EBX + EAX

    EBX[0] = ECX
Exit:
	-All
}

function Executing2 {
	// This is the secondary stub routine. After the primary has decided we should do some debug
	// activities, this one is called.
	// Each of these checks a flag, and if it processes then it jumps to .Normal.
    // CheckForAsmBreak must coe before CheckForBreakpoint. They could exist for the same EIP.
//    CheckForAsmBreak()
//    CheckForBreakpoint()
//    // Only one of the following can be active at a time.
//    CheckStepF11()
//    CheckStepF10()
//    CheckStepShiftF11()

Normal:

    // If tracing is on, send a trace message.
    // Tracing isnt really used any more, was used by the old stand alone debugger. Might be upgraded
    // and resused in the future.
	//if .TraceMode = #Tracing_On {
		SendTrace()
	//}

    // Is there a new incoming command? We dont want to wait for one
    // if there isn't one already here. This is a non blocking check.
CheckForCmd:
//    DX = (ushort)(0x3F8 + 5u)
    AL = Port[DX]
    AL ?& 1
    // If a command is waiting, process it and then check for another.
    // If no command waiting, break from loop.
	//if !0 {
		ProcessCommand()
		// See if there are more commands waiting
		goto CheckForCmd
	//}
}

//  void CheckForBreakpoint() {
//    // Look for a possible matching BP
//    // TODO: This is slow on every Int3...
//    //   -Find a faster way - a list of 256 straight compares and code modifation?
//    //   -Count BPs and modify ECX since we usually dont have 256 of them?
//    //   -Move this scan earlier - Have to set a global flag when anything (StepTriggers, etc below) is going on at all
//    //     A selective disable of the DS
//    //   -If there are 0 BPs, skip scan - easy and should have a good increase
//    EAX = CallerEIP.Value
//    EDI = AddressOf("DebugBPs")
//    ECX = 256
//    new Scas { Prefixes = InstructionPrefixes.RepeatTillEqual, Size = 32 }
//    JumpIf(Flags.NotEqual, ".AfterBreakOnAddress")
//    Call<Break>()
//    Jump(".Normal")

//    Label = ".AfterBreakOnAddress"
//  }

//  void CheckStepF10() {
//    DebugBreakOnNextTrace.Value.Compare(StepTrigger.Over)
//    JumpIf(Flags.NotEqual, ".StepOverAfter")
        
//    EAX = CallerEBP.Value
//    EAX.Compare(BreakEBP.Value)
//    // If EBP and start EBP arent equal, dont break
//    // Dont use Equal because we also need to stop above if the user starts
//    // the step at the end of a method and next item is after a return
//    Call<Break>(Flags.LessThanOrEqualTo)
//    Jump(".Normal")

//    Label = ".StepOverAfter"
//  }

//  void CheckStepF11() {
//    DebugBreakOnNextTrace.Value.Compare(StepTrigger.Into)
//    //TODO: I think we can use a using statement to create this type of block
//    // and emit asm
//    // using (var xBlock = new AsmBlock()) {
//    //   JumpIf(something, xBlock.End/Begin)
//    //   also can do xBlock.Break()
//    // }
//    //TODO: If statements can probably be done with anonymous delegates...
//    JumpIf(Flags.NotEqual, ".StepIntoAfter")
        
//    Call<Break>()
//    //TODO: Allow creating labels but issuing them later, then we can call them with early binding
//    //TODO: End - can be exit label for each method, allowing Jump(Begin/End) etc... Also make a label type and allwo Jump overload to the label itself. Or better yet, End.Jump()
//    Jump(".Normal")
        
//    Label = ".StepIntoAfter"
//  }

//  void CheckStepShiftF11() {
//    DebugBreakOnNextTrace.Value.Compare(StepTrigger.Out)
//    JumpIf(Flags.NotEqual, ".StepOutAfter")

//    EAX = CallerEBP.Value
//    EAX.Compare(BreakEBP.Value) // TODO: X# JumpIf(EAX == Memory[...... or better yet if(EAX==Memory..., new Delegate { Jump.... Jump should be handled specially so we dont jump around jumps... TODO: Also allow Compare(EAX, 0), in fact force this new syntax
//    JumpIf(Flags.Equal, ".Normal")
        
//    CallIf(Flags.LessThanOrEqualTo, "DebugStub_Break")
//    Jump(".Normal")
        
//    Label = ".StepOutAfter"
//  }

//  void CheckForAsmBreak() {
//    EAX = CallerEIP.Value
//    // AsmBreakEIP is 0 when disabled, but EIP can never be 0 so we dont need a separate check.
//    Call("DebugStub_HackCompareAsmBreakEIP")
//    JumpIf(Flags.NotEqual, ".AsmBreakAfter")

//    Call("DebugStub_ClearAsmBreak")
//    Call<Break>()
//    Jump(".Normal")

//    Label = ".AsmBreakAfter"
//  }


//function Break {
//  // Should only be called internally by DebugStub. Has a lot of preconditions.
//  // Externals should use BreakOnNextTrace instead.
//  public override void Assemble() {
//    // Reset request in case we are currently responding to one or we hit a fixed breakpoint
//    // before our request could be serviced (if one existed)
//    DebugBreakOnNextTrace.Value = StepTrigger.None
//    BreakEBP.Value = 0
//    // Set break status
//    DebugStatus.Value = Status.Break
//    Call("DebugStub_SendTrace")

//    // Wait for a command
//    Label = ".WaitCmd"
//    {
//      // Check for common commands first
//      Call("DebugStub_ProcessCommand")

//      // Now check for commands that are only valid in break state or commands that require special handling while in break state.

//      AL.Compare(Vs2Ds.Continue)
//      JumpIf(Flags.Equal, ".Done")

//      {
//        AL.Compare(Vs2Ds.SetAsmBreak)
//        JumpIf(Flags.NotEqual, ".SetAsmBreak_After")
//        Call("DebugStub_SetAsmBreak")
//        Jump(".WaitCmd")
//        Label = ".SetAsmBreak_After"
//      }

//      {
//        AL.Compare(Vs2Ds.StepInto)
//        JumpIf(Flags.NotEqual, ".StepInto_After")
//        DebugBreakOnNextTrace.Value = StepTrigger.Into
//        Jump(".Done")
//        Label = ".StepInto_After"
//      }

//      {
//        AL.Compare(Vs2Ds.StepOver)
//        JumpIf(Flags.NotEqual, ".StepOver_After")
//        DebugBreakOnNextTrace.Value = StepTrigger.Over
//        EAX = CallerEBP.Value
//        BreakEBP.Value = EAX
//        Jump(".Done")
//        Label = ".StepOver_After"
//      }

//      {
//        AL.Compare(Vs2Ds.StepOut)
//        JumpIf(Flags.NotEqual, ".StepOut_After")
//        DebugBreakOnNextTrace.Value = StepTrigger.Out
//        EAX = CallerEBP.Value
//        BreakEBP.Value = EAX
//        Jump(".Done")
//        Label = ".StepOut_After"
//      }

//      // Loop around and wait for another command
//      Jump(".WaitCmd")
//    }

//    Label = ".Done"
//    Call("DebugStub_AckCommand")
//    DebugStatus.Value = Status.Run
//  }
//}

