; Generated at 22-5-2015 19:17:06












%ifndef Exclude_IOPort_Based_SerialInit

DebugStub_InitSerial:
Mov DX, 1
Mov AL, 0
Call DebugStub_WriteRegister

Mov DX, 3
Mov AL, 0x80
Call DebugStub_WriteRegister


Mov DX, 0
Mov AL, 0x01
Call DebugStub_WriteRegister

Mov DX, 1
Mov AL, 0x00
Call DebugStub_WriteRegister

Mov DX, 3
Mov AL, 0x03
Call DebugStub_WriteRegister

Mov DX, 2
Mov AL, 0xC7
Call DebugStub_WriteRegister

Mov DX, 4
Mov AL, 0x03
Call DebugStub_WriteRegister
DebugStub_InitSerial_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_InitSerial_Exit
Ret

DebugStub_ComReadAL:
Mov DX, 5
DebugStub_ComReadAL_Wait:
Call DebugStub_ReadRegister
Test AL, 0x01
JZ DebugStub_ComReadAL_Wait

Mov DX, 0
Call DebugStub_ReadRegister
DebugStub_ComReadAL_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComReadAL_Exit
Ret

DebugStub_ComWrite8:




Mov DX, 5

DebugStub_ComWrite8_Wait:
Call DebugStub_ReadRegister
Test AL, 0x20
JZ DebugStub_ComWrite8_Wait

Mov DX, 0
Mov AL, [ESI + 0]
Call DebugStub_WriteRegister

Inc ESI
DebugStub_ComWrite8_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWrite8_Exit
Ret

%endif

