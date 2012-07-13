

DebugStub_Const_VidBase equ 0xB8000

DebugStub_Cls:
Mov ESI, DebugStub_Const_VidBase

DebugStub_Cls_Block1Begin:
Cmp ESI, 0xB8FA0
JAE DebugStub_Cls_Block1End
Mov dword [ESI + 0], 0x00
Inc ESI

Mov dword [ESI + 0], 0x0A
Inc ESI
jmp DebugStub_Cls_Block1Begin
DebugStub_Cls_Block1End:
DebugStub_Cls_Exit:
Ret

DebugStub_DisplayWaitMsg:
Mov ESI, DebugWaitMsg

Mov EDI, DebugStub_Const_VidBase
Add EDI, 1640

Mov AL, 1
DebugStub_DisplayWaitMsg_Block2Begin:
Cmp AL, 0
JE DebugStub_DisplayWaitMsg_Block2End
Mov AL, [ESI + 0]
Mov [EDI + 0], AL
Inc ESI
Add EDI, 2
jmp DebugStub_DisplayWaitMsg_Block2Begin
DebugStub_DisplayWaitMsg_Block2End:
DebugStub_DisplayWaitMsg_Exit:
Ret

