; Generated at 6/14/2016 12:11:29 PM

DebugStub_AsmBreakEIP dd 0
DebugStub_AsmOrigByte dd 0


DebugStub_DoAsmBreak:
mov dword ESI, [DebugStub_CallerESP]
mov dword EAX, [DebugStub_AsmBreakEIP]
mov dword [ESI - 12], EAX
Call DebugStub_ClearAsmBreak
Call DebugStub_Break

DebugStub_DoAsmBreak_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_DoAsmBreak_Exit
Ret


DebugStub_SetAsmBreak:
Call DebugStub_ClearAsmBreak
Call DebugStub_ComReadEAX
mov dword [DebugStub_AsmBreakEIP], EAX
mov dword EDI, EAX
mov byte AL, [EDI]
mov byte [DebugStub_AsmOrigByte], AL
mov byte AL, 0xCC
mov byte [EDI], AL

DebugStub_SetAsmBreak_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SetAsmBreak_Exit
Ret


DebugStub_ClearAsmBreak:
mov dword EDI, [DebugStub_AsmBreakEIP]
cmp dword EDI, 0x0
JE near DebugStub_ClearAsmBreak_Exit
mov byte AL, [DebugStub_AsmOrigByte]
mov byte [EDI], AL
mov dword [DebugStub_AsmBreakEIP], 0x0

DebugStub_ClearAsmBreak_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ClearAsmBreak_Exit
Ret


DebugStub_SetINT1_TrapFLAG:
push dword EBP
push dword EAX
mov dword EBP, [DebugStub_CallerESP]
sub dword EBP, 0x4
mov dword EAX, [EBP]
or dword EAX, 0x100
mov dword [EBP], EAX
pop dword EAX
pop dword EBP

DebugStub_SetINT1_TrapFLAG_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SetINT1_TrapFLAG_Exit
Ret


DebugStub_ResetINT1_TrapFLAG:
push dword EBP
push dword EAX
mov dword EBP, [DebugStub_CallerESP]
sub dword EBP, 0x4
mov dword EAX, [EBP]
and dword EAX, 0xFEFF
mov dword [EBP], EAX
pop dword EAX
pop dword EBP

DebugStub_ResetINT1_TrapFLAG_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ResetINT1_TrapFLAG_Exit
Ret

