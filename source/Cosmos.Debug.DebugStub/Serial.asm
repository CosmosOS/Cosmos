DebugStub_ComAddr dd 0x03F8




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
Ret

DebugStub_ComReadAL:
Mov DX, [DebugStub_ComAddr]
Add DX, 5
DebugStub_ComReadAL_Wait:
In AL, DX
Test AL, 0x01
JNZ DebugStub_ComReadAL_Wait

Mov DX, [DebugStub_ComAddr]
In AL, DX
DebugStub_ComReadAL_Exit:
Ret
DebugStub_ComReadEAX:
Call DebugStub_ComReadAL
ROR EAX, 8
Call DebugStub_ComReadAL
ROR EAX, 8
Call DebugStub_ComReadAL
ROR EAX, 8
Call DebugStub_ComReadAL
ROR EAX, 8
DebugStub_ComReadEAX_Exit:
Ret

DebugStub_ComRead8:
Call DebugStub_ComReadAL
Mov [EDI + 0], AL
Add EDI, 1
DebugStub_ComRead8_Exit:
Ret
DebugStub_ComRead16:
Call DebugStub_ComRead8
Call DebugStub_ComRead8
DebugStub_ComRead16_Exit:
Ret
DebugStub_ComRead32:
Call DebugStub_ComRead8
Call DebugStub_ComRead8
Call DebugStub_ComRead8
Call DebugStub_ComRead8
DebugStub_ComRead32_Exit:
Ret

DebugStub_ComWriteAL:
Push EAX
Mov ESI, ESP
Call DebugStub_ComWrite8
Pop EAX
DebugStub_ComWriteAL_Exit:
Ret
DebugStub_ComWriteAX:
Push EAX
Mov ESI, ESP
Call DebugStub_ComWrite16
Pop EAX
DebugStub_ComWriteAX_Exit:
Ret
DebugStub_ComWriteEAX:
Push EAX
Mov ESI, ESP
Call DebugStub_ComWrite32
Pop EAX
DebugStub_ComWriteEAX_Exit:
Ret

DebugStub_ComWrite8:

Mov DX, [DebugStub_ComAddr]
Add DX, 5

DebugStub_ComWrite8_Wait:
In AL, DX
Test AL, 0x20
JNZ DebugStub_ComWrite8_Wait

Mov DX, 0x03F8
Mov AL, [ESI + 0]
Out DX, AL

Inc ESI
DebugStub_ComWrite8_Exit:
Ret
DebugStub_ComWrite16:
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
DebugStub_ComWrite16_Exit:
Ret
DebugStub_ComWrite32:
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
DebugStub_ComWrite32_Exit:
Ret
DebugStub_ComWriteX:
DebugStub_ComWriteX_More:
Call DebugStub_ComWrite8
Dec ECX
JNZ DebugStub_ComWriteX_More
DebugStub_ComWriteX_Exit:
Ret

