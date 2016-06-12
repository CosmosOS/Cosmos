; Generated at 6/12/2016 3:23:24 PM



DebugStub_Ping:
mov byte AL, 0xD
Call DebugStub_ComWriteAL

DebugStub_Ping_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_Ping_Exit
Ret


DebugStub_TraceOn:
mov dword [DebugStub_TraceMode], 0x1

DebugStub_TraceOn_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_TraceOn_Exit
Ret


DebugStub_TraceOff:
mov dword [DebugStub_TraceMode], 0x0

DebugStub_TraceOff_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_TraceOff_Exit
Ret

