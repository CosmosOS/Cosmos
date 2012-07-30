DebugStub_CallerEBP dd 0
DebugStub_CallerEIP dd 0
DebugStub_CallerESP dd 0
DebugStub_TraceMode dd 0
DebugStub_DebugStatus dd 0
DebugStub_PushAllPtr dd 0
DebugStub_DebugBreakOnNextTrace dd 0
DebugStub_BreakEBP dd 0
DebugStub_CommandID dd 0




DebugStub_BreakOnAddress:
Pushad
Call DebugStub_ComReadEAX
Mov ECX, EAX

Mov EAX, 0
Call DebugStub_ComReadAL

Mov EBX, DebugStub_DebugBPs
SHL EAX, 2
Add EBX, EAX

Mov [EBX + 0], ECX
DebugStub_BreakOnAddress_Exit:
Popad
Ret

DebugStub_Executing:

Mov EAX, [DebugStub_CallerEIP]
Cmp EAX, [DebugStub_AsmBreakEIP]
JNE DebugStub_Executing_Block1_End
Call DebugStub_ClearAsmBreak
Call DebugStub_Break
Jmp DebugStub_Executing_Normal
DebugStub_Executing_Block1_End:

Mov EAX, [DebugStub_CallerEIP]
Mov EDI, DebugStub_DebugBPs
Mov ECX, 256
repne scasd
JNE DebugStub_Executing_Block2_End
Call DebugStub_Break
Jmp DebugStub_Executing_Normal
DebugStub_Executing_Block2_End:


Cmp dword [DebugStub_DebugBreakOnNextTrace], DebugStub_Const_StepTrigger_Into
JNE DebugStub_Executing_Block3_End
Call DebugStub_Break
Jmp DebugStub_Executing_Normal
DebugStub_Executing_Block3_End:

Mov EAX, [DebugStub_CallerEBP]

Cmp dword [DebugStub_DebugBreakOnNextTrace], DebugStub_Const_StepTrigger_Over
JNE DebugStub_Executing_Block4_End
Cmp EAX, [DebugStub_BreakEBP]
JB DebugStub_Executing_Block5_End
Call DebugStub_Break
DebugStub_Executing_Block5_End:
Jmp DebugStub_Executing_Normal
DebugStub_Executing_Block4_End:

Cmp dword [DebugStub_DebugBreakOnNextTrace], DebugStub_Const_StepTrigger_Out
JNE DebugStub_Executing_Block6_End
Cmp EAX, [DebugStub_BreakEBP]
JBE DebugStub_Executing_Block7_End
Call DebugStub_Break
DebugStub_Executing_Block7_End:
Jmp DebugStub_Executing_Normal
DebugStub_Executing_Block6_End:

DebugStub_Executing_Normal:
Cmp dword [DebugStub_TraceMode], DebugStub_Const_Tracing_On
JNE DebugStub_Executing_Block8_End
Call DebugStub_SendTrace
DebugStub_Executing_Block8_End:

DebugStub_Executing_CheckForCmd:
Mov DX, [DebugStub_ComAddr]
Add DX, 5
In AL, DX
Test AL, 1
JZ DebugStub_Executing_Block9_End
Call DebugStub_ProcessCommand
Jmp DebugStub_Executing_CheckForCmd
DebugStub_Executing_Block9_End:
DebugStub_Executing_Exit:
Ret

DebugStub_Break:
Mov dword [DebugStub_DebugBreakOnNextTrace], DebugStub_Const_StepTrigger_None
Mov dword [DebugStub_BreakEBP], 0
Mov dword [DebugStub_DebugStatus], DebugStub_Const_Status_Break
Call DebugStub_SendTrace

DebugStub_Break_WaitCmd:
Call DebugStub_ProcessCommand


Cmp AL, DebugStub_Const_Vs2Ds_Continue
JE DebugStub_Break_Done

Cmp AL, DebugStub_Const_Vs2Ds_SetAsmBreak
JNE DebugStub_Break_Block1_End
Call DebugStub_SetAsmBreak
Call DebugStub_AckCommand
Jmp DebugStub_Break_WaitCmd
DebugStub_Break_Block1_End:

Cmp AL, DebugStub_Const_Vs2Ds_StepInto
JNE DebugStub_Break_Block2_End
Mov dword [DebugStub_DebugBreakOnNextTrace], DebugStub_Const_StepTrigger_Into
Mov [DebugStub_BreakEBP], EAX
Jmp DebugStub_Break_Done
DebugStub_Break_Block2_End:

Cmp AL, DebugStub_Const_Vs2Ds_StepOver
JNE DebugStub_Break_Block3_End
Mov dword [DebugStub_DebugBreakOnNextTrace], DebugStub_Const_StepTrigger_Over
Mov EAX, [DebugStub_CallerEBP]
Mov [DebugStub_BreakEBP], EAX
Jmp DebugStub_Break_Done
DebugStub_Break_Block3_End:

Cmp AL, DebugStub_Const_Vs2Ds_StepOut
JNE DebugStub_Break_Block4_End
Mov dword [DebugStub_DebugBreakOnNextTrace], DebugStub_Const_StepTrigger_Out
Mov EAX, [DebugStub_CallerEBP]
Mov [DebugStub_BreakEBP], EAX
Jmp DebugStub_Break_Done
DebugStub_Break_Block4_End:

Jmp DebugStub_Break_WaitCmd

DebugStub_Break_Done:
Call DebugStub_AckCommand
Mov dword [DebugStub_DebugStatus], DebugStub_Const_Status_Run
DebugStub_Break_Exit:
Ret


