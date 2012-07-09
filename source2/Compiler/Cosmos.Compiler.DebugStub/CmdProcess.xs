Group DebugStub

# test when emitted after usage too
! DebugStub_DsVsip_CmdCompleted equ 9

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

	# DsVsip.CmdCompleted
	# ! mov AL, DebugStub_DsVsip_CmdCompleted
	#AL = .DsVsip_CmdCompleted
    #AL = 9
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
	if (AL != 8) goto Begin

    AckCommand()
}
