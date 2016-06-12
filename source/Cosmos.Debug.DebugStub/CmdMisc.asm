; Generated at 6/12/2016 9:24:33 AM



DebugStub_Ping:
mov byte AL, 0xD
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

