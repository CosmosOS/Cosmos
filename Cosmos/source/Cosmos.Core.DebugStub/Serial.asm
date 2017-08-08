; Generated at 6/14/2016 12:11:29 PM


%ifndef Exclude_IOPort_Based_SerialInit

DebugStub_InitSerial:
mov word DX, 0x1
mov byte AL, 0x0
Call DebugStub_WriteRegister
mov word DX, 0x3
mov byte AL, 0x80
Call DebugStub_WriteRegister
mov word DX, 0x0
mov byte AL, 0x1
Call DebugStub_WriteRegister
mov word DX, 0x1
mov byte AL, 0x0
Call DebugStub_WriteRegister
mov word DX, 0x3
mov byte AL, 0x3
Call DebugStub_WriteRegister
mov word DX, 0x2
mov byte AL, 0xC7
Call DebugStub_WriteRegister
mov word DX, 0x4
mov byte AL, 0x3
Call DebugStub_WriteRegister

DebugStub_InitSerial_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_InitSerial_Exit
Ret


DebugStub_ComReadAL:
mov word DX, 0x5

DebugStub_ComReadAL_Wait:
Call DebugStub_ReadRegister
test byte AL, 0x1
JE near DebugStub_ComReadAL_Wait
mov word DX, 0x0
Call DebugStub_ReadRegister

DebugStub_ComReadAL_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComReadAL_Exit
Ret


DebugStub_ComWrite8:
mov word DX, 0x5

DebugStub_ComWrite8_Wait:
Call DebugStub_ReadRegister
test byte AL, 0x20
JE near DebugStub_ComWrite8_Wait
mov word DX, 0x0
mov byte AL, [ESI]
Call DebugStub_WriteRegister
inc dword ESI

DebugStub_ComWrite8_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWrite8_Exit
Ret

%endif
