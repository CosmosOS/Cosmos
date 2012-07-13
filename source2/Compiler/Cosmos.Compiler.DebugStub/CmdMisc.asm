

DebugStub_Ping:
Mov AL, 13
Call DebugStub_ComWriteAL
DebugStub_Ping_Exit:
Ret

DebugStub_TraceOn:
Mov dword [DebugStub_TraceMode], 1
DebugStub_TraceOn_Exit:
Ret

DebugStub_TraceOff:
Mov dword [DebugStub_TraceMode], 0
DebugStub_TraceOff_Exit:
Ret

