Group DebugStub

procedure SendRegisters {
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

procedure SendFrame2 {
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

procedure SendStack2 {
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
	//TODO While
SendByte:
    if ESI = .CallerEBP exit
    ComWrite8()
    goto SendByte
}

procedure SendMethodContext2 {
    // sends a stack value
    // Serial Params:
    //  1: x32 - offset relative to EBP
    //  2: x32 - size of data to send
//    [XSharp(PreserveStack = true)]

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

    // TODO While
SendByte:
	if ECX = 0 goto AfterSendByte
    ComWrite8()
    ECX--
    goto SendByte
AfterSendByte:
}

procedure SendMemory2 {
    // sends a stack value
    // Serial Params:
    //  1: x32 - offset relative to EBP
    //  2: x32 - size of data to send
//    [XSharp(PreserveStack = true)]
    ComReadEAX()
    ECX = EAX
    AL = #Ds2Vs_MemoryData
    ComWriteAL()

    ComReadEAX()
    ESI = EAX

// TODO - Make this a method and use it in above procedure too
    // now ECX contains size of data (count)
    // ESI contains address

SendByte:
	if ECX = 0 goto AfterSendByte
    ComWrite8()
    ECX--
    goto SendByte
AfterSendByte:
}

// Modifies: EAX, ESI
procedure SendTrace2 {
	if .DebugStatus = #Status_Run goto Normal

    AL = #Ds2Vs_BreakPoint
    goto Type

Normal:
    AL = #Ds2Vs_TracePoint

Type:
    ComWriteAL()

    // Send Calling EIP.
    ESI = @.CallerEIP
    ComWrite32()
}

// Input: Stack
// Output: None
// Modifies: EAX, ECX, EDX, ESI
procedure SendText2 {
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
    if ECX = 0 exit
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
procedure SendPtr2 {
    // Write the type
    AL = #Ds2Vs_Pointer
    ComWriteAL()

    // pointer value
    ESI = EBP[8]
    ComWrite32()
}
