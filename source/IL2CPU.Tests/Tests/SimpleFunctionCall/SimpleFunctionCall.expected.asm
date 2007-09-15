format PE console
entry ___ENTRYPOINT___


section '.code' code readable executable

	___ENTRYPOINT___:
		call System_Void___Program_Main____
		pushd 0
		call [ExitProcess]

	System_Void___Program_Main____:
		mov ebp,esp
		pushd ebp
		; IL: Nop 
		nop
		; IL: Call System.Int32 Program::TheMethod()
		call System_Int32___Program_TheMethod____
		push eax
		; IL: Stloc_0 
		pop eax
		mov [esp - 12],eax
		; IL: Ret 
		pop ebp
		ret 

	System_Int32___Program_TheMethod____:
		mov ebp,esp
		pushd ebp
		; IL: Nop 
		nop
		; IL: Ldc_I4_5 
		pushd 000000005h
		; IL: Stloc_0 
		pop eax
		mov [esp - 12],eax
		; IL: Br_S Mono.Cecil.Cil.Instruction
		; IL: Ldloc_0 
		push eax
		mov eax,[esp - 12]
		; IL: Ret 
		pop ebp
		ret 

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel32_dll_name,rva kernel32_dll_table
	dd 0,0,0,0,0

	kernel32_dll_table:
		ExitProcess dd rva _ExitProcess
		dd 0

	kernel32_dll_name db 'kernel32.dll',0

	_ExitProcess dw 0
	db 'ExitProcess',0
