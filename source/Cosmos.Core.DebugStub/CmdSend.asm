; Generated at 8/25/2016 4:07:44 PM




		DebugStub_SendRegisters:
			mov byte AL, DebugStub_Const_Ds2Vs_Registers
			Call DebugStub_ComWriteAL
			mov dword ESI, [DebugStub_PushAllPtr]
			mov dword ECX, 0x20
			Call DebugStub_ComWriteX
			mov dword ESI, DebugStub_CallerESP
			Call DebugStub_ComWrite32
			mov dword ESI, DebugStub_CallerEIP
			Call DebugStub_ComWrite32

		DebugStub_SendRegisters_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendRegisters_Exit
			Ret


		DebugStub_SendFrame:
			mov byte AL, DebugStub_Const_Ds2Vs_Frame
			Call DebugStub_ComWriteAL
			mov dword EAX, 0x20
			Call DebugStub_ComWriteAX
			mov dword ESI, [DebugStub_CallerEBP]
			add dword ESI, 0x8
			mov dword ECX, 0x20
			Call DebugStub_ComWriteX

		DebugStub_SendFrame_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendFrame_Exit
			Ret


		DebugStub_SendCommandOnChannel:
			Pushad
			Call DebugStub_ComWriteAL
			Popad
			mov byte AL, BL
			Pushad
			Call DebugStub_ComWriteAL
			Popad
			Pushad
			mov dword EAX, ECX
			Call DebugStub_ComWriteEAX
			Popad

		DebugStub_SendCommandOnChannel_Block1_Begin:
			cmp dword ECX, 0x0
			JE near DebugStub_SendCommandOnChannel_Block1_End
			Call DebugStub_ComWrite8
			dec dword ECX
			Jmp DebugStub_SendCommandOnChannel_Block1_Begin

		DebugStub_SendCommandOnChannel_Block1_End:

		DebugStub_SendCommandOnChannel_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendCommandOnChannel_Exit
			Ret


		DebugStub_SendStack:
			mov byte AL, DebugStub_Const_Ds2Vs_Stack
			Call DebugStub_ComWriteAL
			mov dword ESI, [DebugStub_CallerESP]
			mov dword EAX, [DebugStub_CallerEBP]
			sub dword EAX, ESI
			Call DebugStub_ComWriteAX
			mov dword ESI, [DebugStub_CallerESP]

		DebugStub_SendStack_Block1_Begin:
			cmp dword ESI, [DebugStub_CallerEBP]
			JE near DebugStub_SendStack_Block1_End
			Call DebugStub_ComWrite8
			Jmp DebugStub_SendStack_Block1_Begin

		DebugStub_SendStack_Block1_End:

		DebugStub_SendStack_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendStack_Exit
			Ret


		DebugStub_SendMethodContext:
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_MethodContext
			Call DebugStub_ComWriteAL
			mov dword ESI, [DebugStub_CallerEBP]
			Call DebugStub_ComReadEAX
			add dword ESI, EAX
			Call DebugStub_ComReadEAX
			mov dword ECX, EAX

		DebugStub_SendMethodContext_Block1_Begin:
			cmp dword ECX, 0x0
			JE near DebugStub_SendMethodContext_Block1_End
			Call DebugStub_ComWrite8
			dec dword ECX
			Jmp DebugStub_SendMethodContext_Block1_Begin

		DebugStub_SendMethodContext_Block1_End:

		DebugStub_SendMethodContext_Exit:
			Popad
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendMethodContext_Exit
			Ret


		DebugStub_SendMemory:
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_MemoryData
			Call DebugStub_ComWriteAL
			Call DebugStub_ComReadEAX
			mov dword ESI, EAX
			Call DebugStub_ComReadEAX
			mov dword ECX, EAX

		DebugStub_SendMemory_Block1_Begin:
			cmp dword ECX, 0x0
			JE near DebugStub_SendMemory_Block1_End
			Call DebugStub_ComWrite8
			dec dword ECX
			Jmp DebugStub_SendMemory_Block1_Begin

		DebugStub_SendMemory_Block1_End:

		DebugStub_SendMemory_Exit:
			Popad
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendMemory_Exit
			Ret


		DebugStub_SendTrace:
			mov byte AL, DebugStub_Const_Ds2Vs_BreakPoint
			cmp dword [DebugStub_DebugStatus], DebugStub_Const_Status_Run
			JNE near DebugStub_SendTrace_Block1_End
			mov byte AL, DebugStub_Const_Ds2Vs_TracePoint

		DebugStub_SendTrace_Block1_End:
			Call DebugStub_ComWriteAL
			mov dword ESI, DebugStub_CallerEIP
			Call DebugStub_ComWrite32

		DebugStub_SendTrace_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendTrace_Exit
			Ret


		DebugStub_SendText:
			push dword EBP
			mov dword EBP, ESP
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_Message
			Call DebugStub_ComWriteAL
			mov dword ESI, EBP
			add dword ESI, 0xC
			mov dword ECX, [ESI]
			Call DebugStub_ComWrite16
			mov dword ESI, [EBP + 8]

		DebugStub_SendText_WriteChar:
			cmp dword ECX, 0x0
			JE near DebugStub_SendText_Finalize
			Call DebugStub_ComWrite8
			dec dword ECX
			inc dword ESI
			Jmp DebugStub_SendText_WriteChar

		DebugStub_SendText_Finalize:
			Popad
			pop dword EBP

		DebugStub_SendText_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendText_Exit
			Ret


		DebugStub_SendSimpleNumber:
			push dword EBP
			mov dword EBP, ESP
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_SimpleNumber
			Call DebugStub_ComWriteAL
			mov dword EAX, [EBP + 8]
			Call DebugStub_ComWriteEAX
			Popad
			pop dword EBP

		DebugStub_SendSimpleNumber_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendSimpleNumber_Exit
			Ret


		DebugStub_SendKernelPanic:
			push dword EBP
			mov dword EBP, ESP
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_KernelPanic
			Call DebugStub_ComWriteAL
			mov dword EAX, [EBP + 8]
			Call DebugStub_ComWriteEAX
			Call DebugStub_SendCoreDump
			Popad
			pop dword EBP

		DebugStub_SendKernelPanic_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendKernelPanic_Exit
			Ret


		DebugStub_SendSimpleLongNumber:
			push dword EBP
			mov dword EBP, ESP
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_SimpleLongNumber
			Call DebugStub_ComWriteAL
			mov dword EAX, [EBP + 8]
			Call DebugStub_ComWriteEAX
			mov dword EAX, [EBP + 12]
			Call DebugStub_ComWriteEAX
			Popad
			pop dword EBP

		DebugStub_SendSimpleLongNumber_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendSimpleLongNumber_Exit
			Ret


		DebugStub_SendComplexNumber:
			push dword EBP
			mov dword EBP, ESP
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_ComplexNumber
			Call DebugStub_ComWriteAL
			mov dword EAX, [EBP + 8]
			Call DebugStub_ComWriteEAX
			Popad
			pop dword EBP

		DebugStub_SendComplexNumber_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendComplexNumber_Exit
			Ret


		DebugStub_SendComplexLongNumber:
			push dword EBP
			mov dword EBP, ESP
			Pushad
			mov byte AL, DebugStub_Const_Ds2Vs_ComplexLongNumber
			Call DebugStub_ComWriteAL
			mov dword EAX, [EBP + 8]
			Call DebugStub_ComWriteEAX
			mov dword EAX, [EBP + 12]
			Call DebugStub_ComWriteEAX
			Popad
			pop dword EBP

		DebugStub_SendComplexLongNumber_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendComplexLongNumber_Exit
			Ret


		DebugStub_SendPtr:
			mov byte AL, DebugStub_Const_Ds2Vs_Pointer
			Call DebugStub_ComWriteAL
			mov dword ESI, [EBP + 8]
			Call DebugStub_ComWrite32

		DebugStub_SendPtr_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendPtr_Exit
			Ret


		DebugStub_SendStackCorruptionOccurred:
			mov byte AL, DebugStub_Const_Ds2Vs_StackCorruptionOccurred
			Call DebugStub_ComWriteAL
			mov dword ESI, DebugStub_CallerEIP
			Call DebugStub_ComWrite32

		DebugStub_SendStackCorruptionOccurred_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendStackCorruptionOccurred_Exit
			Ret


		DebugStub_SendStackOverflowOccurred:
			mov byte AL, DebugStub_Const_Ds2Vs_StackOverflowOccurred
			Call DebugStub_ComWriteAL
			mov dword ESI, DebugStub_CallerEIP
			Call DebugStub_ComWrite32

		DebugStub_SendStackOverflowOccurred_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendStackOverflowOccurred_Exit
			Ret


		DebugStub_SendInterruptOccurred:
			push dword EAX
			mov byte AL, DebugStub_Const_Ds2Vs_InterruptOccurred
			Call DebugStub_ComWriteAL
			pop dword EAX
			Call DebugStub_ComWriteEAX

		DebugStub_SendInterruptOccurred_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendInterruptOccurred_Exit
			Ret


		DebugStub_SendNullReferenceOccurred:
			mov byte AL, DebugStub_Const_Ds2Vs_NullReferenceOccurred
			Call DebugStub_ComWriteAL
			mov dword ESI, DebugStub_CallerEIP
			Call DebugStub_ComWrite32

		DebugStub_SendNullReferenceOccurred_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendNullReferenceOccurred_Exit
			Ret


		DebugStub_SendMessageBox:
			mov byte AL, DebugStub_Const_Ds2Vs_MessageBox
			Call DebugStub_ComWriteAL
			mov dword ESI, EBP
			add dword ESI, 0xC
			mov dword ECX, [ESI]
			Call DebugStub_ComWrite16
			mov dword ESI, [EBP + 8]

		DebugStub_SendMessageBox_WriteChar:
			cmp dword ECX, 0x0
			JE near DebugStub_SendMessageBox_Exit
			Call DebugStub_ComWrite8
			dec dword ECX
			inc dword ESI
			Jmp DebugStub_SendMessageBox_WriteChar

		DebugStub_SendMessageBox_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendMessageBox_Exit
			Ret


		DebugStub_SendCoreDump:
			push dword EAX
			push dword EBX
			push dword ECX
			push dword EDX
			push dword EDI
			push dword ESI
			mov dword EAX, DebugStub_CallerEBP
			push dword EAX
			mov dword EAX, DebugStub_CallerEIP
			push dword EAX
			mov dword EAX, DebugStub_CallerESP
			push dword EAX
			mov dword ECX, 0x24
			mov dword EAX, EBP

		DebugStub_SendCoreDump_Block1_Begin:
			cmp dword EAX, 0x0
			JE near DebugStub_SendCoreDump_Block1_End
			push dword EAX
			mov dword EAX, [EAX]
			Jmp DebugStub_SendCoreDump_Block1_Begin

		DebugStub_SendCoreDump_Block1_End:
			mov byte AL, DebugStub_Const_Ds2Vs_CoreDump
			Call DebugStub_ComWriteAL
			mov dword EAX, ECX
			Call DebugStub_ComWriteAX

		DebugStub_SendCoreDump_Block2_Begin:
			cmp dword ECX, 0x0
			JE near DebugStub_SendCoreDump_Block2_End
			pop dword EAX
			Call DebugStub_ComWriteEAX
			dec dword ECX
			Jmp DebugStub_SendCoreDump_Block2_Begin

		DebugStub_SendCoreDump_Block2_End:

		DebugStub_SendCoreDump_Exit:
			mov dword [static_field__Cosmos_Core_INTs_mLastKnownAddress], DebugStub_SendCoreDump_Exit
			Ret


