; Generated at 3/12/2016 8:37:32 PM

DebugStub_DebugBPs TIMES 256 dd 0
DebugStub_MaxBPId dd 0



DebugStub_Init:
Call DebugStub_Cls
Call DebugStub_DisplayWaitMsg
Call DebugStub_InitSerial
Call DebugStub_WaitForDbgHandshake
Call DebugStub_Cls
DebugStub_Init_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_Init_Exit
Ret

DebugStub_WaitForSignature:
Mov EBX, 0
DebugStub_WaitForSignature_Block1_Begin:
Cmp EBX, DebugStub_Const_Signature
JE DebugStub_WaitForSignature_Block1_End
Call DebugStub_ComReadAL
Mov BL, AL
ROR EBX, 8
jmp DebugStub_WaitForSignature_Block1_Begin
DebugStub_WaitForSignature_Block1_End:
DebugStub_WaitForSignature_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_WaitForSignature_Exit
Ret

DebugStub_WaitForDbgHandshake:
Mov AL, 0
Call DebugStub_ComWriteAL
Mov AL, 0
Call DebugStub_ComWriteAL
Mov AL, 0
Call DebugStub_ComWriteAL

Push dword DebugStub_Const_Signature
Mov ESI, ESP

Call DebugStub_ComWrite32

Pop EAX


Mov AL, DebugStub_Const_Ds2Vs_Started
Call DebugStub_ComWriteAL

Call DebugStub_WaitForSignature
Call DebugStub_ProcessCommandBatch
Call DebugStub_Hook_OnHandshakeCompleted
DebugStub_WaitForDbgHandshake_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_WaitForDbgHandshake_Exit
Ret

%ifndef Exclude_Dummy_Hooks
DebugStub_Hook_OnHandshakeCompleted:
DebugStub_Hook_OnHandshakeCompleted_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_Hook_OnHandshakeCompleted_Exit
Ret
%endif

