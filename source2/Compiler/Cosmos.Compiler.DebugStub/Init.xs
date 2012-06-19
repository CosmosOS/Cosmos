Group DebugStub

# Called before Kernel runs. Inits debug stub, etc
procedure Init {
    Call .Cls
    Call .DisplayWaitMsg
    Call .InitSerial
    Call .WaitForDbgHandshake
    Call .Cls
}
