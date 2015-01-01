; Generated at 1-1-2015 12:50:54

DebugStub_ComAddr dd 0x03F8


%ifndef Exclude_IOPort_Based_Serial












DebugStub_InitSerial:
Mov DX, [DebugStub_ComAddr]

Mov BX, DX
Add DX, 1
Mov AL, 0
Out DX, AL

Mov DX, BX
Add DX, 3
Mov AL, 0x80
Out DX, AL


Mov DX, BX
Mov AL, 0x01
Out DX, AL
Mov DX, BX
Add DX, 1
Mov AL, 0x00
Out DX, AL

Mov DX, BX
Add DX, 3
Mov AL, 0x03
Out DX, AL

Mov DX, BX
Add DX, 2
Mov AL, 0xC7
Out DX, AL

Mov DX, BX
Add DX, 4
Mov AL, 0x03
Out DX, AL
DebugStub_InitSerial_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_InitSerial_Exit
Ret

DebugStub_ComReadAL:
Mov DX, [DebugStub_ComAddr]
Add DX, 5
DebugStub_ComReadAL_Wait:
In AL, DX
Test AL, 0x01
JZ DebugStub_ComReadAL_Wait

Mov DX, [DebugStub_ComAddr]
In AL, DX
DebugStub_ComReadAL_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComReadAL_Exit
Ret

DebugStub_ComWrite8:




Mov DX, [DebugStub_ComAddr]
Add DX, 5

DebugStub_ComWrite8_Wait:
In AL, DX
Test AL, 0x20
JZ DebugStub_ComWrite8_Wait

Mov DX, 0x03F8
Mov AL, [ESI + 0]
Out DX, AL

Inc ESI
DebugStub_ComWrite8_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWrite8_Exit
Ret

%endif

