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
		pushd 000000002h
		; IL: Call System.Void Program::TheMethod(System.Int32)
		call System_Void___Program_TheMethod___System_Int32___
		; IL: Nop 
		nop
		; IL: Ret 
		ret 

	System_Void___Program_TheMethod___System_Int32___:
		mov ebp,esp
		pushd ebp
		; IL: Nop 
		nop
		; IL: Ldarg_0 
		push eax
		mov eax,[ebp + 4]
		; IL: Stloc_0 
		pop eax
		mov [esp - 12],eax
		; IL: Ret 
		pop ebp
		ret 4

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel32_dll_name,rva kernel32_dll_table
	dd 0,0,0,0,0

	kernel32_dll_table:
		ExitProcess dd rva _ExitProcess
		dd 0

	kernel32_dll_name db 'kernel32.dll',0

	_ExitProcess dw 0
	db 'ExitProcess',0
