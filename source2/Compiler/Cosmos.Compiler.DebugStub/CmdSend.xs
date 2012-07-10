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
SendByte:
//    if ESI = CallerEBP exit
    ComWrite8()
    goto SendByte
}

procedure SendMethodContext2 {
//    // sends a stack value
//    // Serial Params:
//    //  1: x32 - offset relative to EBP
//    //  2: x32 - size of data to send
//    [XSharp(PreserveStack = true)]
//    AL = Ds2Vs_MethodContext
    ComWriteAL()
//
//    // offset relative to ebp
//    // size of data to send
//    Call("DebugStub_ComReadEAX()
//    ECX = EAX
//    Call("DebugStub_ComReadEAX()
//
//    // now ECX contains size of data (count)
//    // EAX contains relative to EBP
//    ESI = CallerEBP.Value
//    ESI.Add(EAX) //TODO: ESI = ESI + EAX
//
//    Label = ".SendByte"
//    ECX.Compare(0)
//    JumpIf(Flags.Equal, ".AfterSendByte")
    ComWrite8()
//    ECX--
//    Jump(".SendByte")
//    Label = ".AfterSendByte"
}

procedure SendMemory2 {
//    // sends a stack value
//    // Serial Params:
//    //  1: x32 - offset relative to EBP
//    //  2: x32 - size of data to send
//    [XSharp(PreserveStack = true)]
//    procedure
//    Call("DebugStub_ComReadEAX()
//    ECX = EAX
//    AL = Ds2Vs_MemoryData
    ComWriteAL()
//
//    Call("DebugStub_ComReadEAX()
//    ESI = EAX
//
//    // now ECX contains size of data (count)
//    // ESI contains address
//
//    Label = "DebugStub_SendMemory_SendByte"
//    new Compare { DestinationReg = Registers.ECX, SourceValue = 0 }
//    JumpIf(Flags.Equal, "DebugStub_SendMemory_After_SendByte")
    ComWrite8()
//    ECX--
//    Jump("DebugStub_SendMemory_SendByte")
//
//    Label = "DebugStub_SendMemory_After_SendByte"
//    }
}

procedure SendTrace2 {
//    // Modifies: EAX, ESI
//    DebugStatus.Value.Compare(Status.Run)
//    JumpIf(Flags.Equal, ".Normal")
//    AL = Ds2Vs_BreakPoint
//    Jump(".Type")
//
//    Label = ".Normal"
//    AL = Ds2Vs_TracePoint
//
//    Label = ".Type"
    ComWriteAL()
//
//    // Send Calling EIP.
//    ESI = CallerEIP.Address
//    DebugStub_ComWrite32()
}

procedure SendText2 {
//    // Input: Stack
//    // Output: None
//    // Modifies: EAX, ECX, EDX, ESI
//    // Write the type
//    AL = Ds2Vs_Message
    ComWriteAL()
//
//    // Write Length
//    ESI = EBP
//    ESI = ESI + 12
//    ECX = ESI[0]
    ComWrite16()
//
//    // Address of string
//    ESI = EBP[8]
//    Label = ".WriteChar"
//    ECX.Compare(0)
//    JumpIf(Flags.Equal, ".Exit")
    ComWrite8()
//    ECX--
//    // We are storing as 16 bits, but for now I will transmit 8 bits
//    // So we inc again to skip the 0
//    ESI++
//    Jump(".WriteChar")
}

procedure SendPtr2 {
//    // Input: Stack
//    // Output: None
//    // Modifies: EAX, ECX, EDX, ESI
//    // Write the type
//    AL = Ds2Vs_Pointer
    ComWriteAL()
//
//    // pointer value
//    ESI = EBP[8]
//    DebugStub_ComWrite32()
}
