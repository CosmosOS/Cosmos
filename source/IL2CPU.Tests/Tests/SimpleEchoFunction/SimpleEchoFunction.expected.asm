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
		; IL: Nop 
		nop
		; IL: Ldc_I4_0 
		pushd 00h
		; IL: Stloc_0 
		pop eax
		mov [ebp - 0Ch],eax
		; IL: Ldc_I4_5 
		pushd 05h
		; IL: Call System.Int32 Program::DoEcho(System.Int32)
		call System_Int32___Program_DoEcho___System_Int32___
		push eax
		; IL: Stloc_0 
		pop eax
		mov [ebp - 0Ch],eax
		; IL: Ret 
		pop ebp
		pop ebp
		ret 

	System_Int32___Program_DoEcho___System_Int32___:
		mov ebp,esp
		pushd ebp
		pushd ebp
		; IL: Nop 
		nop
		; IL: Ldarg_0 
		push eax
		mov eax,[ebp + 04h]
		; IL: Stloc_0 
		pop eax
		mov [ebp - 0Ch],eax
		; IL: Br_S Mono.Cecil.Cil.Instruction
		; IL: Ldloc_0 
		mov eax,[ebp - 0Ch]
		push eax
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
