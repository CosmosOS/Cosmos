



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


DebugStub_Executing2_Normal:
Cmp dword [DebugStub_TraceMode], DebugStub_Const_Tracing_On
JNE DebugStub_Executing2_Block1End
Call DebugStub_SendTrace
DebugStub_Executing2_Block1End:

DebugStub_Executing2_CheckForCmd:
In AL, DX
Test AL, 1

Line 79, Parsing error: if !0 {
