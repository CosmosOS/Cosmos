; Generated at 3/12/2016 8:37:32 PM



DebugStub_Ping:
Mov AL, 13
Call DebugStub_ComWriteAL
DebugStub_Ping_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_Ping_Exit
Ret

DebugStub_TraceOn:
Mov dword [DebugStub_TraceMode], 1
DebugStub_TraceOn_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_TraceOn_Exit
Ret

DebugStub_TraceOff:
Mov dword [DebugStub_TraceMode], 0
DebugStub_TraceOff_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_TraceOff_Exit
Ret

