; Generated at 28-7-2015 17:36:29



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
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendRegisters_Exit
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
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendFrame_Exit
Ret

DebugStub_SendCommandOnChannel:
Pushad
Call DebugStub_ComWriteAL
Popad

Mov AL, BL

Pushad
Call DebugStub_ComWriteAL
Popad

Pushad
Mov EAX, ECX
Call DebugStub_ComWriteEAX
Popad

DebugStub_SendCommandOnChannel_Block1_Begin:
Cmp ECX, 0
JE DebugStub_SendCommandOnChannel_Block1_End
Call DebugStub_ComWrite8
Dec ECX
jmp DebugStub_SendCommandOnChannel_Block1_Begin
DebugStub_SendCommandOnChannel_Block1_End:
DebugStub_SendCommandOnChannel_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendCommandOnChannel_Exit
Ret

DebugStub_SendStack:
Mov AL, DebugStub_Const_Ds2Vs_Stack
Call DebugStub_ComWriteAL

Mov ESI, [DebugStub_CallerESP]
Mov EAX, [DebugStub_CallerEBP]
Sub EAX, ESI
Call DebugStub_ComWriteAX


Mov ESI, [DebugStub_CallerESP]
DebugStub_SendStack_Block1_Begin:
Cmp ESI, [DebugStub_CallerEBP]
JE DebugStub_SendStack_Block1_End
Call DebugStub_ComWrite8
jmp DebugStub_SendStack_Block1_Begin
DebugStub_SendStack_Block1_End:
DebugStub_SendStack_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendStack_Exit
Ret

DebugStub_SendMethodContext:
Pushad

Mov AL, DebugStub_Const_Ds2Vs_MethodContext
Call DebugStub_ComWriteAL

Mov ESI, [DebugStub_CallerEBP]

Call DebugStub_ComReadEAX
Add ESI, EAX
Call DebugStub_ComReadEAX
Mov ECX, EAX


DebugStub_SendMethodContext_Block1_Begin:
Cmp ECX, 0
JE DebugStub_SendMethodContext_Block1_End
Call DebugStub_ComWrite8
Dec ECX
jmp DebugStub_SendMethodContext_Block1_Begin
DebugStub_SendMethodContext_Block1_End:

DebugStub_SendMethodContext_Exit:
Popad
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendMethodContext_Exit
Ret


DebugStub_SendMemory:
Pushad

Mov AL, DebugStub_Const_Ds2Vs_MemoryData
Call DebugStub_ComWriteAL

Call DebugStub_ComReadEAX
Mov ESI, EAX
Call DebugStub_ComReadEAX
Mov ECX, EAX

DebugStub_SendMemory_Block1_Begin:
Cmp ECX, 0
JE DebugStub_SendMemory_Block1_End
Call DebugStub_ComWrite8
Dec ECX
jmp DebugStub_SendMemory_Block1_Begin
DebugStub_SendMemory_Block1_End:

DebugStub_SendMemory_Exit:
Popad
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendMemory_Exit
Ret

DebugStub_SendTrace:
Mov AL, DebugStub_Const_Ds2Vs_BreakPoint
Cmp dword [DebugStub_DebugStatus], DebugStub_Const_Status_Run
JNE DebugStub_SendTrace_Block1_End
Mov AL, DebugStub_Const_Ds2Vs_TracePoint
DebugStub_SendTrace_Block1_End:
Call DebugStub_ComWriteAL

Mov ESI, DebugStub_CallerEIP
Call DebugStub_ComWrite32
DebugStub_SendTrace_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendTrace_Exit
Ret

DebugStub_SendText:
Push EBP
Mov EBP, ESP
Pushad
Mov AL, DebugStub_Const_Ds2Vs_Message
Call DebugStub_ComWriteAL

Mov ESI, EBP
Add ESI, 12
Mov ECX, [ESI + 0]
Call DebugStub_ComWrite16

Mov ESI, [EBP + 8]
DebugStub_SendText_WriteChar:
Cmp ECX, 0
JE DebugStub_SendText_Finalize
Call DebugStub_ComWrite8
Dec ECX
Inc ESI
Jmp DebugStub_SendText_WriteChar


DebugStub_SendText_Finalize:
Popad
Pop EBP
DebugStub_SendText_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendText_Exit
Ret

DebugStub_SendSimpleNumber:
Push EBP
Mov EBP, ESP
Pushad
Mov AL, DebugStub_Const_Ds2Vs_SimpleNumber
Call DebugStub_ComWriteAL

Mov EAX, [EBP + 8]
Call DebugStub_ComWriteEAX

Popad
Pop EBP
DebugStub_SendSimpleNumber_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendSimpleNumber_Exit
Ret

DebugStub_SendPtr:
Mov AL, DebugStub_Const_Ds2Vs_Pointer
Call DebugStub_ComWriteAL

Mov ESI, [EBP + 8]
Call DebugStub_ComWrite32
DebugStub_SendPtr_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendPtr_Exit
Ret

DebugStub_SendStackCorruptionOccurred:
Mov AL, DebugStub_Const_Ds2Vs_StackCorruptionOccurred
Call DebugStub_ComWriteAL

Mov ESI, DebugStub_CallerEIP
Call DebugStub_ComWrite32
DebugStub_SendStackCorruptionOccurred_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendStackCorruptionOccurred_Exit
Ret

DebugStub_SendNullReferenceOccurred:
Mov AL, DebugStub_Const_Ds2Vs_NullReferenceOccurred
Call DebugStub_ComWriteAL

Mov ESI, DebugStub_CallerEIP
Call DebugStub_ComWrite32
DebugStub_SendNullReferenceOccurred_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendNullReferenceOccurred_Exit
Ret

DebugStub_SendMessageBox:
Mov AL, DebugStub_Const_Ds2Vs_MessageBox
Call DebugStub_ComWriteAL

Mov ESI, EBP
Add ESI, 12
Mov ECX, [ESI + 0]
Call DebugStub_ComWrite16

Mov ESI, [EBP + 8]
DebugStub_SendMessageBox_WriteChar:
Cmp ECX, 0
JE DebugStub_SendMessageBox_Exit
Call DebugStub_ComWrite8
Dec ECX
Inc ESI
Jmp DebugStub_SendMessageBox_WriteChar
DebugStub_SendMessageBox_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendMessageBox_Exit
Ret

