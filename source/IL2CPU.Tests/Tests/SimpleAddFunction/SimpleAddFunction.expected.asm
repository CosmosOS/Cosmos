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
			pushd ebp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_1 
			pushd 01h

		.L00000002:
			; IL: Ldc_I4_2 
			pushd 02h

		.L00000003:
			; IL: Call System.Int32 Program::Add(System.Int32,System.Int32)
			call System_Int32___Program_Add___System_Int32__System_Int32___
			push eax

		.L00000008:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L00000009:
			; IL: Ret 
			pop ebp
			pop ebp
			ret 

	System_Int32___Program_Add___System_Int32__System_Int32___:
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
			; IL: Ldarg_1 
			mov eax,[ebp + 08h]
			push eax

		.L00000003:
			; IL: Add 
			pop eax
			add eax,[esp]
			add esp,4
			mov [esp],eax

		.L00000004:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L00000005:
			; IL: Br_S Mono.Cecil.Cil.Instruction
			jmp .L00000007

		.L00000007:
			; IL: Ldloc_0 
			mov eax,[ebp - 0Ch]
			push eax

		.L00000008:
			; IL: Ret 
			pop ebp
			pop ebp
			ret 8

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel32_dll_name,rva kernel32_dll_table
	dd 0,0,0,0,0

	kernel32_dll_table:
		ExitProcess dd rva _ExitProcess
		dd 0

	kernel32_dll_name db 'kernel32.dll',0

	_ExitProcess dw 0
	db 'ExitProcess',0
