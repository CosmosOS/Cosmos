; Generated at 6/14/2016 12:11:29 PM

DebugStub_ComAddr dd 1016

%ifndef Exclude_IOPort_Based_Serial

DebugStub_WriteRegister:
push dword EDX
add word DX, 0x3F8
out DX, AL
pop dword EDX

DebugStub_WriteRegister_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_WriteRegister_Exit
Ret


DebugStub_ReadRegister:
push dword EDX
add word DX, 0x3F8
in byte AL, DX
pop dword EDX

DebugStub_ReadRegister_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ReadRegister_Exit
Ret

%endif
