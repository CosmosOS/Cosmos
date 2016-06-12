; Generated at 6/12/2016 10:15:08 AM



DebugStub_CheckStack:
add dword EAX, 0x4
Mov EBX, EBP
add dword EBX, EAX
Cmp EBX, ESP
JE near DebugStub_CheckStack_Block1_End
Mov EAX, [ESP + 0]
Mov [DebugStub_CallerEIP], EAX
Call DebugStub_SendStackCorruptionOccurred

DebugStub_CheckStack_halt:
Jmp DebugStub_CheckStack_halt

DebugStub_CheckStack_Block1_End:

DebugStub_CheckStack_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_CheckStack_Exit
Ret

