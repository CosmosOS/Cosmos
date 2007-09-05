format PE console
entry ___ENTRYPOINT___


section '.code' code readable executable

	___ENTRYPOINT___:
		jmp System_Void___IL2CPU_Tests_Tests_TestEmptyMethodApp_Main____

	System_Void___IL2CPU_Tests_Tests_TestEmptyMethodApp_Main____:
		mov ebp,esp
		pushd ebp
		; IL: Nop 
		nop
		; IL: Ldc_I4_5 
		pushd 5
		; IL: Stloc_0 
		pop eax
		mov [ebp + 12],eax
		; IL: Ret 
		pop ebp
		ret 

