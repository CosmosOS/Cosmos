

DebugStub_ProcessCommand:
Call DebugStub_ComReadAL
Push EAX

Cmp AL, DebugStub_Const_Vs2Ds_Noop
JE DebugStub_ProcessCommand_Exit

Mov EAX, 0
Call DebugStub_ComReadAL
Mov [DebugStub_CommandID], EAX

Mov EAX, [ESP + 0]

Cmp AL, DebugStub_Const_Vs2Ds_TraceOff
JNE DebugStub_ProcessCommand_Block1_End
Call DebugStub_TraceOff
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block1_End:
Cmp AL, DebugStub_Const_Vs2Ds_TraceOn
JNE DebugStub_ProcessCommand_Block2_End
Call DebugStub_TraceOn
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block2_End:
Cmp AL, DebugStub_Const_Vs2Ds_Break
JNE DebugStub_ProcessCommand_Block3_End
Call DebugStub_Break
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block3_End:
Cmp AL, DebugStub_Const_Vs2Ds_BreakOnAddress
JNE DebugStub_ProcessCommand_Block4_End
Call DebugStub_BreakOnAddress
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block4_End:
Cmp AL, DebugStub_Const_Vs2Ds_SendMethodContext
JNE DebugStub_ProcessCommand_Block5_End
Call DebugStub_SendMethodContext
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block5_End:
Cmp AL, DebugStub_Const_Vs2Ds_SendMemory
JNE DebugStub_ProcessCommand_Block6_End
Call DebugStub_SendMemory
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block6_End:
Cmp AL, DebugStub_Const_Vs2Ds_SendRegisters
JNE DebugStub_ProcessCommand_Block7_End
Call DebugStub_SendRegisters
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block7_End:
Cmp AL, DebugStub_Const_Vs2Ds_SendFrame
JNE DebugStub_ProcessCommand_Block8_End
Call DebugStub_SendFrame
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block8_End:
Cmp AL, DebugStub_Const_Vs2Ds_SendStack
JNE DebugStub_ProcessCommand_Block9_End
Call DebugStub_SendStack
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block9_End:
Cmp AL, DebugStub_Const_Vs2Ds_Ping
JNE DebugStub_ProcessCommand_Block10_End
Call DebugStub_Ping
Call DebugStub_AckCommand
Jmp DebugStub_ProcessCommand_Exit
DebugStub_ProcessCommand_Block10_End:


DebugStub_ProcessCommand_Exit:
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

