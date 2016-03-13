; Generated at 3/12/2016 8:37:32 PM




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
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComReadEAX_Exit
Ret


DebugStub_ComRead8:
Call DebugStub_ComReadAL
Mov [EDI + 0], AL
Add EDI, 1
DebugStub_ComRead8_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComRead8_Exit
Ret
DebugStub_ComRead16:
Call DebugStub_ComRead8
Call DebugStub_ComRead8
DebugStub_ComRead16_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComRead16_Exit
Ret
DebugStub_ComRead32:
Call DebugStub_ComRead8
Call DebugStub_ComRead8
Call DebugStub_ComRead8
Call DebugStub_ComRead8
DebugStub_ComRead32_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComRead32_Exit
Ret

DebugStub_ComWriteAL:
Push ESI
Push EAX
Mov ESI, ESP
Call DebugStub_ComWrite8
Pop EAX
Pop ESI
DebugStub_ComWriteAL_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWriteAL_Exit
Ret
DebugStub_ComWriteAX:
Push EAX
Mov ESI, ESP
Call DebugStub_ComWrite16
Pop EAX
DebugStub_ComWriteAX_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWriteAX_Exit
Ret
DebugStub_ComWriteEAX:
Push EAX
Mov ESI, ESP
Call DebugStub_ComWrite32
Pop EAX
DebugStub_ComWriteEAX_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWriteEAX_Exit
Ret

DebugStub_ComWrite16:
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
DebugStub_ComWrite16_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWrite16_Exit
Ret
DebugStub_ComWrite32:
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
Call DebugStub_ComWrite8
DebugStub_ComWrite32_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWrite32_Exit
Ret
DebugStub_ComWriteX:
DebugStub_ComWriteX_More:
Call DebugStub_ComWrite8
Dec ECX
JNZ DebugStub_ComWriteX_More
DebugStub_ComWriteX_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWriteX_Exit
Ret

