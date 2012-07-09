Group DebugStub

const DsVsip_Noop = 0
const DsVsip_TracePoint = 1
const DsVsip_Message = 2
const DsVsip_BreakPoint = 3
const DsVsip_Error = 4
const DsVsip_Pointer = 5
const DsVsip_Started = 6
const DsVsip_MethodContext = 7
const DsVsip_MemoryData = 8
const DsVsip_CmdCompleted = 9
const DsVsip_Registers = 10
const DsVsip_Frame = 11
const DsVsip_Stack = 12
const DsVsip_Pong = 13

procedure AckCommand {
    # We acknowledge receipt of the command AND the processing of it.
    #   -In the past the ACK only acknowledged receipt.
    # We have to do this because sometimes callers do more processing.
    # We ACK even ones we dont process here, but do not ACK Noop.
    # The buffers should be ok because more wont be sent till after our NACK
    # is received.
    # Right now our max cmd size is 2 (Cmd + Cmd ID) + 5 (Data) = 7. 
    # UART buffer is 16.
    # We may need to revisit this in the future to ack not commands, but data chunks
    # and move them to a buffer.
    # The buffer problem exists only to inbound data, not outbound data (relative to DebugStub).

	AL = #DsVsip_CmdCompleted
    ComWriteAL()
    
    EAX = .CommandID
    ComWriteAL()
}

procedure ProcessCommandBatch {
Begin:
    ProcessCommand()

    # See if batch is complete
    # Loop and wait
	# VsipDs.BatchEnd
	if AL != 8 goto Begin

    AckCommand()
}
