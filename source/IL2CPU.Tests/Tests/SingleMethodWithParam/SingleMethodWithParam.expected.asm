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

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel_name,rva kernel_table
	dd 0,0,0,0,0

	kernel_table:
		ExitProcess dd rva _ExitProcess
		dd 0

	kernel_name db 'KERNEL32.DLL',0

	_ExitProcess dw 0
	db 'ExitProcess',0
