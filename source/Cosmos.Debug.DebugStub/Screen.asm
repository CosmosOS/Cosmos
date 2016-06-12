; Generated at 6/12/2016 10:25:12 AM

DebugWaitMsg db 96, 87, 97, 105, 116, 105, 110, 103, 32, 102, 111, 114, 32, 100, 101, 98, 117, 103, 103, 101, 114, 32, 99, 111, 110, 110, 101, 99, 116, 105, 111, 110, 46, 46, 46, 96, 0

%ifndef Exclude_Memory_Based_Console
DebugStub_Const_VidBase equ 753664

DebugStub_Cls:
Mov ESI, DebugStub_Const_VidBase

DebugStub_Cls_Block1_Begin:
Cmp ESI, 0xB8FA0
JNB near DebugStub_Cls_Block1_End
Mov dword [ESI + 0], 0x00
inc dword ESI
Mov dword [ESI + 0], 0x0A
inc dword ESI
Jmp DebugStub_Cls_Block1_Begin

DebugStub_Cls_Block1_End:

DebugStub_Cls_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_Cls_Exit
Ret


DebugStub_DisplayWaitMsg:
Mov ESI, DebugWaitMsg
Mov EDI, DebugStub_Const_VidBase
add dword EDI, 0x668

DebugStub_DisplayWaitMsg_Block1_Begin:
Cmp byte [ESI + 0], 0
JE near DebugStub_DisplayWaitMsg_Block1_End
Mov AL, [ESI + 0]
Mov [EDI + 0], AL
inc dword ESI
add dword EDI, 0x2
Jmp DebugStub_DisplayWaitMsg_Block1_Begin

DebugStub_DisplayWaitMsg_Block1_End:

DebugStub_DisplayWaitMsg_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_DisplayWaitMsg_Exit
Ret

%endif
