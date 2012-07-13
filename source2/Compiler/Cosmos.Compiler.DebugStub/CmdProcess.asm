

DebugStub_AckCommand:

Mov AL, DebugStub_Const_Ds2Vs_CmdCompleted
Call DebugStub_ComWriteAL

Mov EAX, [DebugStub_CommandID]
Call DebugStub_ComWriteAL
DebugStub_AckCommand_Exit:
Ret

DebugStub_ProcessCommandBatch:
DebugStub_ProcessCommandBatch_Begin:
Call DebugStub_ProcessCommand

Cmp AL, 8
JNE DebugStub_ProcessCommandBatch_Begin

Call DebugStub_AckCommand
DebugStub_ProcessCommandBatch_Exit:
Ret

