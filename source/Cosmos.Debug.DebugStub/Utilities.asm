

DebugStub_CheckStack:

Add eax, 4
Mov EBX, EBP
Add EBX, EAX

Cmp EBX, ESP
JE DebugStub_CheckStack_Block1_End
Mov EAX, [ESP + 0]
Mov [DebugStub_CallerEIP], EAX
Call DebugStub_SendStackCorruptionOccurred
DebugStub_CheckStack_halt:
Jmp DebugStub_CheckStack_halt
DebugStub_CheckStack_Block1_End:
DebugStub_CheckStack_Exit:
Ret

