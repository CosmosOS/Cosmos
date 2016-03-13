; Generated at 3/12/2016 8:37:32 PM

DebugStub_ComAddr dd 0x03F8





%ifndef Exclude_IOPort_Based_Serial



DebugStub_WriteRegister:
Push EDX
Add DX, 0x03F8
Out DX, AL
Pop EDX
DebugStub_WriteRegister_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_WriteRegister_Exit
Ret

DebugStub_ReadRegister:
Push EDX
Add DX, 0x03F8
In AL, DX
Pop EDX
DebugStub_ReadRegister_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ReadRegister_Exit
Ret

%endif

