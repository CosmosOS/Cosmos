

DebugStub_ProcessCommand2:
Call DebugStub_ComReadAL
Push EAX

Cmp AL, DebugStub_Const_Vs2Ds_Noop
JE DebugStub_ProcessCommand2_Exit

Mov EAX, 0
Call DebugStub_ComReadAL
Mov [DebugStub_CommandID], EAX

Mov EAX, [ESP + 0]

Cmp EAX, DebugStub_Const_Vs2Ds_TraceOff
JNE DebugStub_ProcessCommand2_Block1End
Call DebugStub_TraceOff
Jmp DebugStub_ProcessCommand2_Exit
DebugStub_ProcessCommand2_Block1End:

DebugStub_ProcessCommand2_Exit:
Pop EAX
Ret

DebugStub_CheckCmd2:
DebugStub_CheckCmd2_Exit:
Ret

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

