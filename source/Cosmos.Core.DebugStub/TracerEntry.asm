; Generated at 6/14/2016 12:11:29 PM



DebugStub_TracerEntry:
cli
Pushad
mov dword [DebugStub_PushAllPtr], ESP
mov dword [DebugStub_CallerEBP], EBP
mov dword EBP, ESP
add dword EBP, 0x20
mov dword EAX, [EBP]
add dword EBP, 0xC
mov dword [DebugStub_CallerESP], EBP
mov dword EBX, EAX
MOV EAX, DR6
and dword EAX, 0x4000
cmp dword EAX, 0x4000
JE near DebugStub_TracerEntry_Block1_End
dec dword EBX

DebugStub_TracerEntry_Block1_End:
mov dword EAX, EBX
mov dword [DebugStub_CallerEIP], EAX
Call DebugStub_Executing
Popad
sti

DebugStub_TracerEntry_Exit:
iret
