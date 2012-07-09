Group DebugStub

const Vs2Ds_Noop = 0
const Vs2Ds_TraceOff = 1
const Vs2Ds_TraceOn = 2
const Vs2Ds_Break = 3
const Vs2Ds_Continue = 4
const Vs2Ds_BreakOnAddress = 6
const Vs2Ds_BatchBegin = 7
const Vs2Ds_BatchEnd = 8
const Vs2Ds_StepInto = 5
const Vs2Ds_StepOver = 11
const Vs2Ds_StepOut = 12
const Vs2Ds_SendMethodContext = 9
const Vs2Ds_SendMemory = 10
const Vs2Ds_SendRegisters = 13
const Vs2Ds_SendFrame = 14
const Vs2Ds_SendStack = 15
const Vs2Ds_Ping = 17 
const Vs2Ds_Max = 18

const Ds2Vs_Noop = 0
const Ds2Vs_TracePoint = 1
const Ds2Vs_Message = 2
const Ds2Vs_BreakPoint = 3
const Ds2Vs_Error = 4
const Ds2Vs_Pointer = 5
const Ds2Vs_Started = 6
const Ds2Vs_MethodContext = 7
const Ds2Vs_MemoryData = 8
const Ds2Vs_CmdCompleted = 9
const Ds2Vs_Registers = 10
const Ds2Vs_Frame = 11
const Ds2Vs_Stack = 12
const Ds2Vs_Pong = 13

procedure AckCommand {
    // We acknowledge receipt of the command AND the processing of it.
    //   -In the past the ACK only acknowledged receipt.
    // We have to do this because sometimes callers do more processing.
    // We ACK even ones we dont process here, but do not ACK Noop.
    // The buffers should be ok because more wont be sent till after our NACK
    // is received.
    // Right now our max cmd size is 2 (Cmd + Cmd ID) + 5 (Data) = 7. 
    // UART buffer is 16.
    // We may need to revisit this in the future to ack not commands, but data chunks
    // and move them to a buffer.
    // The buffer problem exists only to inbound data, not outbound data (relative to DebugStub).

	AL = #Ds2Vs_CmdCompleted
    ComWriteAL()
    
    EAX = .CommandID
    ComWriteAL()
}

procedure ProcessCommandBatch {
Begin:
    ProcessCommand()

    // See if batch is complete
    // Loop and wait
	// Vs2Ds.BatchEnd
	if AL != 8 goto Begin

    AckCommand()
}
