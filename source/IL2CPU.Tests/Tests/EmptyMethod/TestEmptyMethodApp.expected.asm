format PE console
entry ___ENTRYPOINT___


section '.code' code readable executable

	___ENTRYPOINT___:
		jmp System_Void___IL2CPU_Tests_Tests_TestEmptyMethodApp_Main____

	System_Void___IL2CPU_Tests_Tests_TestEmptyMethodApp_Main____:
		mov ebp,esp
		; IL: Nop 
		nop
		; IL: Ret 
		ret 

