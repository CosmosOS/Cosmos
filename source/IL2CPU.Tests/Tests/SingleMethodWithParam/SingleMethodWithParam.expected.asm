format PE console
entry ___ENTRYPOINT___


section '.code' code readable executable

	___ENTRYPOINT___:
		jmp System_Void___Program_Main____

	System_Void___Program_Main____:
		mov ebp,esp
		; IL: Nop 
		nop
		; IL: Ldc_I4_2 
		pushd 2
		; IL: Call System.Void Program::TheMethod(System.Int32)
		call System_Void___Program_TheMethod___System_Int32___
		; IL: Nop 
		nop
		; IL: Ret 
		ret 

	System_Void___Program_TheMethod___System_Int32___:
		mov ebp,esp
		; IL: Nop 
		nop
		; IL: Ret 
		ret 4

