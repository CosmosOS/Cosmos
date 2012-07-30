DebugStub_AsmBreakEIP dd 0
DebugStub_AsmOrigByte dd 0




DebugStub_SetAsmBreak:
Call DebugStub_ClearAsmBreak

Call DebugStub_ComReadEAX
Mov EDI, EAX
Mov [DebugStub_AsmBreakEIP], EDI

Mov AX, [EDI + 0]
Mov [DebugStub_AsmOrigByte], AX

Mov AX, 0xCC
Mov [EDI + 0], AX
DebugStub_SetAsmBreak_Exit:
Ret






DebugStub_ClearAsmBreak:
Mov EDI, [DebugStub_AsmBreakEIP]
Cmp EDI, 0
JE DebugStub_ClearAsmBreak_Exit

Mov AX, [DebugStub_AsmOrigByte]
Mov [EDI + 0], AX

Mov dword [DebugStub_AsmBreakEIP], 0
DebugStub_ClearAsmBreak_Exit:
Ret


