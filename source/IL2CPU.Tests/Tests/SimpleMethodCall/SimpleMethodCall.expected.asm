format PE console
entry ___ENTRYPOINT___


section '.code' code readable executable

	___ENTRYPOINT___:
		jmp System_Void___Program_Main____

	System_Void___Program_Main____:
		mov ebp,esp
		; IL: Nop 
		nop
		; IL: Call System.Void Program::TheMethod()
		call System_Void___Program_TheMethod____
		; IL: Nop 
		nop
		; IL: Ret 
		ret 

	System_Void___Program_TheMethod____:
		mov ebp,esp
		; IL: Nop 
		nop
		; IL: Ret 
		ret 

