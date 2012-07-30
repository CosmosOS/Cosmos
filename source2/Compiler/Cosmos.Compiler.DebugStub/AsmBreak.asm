DebugStub_AsmBreakEIP dd 0
DebugStub_AsmOrigByte dd 0




DebugStub_DoAsmBreak:
Mov ESI, [DebugStub_CallerESP]
Mov EAX, [DebugStub_AsmBreakEIP]

Index (zero based) must be greater than or equal to zero and less than the size of the argument list.
