



DebugStub_TracerEntry:

Pushad
Mov [DebugStub_PushAllPtr], ESP
Mov [DebugStub_CallerEBP], EBP

Mov EBP, ESP
Add EBP, 32
Mov EAX, [EBP + 0]

Add EBP, 12
Mov [DebugStub_CallerESP], EBP


Mov EBX, EAX
MOV EAX, DR6
And EAX, 0x4000
Cmp EAX, 0x4000
JE DebugStub_TracerEntry_Block1_End
Dec EBX
DebugStub_TracerEntry_Block1_End:
Mov EAX, EBX

Mov [DebugStub_CallerEIP], EAX

Call DebugStub_Executing

Popad

DebugStub_TracerEntry_Exit:
IRet

