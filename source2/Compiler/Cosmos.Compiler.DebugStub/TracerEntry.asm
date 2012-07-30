


DebugStub_TracerEntry:

Pushad
Mov [DebugStub_PushAllPtr], ESP
Mov [DebugStub_CallerEBP], EBP

Mov EBP, ESP
Add EBP, 32
Mov EAX, [EBP + 0]

Add EBP, 12
Mov [DebugStub_CallerESP], EBP

Dec EAX

Mov [DebugStub_CallerEIP], EAX

Call DebugStub_Executing

Popad

DebugStub_TracerEntry_Exit:
IRet

