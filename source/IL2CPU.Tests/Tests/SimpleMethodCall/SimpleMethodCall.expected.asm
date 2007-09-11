format PE console
entry ___ENTRYPOINT___


section '.code' code readable executable

	___ENTRYPOINT___:
		call System_Void___Program_Main____
		pushd 0
		call [ExitProcess]

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

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel_name,rva kernel_table
	dd 0,0,0,0,0

	kernel_table:
		ExitProcess dd rva _ExitProcess
		dd 0

	kernel_name db 'KERNEL32.DLL',0

	_ExitProcess dw 0
	db 'ExitProcess',0
