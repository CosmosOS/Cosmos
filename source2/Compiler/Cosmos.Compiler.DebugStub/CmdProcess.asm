

DebugStub_ProcessCommandNew:
Call DebugStub_ComReadAL
Push EAX

Cmp AL, DebugStub_Const_Vs2Ds_Noop
JE DebugStub_ProcessCommandNew_Exit

Mov EAX, 0
Call DebugStub_ComReadAL
Mov [DebugStub_CommandID], EAX

Mov EAX, [ESP + 0]

Cmp EAX, DebugStub_Const_Vs2Ds_TraceOff
JNE DebugStub_ProcessCommandNew_Block1End
Call DebugStub_TraceOff
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block1End:
Cmp EAX, DebugStub_Const_Vs2Ds_TraceOn
JNE DebugStub_ProcessCommandNew_Block2End
Call DebugStub_TraceOn
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block2End:
Cmp EAX, DebugStub_Const_Vs2Ds_Break
JNE DebugStub_ProcessCommandNew_Block3End
Call DebugStub_Break
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block3End:
Cmp EAX, DebugStub_Const_Vs2Ds_BreakOnAddress
JNE DebugStub_ProcessCommandNew_Block4End
Call DebugStub_BreakOnAddress
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block4End:
Cmp EAX, DebugStub_Const_Vs2Ds_SendMethodContext
JNE DebugStub_ProcessCommandNew_Block5End
Call DebugStub_SendMethodContext
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block5End:
Cmp EAX, DebugStub_Const_Vs2Ds_SendMemory
JNE DebugStub_ProcessCommandNew_Block6End
Call DebugStub_SendMemory
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block6End:
Cmp EAX, DebugStub_Const_Vs2Ds_SendRegisters
JNE DebugStub_ProcessCommandNew_Block7End
Call DebugStub_SendRegisters
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block7End:
Cmp EAX, DebugStub_Const_Vs2Ds_SendFrame
JNE DebugStub_ProcessCommandNew_Block8End
Call DebugStub_SendFrame
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block8End:
Cmp EAX, DebugStub_Const_Vs2Ds_SendStack
JNE DebugStub_ProcessCommandNew_Block9End
Call DebugStub_SendStack
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block9End:
Cmp EAX, DebugStub_Const_Vs2Ds_Ping
JNE DebugStub_ProcessCommandNew_Block10End
Call DebugStub_Ping
Jmp DebugStub_ProcessCommandNew_Exit
DebugStub_ProcessCommandNew_Block10End:

DebugStub_ProcessCommandNew_Exit:
Pop EAX
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

