; Generated at 6/14/2016 12:11:29 PM



DebugStub_CheckStack:
add dword EAX, 0x4
mov dword EBX, EBP
add dword EBX, EAX
cmp dword EBX, ESP
JE near DebugStub_CheckStack_Block1_End
mov dword EAX, [ESP]
mov dword [DebugStub_CallerEIP], EAX
Call DebugStub_SendStackCorruptionOccurred

DebugStub_CheckStack_halt:
Jmp DebugStub_CheckStack_halt

DebugStub_CheckStack_Block1_End:

DebugStub_CheckStack_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_CheckStack_Exit
Ret

