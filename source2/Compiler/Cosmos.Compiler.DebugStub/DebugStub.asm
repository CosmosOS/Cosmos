



DebugStub_BreakOnAddress:
Pushad
Call DebugStub_ComReadEAX
Mov ECX, EAX

Mov EAX, 0
Call DebugStub_ComReadAL

Mov EBX, DebugBPs
SHL EAX, 2
Add EBX, EAX

Mov [EBX + 0], ECX
DebugStub_BreakOnAddress_Exit:
Popad
Ret

DebugStub_Executing2:

Mov EAX, [DebugStub_CallerEIP]
Cmp EAX, [DebugStub_AsmBreakEIP]
JNE DebugStub_Executing2_Block1End
Call DebugStub_ClearAsmBreak
Call DebugStub_Break
Jmp DebugStub_Executing2_Normal
DebugStub_Executing2_Block1End:


Mov EAX, [DebugStub_CallerEIP]
Mov EDI, DebugBPs
Mov ECX, 256
repne scasd

Index was out of range. Must be non-negative and less than the size of the collection.
Parameter name: index
