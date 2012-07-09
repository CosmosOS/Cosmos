Group DebugStub

// Todo: Change these to a group level var
var .DebugBPs int[256]
var .DebugWaitMsg = 'Waiting for debugger connection...'

// Called before Kernel runs. Inits debug stub, etc
procedure Init {
    Cls()
	// Display message before even trying to init serial
    DisplayWaitMsg()
    InitSerial()
    WaitForDbgHandshake()
    Cls()
}

procedure WaitForSignature {
    EBX = 0
Read:
    ComReadAL()
    BL = AL
    EBX ~> 8
    if EBX != $19740807 goto Read
}

// QEMU (and possibly others) send some garbage across the serial line first.
// Actually they send the garbage inbound, but garbage could be inbound as well so we 
// keep this.
// To work around this we send a signature. DC then discards everything before the signature.
// QEMU has other serial issues too, and we dont support it anymore, but this signature is a good
// feature so we kept it.
procedure WaitForDbgHandshake {
    // "Clear" the UART out
    AL = 0
    ComWriteAL()

    // Cosmos.Debug.Consts.Consts.SerialSignature
	+$19740807
    ESI = ESP

    // TODO pass a count register
    ComWrite8()
    ComWrite8()
    ComWrite8()
    ComWrite8()

    // Restore ESP, we actually dont care about EAX or the value on the stack anymore.
    -EAX

    // We could use the signature as the start signal, but I prefer
    // to keep the logic separate, especially in DC.

	// Send the actual started signal
	// Ds2Vs.Started = 6
    AL = 6
    ComWriteAL()

    WaitForSignature()
    ProcessCommandBatch()
}

