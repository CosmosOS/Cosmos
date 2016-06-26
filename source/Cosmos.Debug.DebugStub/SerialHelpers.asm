; Generated at 6/14/2016 12:11:29 PM



DebugStub_ComReadEAX:
Call DebugStub_ComReadAL
ror dword EAX, 0x8
Call DebugStub_ComReadAL
ror dword EAX, 0x8
Call DebugStub_ComReadAL
ror dword EAX, 0x8
Call DebugStub_ComReadAL
ror dword EAX, 0x8

DebugStub_ComReadEAX_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComReadEAX_Exit
Ret


DebugStub_ComRead8:
Call DebugStub_ComReadAL
mov byte [EDI], AL
add dword EDI, 0x1

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
push dword ESI
push dword EAX
mov dword ESI, ESP
Call DebugStub_ComWrite8
pop dword EAX
pop dword ESI

DebugStub_ComWriteAL_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWriteAL_Exit
Ret


DebugStub_ComWriteAX:
push dword EAX
mov dword ESI, ESP
Call DebugStub_ComWrite16
pop dword EAX

DebugStub_ComWriteAX_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWriteAX_Exit
Ret


DebugStub_ComWriteEAX:
push dword EAX
mov dword ESI, ESP
Call DebugStub_ComWrite32
pop dword EAX

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
dec dword ECX
JNE near DebugStub_ComWriteX_More

DebugStub_ComWriteX_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ComWriteX_Exit
Ret

