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

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_2 
			pushd 02h

		.L00000002:
			; IL: Call System.Void Program::TheMethod(System.Int32)
			call System_Void___Program_TheMethod___System_Int32___

		.L00000007:
			; IL: Nop 
			nop

		.L00000008:
			; IL: Ret 
			pop ebp
			ret 

	System_Void___Program_TheMethod___System_Int32___:
			mov ebp,esp
			pushd ebp
			pushd ebp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldarg_0 
			mov eax,[ebp + 04h]
			push eax

		.L00000002:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L00000003:
			; IL: Ret 
			pop ebp
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
