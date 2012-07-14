



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

Call DebugStub_SendTrace

DebugStub_Executing2_CheckForCmd:
In AL, DX
Test AL, 1
Call DebugStub_ProcessCommand
Jmp DebugStub_Executing2_CheckForCmd
DebugStub_Executing2_Exit:
Ret




























