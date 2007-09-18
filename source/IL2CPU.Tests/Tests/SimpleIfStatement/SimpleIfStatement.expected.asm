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
			pushd ebp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_1 
			pushd 01h

		.L00000002:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L00000003:
			; IL: Ldloc_0 
			mov eax,[ebp - 0Ch]
			push eax

		.L00000004:
			; IL: Call System.Int32 Program::ConditionalFunction(System.Boolean)
			call System_Int32___Program_ConditionalFunction___System_Boolean___
			push eax

		.L00000009:
			; IL: Stloc_1 
			pop eax
			mov [ebp - 010h],eax

		.L0000000A:
			; IL: Ret 
			pop ebp
			pop ebp
			pop ebp
			ret 

	System_Int32___Program_ConditionalFunction___System_Boolean___:
			mov ebp,esp
			pushd ebp
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
			; IL: Ldc_I4_0 
			pushd 00h

		.L00000003:
			; IL: Ceq 
			pop eax
			cmp eax,[esp]
			add esp,4
			je .L00000003__True
			jmp .L00000003__False

		.L00000003__True:
			push 01h
			pop eax
			mov [esp],eax
			jmp .L00000005

		.L00000003__False:
			push 00h
			pop eax
			mov [esp],eax
			jmp .L00000005

		.L00000005:
			; IL: Stloc_1 
			pop eax
			mov [ebp - 010h],eax

		.L00000006:
			; IL: Ldloc_1 
			mov eax,[ebp - 010h]
			push eax

		.L00000007:
			; IL: Brtrue_S Mono.Cecil.Cil.Instruction
			popd eax
			cmp eax,01h
			je .L0000000E

		.L00000009:
			; IL: Nop 
			nop

		.L0000000A:
			; IL: Ldc_I4_4 
			pushd 03h

		.L0000000B:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L0000000C:
			; IL: Br_S Mono.Cecil.Cil.Instruction
			jmp .L00000013

		.L0000000E:
			; IL: Nop 
			nop

		.L0000000F:
			; IL: Ldc_I4_2 
			pushd 02h

		.L00000010:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L00000011:
			; IL: Br_S Mono.Cecil.Cil.Instruction
			jmp .L00000013

		.L00000013:
			; IL: Ldloc_0 
			mov eax,[ebp - 0Ch]
			push eax

		.L00000014:
			; IL: Ret 
			pop ebp
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
