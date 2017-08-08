; Generated at 14-6-2016 12:37:38


	DebugWaitMsg db 87, 97, 105, 116, 105, 110, 103, 32, 102, 111, 114, 32, 100, 101, 98, 117, 103, 103, 101, 114, 32, 99, 111, 110, 110, 101, 99, 116, 105, 111, 110, 46, 46, 46, 0

			%ifndef Exclude_Memory_Based_Console
			DebugStub_Const_VidBase equ 753664

		DebugStub_Cls:
			mov dword ESI, DebugStub_Const_VidBase

		DebugStub_Cls_Block1_Begin:
			cmp dword ESI, 0xB8FA0
			JNB near DebugStub_Cls_Block1_End
			mov dword [ESI], 0x0
			inc dword ESI
			mov dword [ESI], 0xA
			inc dword ESI
			Jmp DebugStub_Cls_Block1_Begin

		DebugStub_Cls_Block1_End:

		DebugStub_Cls_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_Cls_Exit
			Ret


		DebugStub_DisplayWaitMsg:
			mov dword ESI, DebugWaitMsg
			mov dword EDI, DebugStub_Const_VidBase
			add dword EDI, 0x668

		DebugStub_DisplayWaitMsg_Block1_Begin:
			cmp byte [ESI], 0x0
			JE near DebugStub_DisplayWaitMsg_Block1_End
			mov byte AL, [ESI]
			mov byte [EDI], AL
			inc dword ESI
			add dword EDI, 0x2
			Jmp DebugStub_DisplayWaitMsg_Block1_Begin

		DebugStub_DisplayWaitMsg_Block1_End:

		DebugStub_DisplayWaitMsg_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_DisplayWaitMsg_Exit
			Ret

			%endif

