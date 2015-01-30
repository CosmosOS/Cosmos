; Generated at 11-1-2015 15:42:05

DebugWaitMsg db `Waiting for debugger connection...`, 0



%ifndef Exclude_Memory_Based_Console

DebugStub_Const_VidBase equ 0xB8000

DebugStub_Cls:
Mov ESI, DebugStub_Const_VidBase

DebugStub_Cls_Block1_Begin:
Cmp ESI, 0xB8FA0
JAE DebugStub_Cls_Block1_End
Mov dword [ESI + 0], 0x00
Inc ESI

Mov dword [ESI + 0], 0x0A
Inc ESI
jmp DebugStub_Cls_Block1_Begin
DebugStub_Cls_Block1_End:
DebugStub_Cls_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_Cls_Exit
Ret

DebugStub_DisplayWaitMsg:
Mov ESI, DebugWaitMsg

Mov EDI, DebugStub_Const_VidBase
Add EDI, 1640

DebugStub_DisplayWaitMsg_Block1_Begin:
Cmp byte [ESI + 0], 0
JE DebugStub_DisplayWaitMsg_Block1_End
Mov AL, [ESI + 0]
Mov [EDI + 0], AL
Inc ESI
Add EDI, 2
jmp DebugStub_DisplayWaitMsg_Block1_Begin
DebugStub_DisplayWaitMsg_Block1_End:
DebugStub_DisplayWaitMsg_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_DisplayWaitMsg_Exit
Ret

%endif

