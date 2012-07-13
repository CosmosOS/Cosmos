

DebugStub_SendRegisters:
Mov AL, DebugStub_Const_Ds2Vs_Registers
Call DebugStub_ComWriteAL

Mov ESI, [DebugStub_PushAllPtr]
Mov ECX, 32
Call DebugStub_ComWriteX

Mov ESI, DebugStub_CallerESP
Call DebugStub_ComWrite32

Mov ESI, DebugStub_CallerEIP
Call DebugStub_ComWrite32
DebugStub_SendRegisters_Exit:
Ret

DebugStub_SendFrame:
Mov AL, DebugStub_Const_Ds2Vs_Frame
Call DebugStub_ComWriteAL

Mov EAX, 32
Call DebugStub_ComWriteAX

Mov ESI, [DebugStub_CallerEBP]
Add ESI, 8
Mov ECX, 32
Call DebugStub_ComWriteX
DebugStub_SendFrame_Exit:
Ret

DebugStub_SendStack:
Mov AL, DebugStub_Const_Ds2Vs_Stack
Call DebugStub_ComWriteAL

Mov ESI, [DebugStub_CallerESP]
Mov EAX, [DebugStub_CallerEBP]
Sub EAX, ESI
Call DebugStub_ComWriteAX

Mov ESI, [DebugStub_CallerESP]
DebugStub_SendStack_Block1Begin:
Cmp ESI, [DebugStub_CallerEBP]
JE DebugStub_SendStack_Block1End
Call DebugStub_ComWrite8
jmp DebugStub_SendStack_Block1Begin
DebugStub_SendStack_Block1End:
DebugStub_SendStack_Exit:
Ret

DebugStub_SendMethodContext:
Pushad

Mov AL, DebugStub_Const_Ds2Vs_MethodContext
Call DebugStub_ComWriteAL

Call DebugStub_ComReadEAX
Mov ECX, EAX
Call DebugStub_ComReadEAX

Mov ESI, [DebugStub_CallerEBP]
Add ESI, EAX

DebugStub_SendMethodContext_Block2Begin:
Cmp ECX, 0
JE DebugStub_SendMethodContext_Block2End
Call DebugStub_ComWrite8
Dec ECX
jmp DebugStub_SendMethodContext_Block2Begin
DebugStub_SendMethodContext_Block2End:

DebugStub_SendMethodContext_Exit:
Popad
Ret

DebugStub_SendMemory:
Pushad

Call DebugStub_ComReadEAX
Mov ECX, EAX
Mov AL, DebugStub_Const_Ds2Vs_MemoryData
Call DebugStub_ComWriteAL

Call DebugStub_ComReadEAX
Mov ESI, EAX

DebugStub_SendMemory_Block3Begin:
Cmp ECX, 0
JE DebugStub_SendMemory_Block3End
Call DebugStub_ComWrite8
Dec ECX
jmp DebugStub_SendMemory_Block3Begin
DebugStub_SendMemory_Block3End:

DebugStub_SendMemory_Exit:
Popad
Ret

DebugStub_SendTrace:
Cmp dword [DebugStub_DebugStatus], DebugStub_Const_Status_Run
JE DebugStub_SendTrace_Normal

Mov AL, DebugStub_Const_Ds2Vs_BreakPoint
Jmp DebugStub_SendTrace_Type
DebugStub_SendTrace_Normal:
Mov AL, DebugStub_Const_Ds2Vs_TracePoint
DebugStub_SendTrace_Type:
Call DebugStub_ComWriteAL
Mov ESI, DebugStub_CallerEIP
Call DebugStub_ComWrite32
DebugStub_SendTrace_Exit:
Ret

DebugStub_SendText:
Mov AL, DebugStub_Const_Ds2Vs_Message
Call DebugStub_ComWriteAL

Mov ESI, EBP
Add ESI, 12
Mov ECX, [ESI + 0]
Call DebugStub_ComWrite16

Mov ESI, [EBP + 8]
DebugStub_SendText_WriteChar:
Cmp ECX, 0
JE DebugStub_SendText_Exit
Call DebugStub_ComWrite8
Dec ECX
Inc ESI
Jmp DebugStub_SendText_WriteChar
DebugStub_SendText_Exit:
Ret

DebugStub_SendPtr:
Mov AL, DebugStub_Const_Ds2Vs_Pointer
Call DebugStub_ComWriteAL

Mov ESI, [EBP + 8]
Call DebugStub_ComWrite32
DebugStub_SendPtr_Exit:
Ret

