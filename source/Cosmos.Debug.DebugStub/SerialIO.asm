; Generated at 6/11/2016 4:16:44 PM

DebugStub_ComAddr dd 1016

%ifndef Exclude_IOPort_Based_Serial

DebugStub_WriteRegister:
Push EDX
Add DX, 0x03F8
out DX, AL
Pop EDX

DebugStub_WriteRegister_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_WriteRegister_Exit
Ret


DebugStub_ReadRegister:
Push EDX
Add DX, 0x03F8
IN AL, DX
Pop EDX

DebugStub_ReadRegister_Exit:
mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_ReadRegister_Exit
Ret

%endif
