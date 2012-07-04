Group DebugStub

# Called before Kernel runs. Inits debug stub, etc
procedure Init {
    Call .Cls
	# Display message before even trying to init serial
    Call .DisplayWaitMsg
    Call .InitSerial
    Call .WaitForDbgHandshake
    Call .Cls
}

procedure WaitForSignatureNew {
    EBX = 0

Read:
    Call .ReadALFromComPort
    BL = AL
    #EBX.RotateRight(8)
    #EBX.Compare(Cosmos.Debug.Consts.Consts.SerialSignature)
    #JumpIf(Flags.NotEqual, "DebugStub_WaitForSignature_Read")
}

# QEMU (and possibly others) send some garbage across the serial line first.
# Actually they send the garbage inbound, but garbage could be inbound as well so we 
# keep this.
# To work around this we send a signature. DC then discards everything before the signature.
# QEMU has other serial issues too, and we dont support it anymore, but this signature is a good
# feature so we kept it.
procedure WaitForDbgHandshake {
    # "Clear" the UART out
    AL = 0
    Call .WriteALToComPort

    # Cosmos.Debug.Consts.Consts.SerialSignature
	+$19740807
    ESI = ESP

    # TODO pass a count register
    Call .WriteByteToComPort
    Call .WriteByteToComPort
    Call .WriteByteToComPort
    Call .WriteByteToComPort

    # Restore ESP, we actually dont care about EAX or the value on the stack anymore.
    -EAX

    # We could use the signature as the start signal, but I prefer
    # to keep the logic separate, especially in DC.

	# Send the actual started signal
	# DsVsip.Started = 6
    AL = 6
    Call .WriteALToComPort

    Call .WaitForSignature
    Call .ProcessCommandBatch
}

