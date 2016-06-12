; Generated at 6/12/2016 10:15:08 AM



DebugStub_TracerEntry:
cli
Pushad
Mov [DebugStub_PushAllPtr], ESP
Mov [DebugStub_CallerEBP], EBP
Mov EBP, ESP
add dword EBP, 0x20
Mov EAX, [EBP + 0]
add dword EBP, 0xC
Mov [DebugStub_CallerESP], EBP
Mov EBX, EAX
MOV EAX, DR6
and dword EAX, 0x4000
Cmp EAX, 0x4000
JE near DebugStub_TracerEntry_Block1_End
dec dword EBX

DebugStub_TracerEntry_Block1_End:
Mov EAX, EBX
Mov [DebugStub_CallerEIP], EAX
Call DebugStub_Executing
Popad
sti

DebugStub_TracerEntry_Exit:
iret
