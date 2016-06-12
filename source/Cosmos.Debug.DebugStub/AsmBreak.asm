; Generated at 6/12/2016 10:15:07 AM

DebugStub_AsmBreakEIP dd 0
DebugStub_AsmOrigByte dd 0


DebugStub_DoAsmBreak:
Mov ESI, [DebugStub_CallerESP]
Mov EAX, [DebugStub_AsmBreakEIP]
Mov [ESI - 12], EAX
Call DebugStub_ClearAsmBreak
Call DebugStub_Break

DebugStub_DoAsmBreak_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_DoAsmBreak_Exit
Ret


DebugStub_SetAsmBreak:
Call DebugStub_ClearAsmBreak
Call DebugStub_ComReadEAX
Mov [DebugStub_AsmBreakEIP], EAX
Mov EDI, EAX
Mov AL, [EDI + 0]
Mov [DebugStub_AsmOrigByte], AL
mov byte AL, 0xCC
Mov [EDI + 0], AL

DebugStub_SetAsmBreak_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SetAsmBreak_Exit
Ret


DebugStub_ClearAsmBreak:
Mov EDI, [DebugStub_AsmBreakEIP]
Cmp EDI, 0
JE near DebugStub_ClearAsmBreak_Exit
Mov AL, [DebugStub_AsmOrigByte]
Mov [EDI + 0], AL
Mov dword [DebugStub_AsmBreakEIP], 0

DebugStub_ClearAsmBreak_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ClearAsmBreak_Exit
Ret


DebugStub_SetINT1_TrapFLAG:
push dword EBP
push dword EAX
Mov EBP, [DebugStub_CallerESP]
sub dword EBP, 0x4
Mov EAX, [EBP]
or dword EAX, 0x100
Mov [EBP], EAX
pop dword EAX
pop dword EBP

DebugStub_SetINT1_TrapFLAG_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SetINT1_TrapFLAG_Exit
Ret


DebugStub_ResetINT1_TrapFLAG:
push dword EBP
push dword EAX
Mov EBP, [DebugStub_CallerESP]
sub dword EBP, 0x4
Mov EAX, [EBP]
and dword EAX, 0xFEFF
Mov [EBP], EAX
pop dword EAX
pop dword EBP

DebugStub_ResetINT1_TrapFLAG_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ResetINT1_TrapFLAG_Exit
Ret

