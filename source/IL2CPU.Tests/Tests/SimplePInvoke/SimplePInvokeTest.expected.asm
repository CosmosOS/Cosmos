format PE console
entry ___ENTRYPOINT___


section '.data' data readable writeable

	StringLiteral00000000 db 72,101,108,108,111,44,32,87,111,114,108,100,33,0

section '.code' code readable executable

	___ENTRYPOINT___:
			call System_Void___SimplePInvokeTest_Program_Main____
			pushd 0
			call [ExitProcess]

	System_Void___SimplePInvokeTest_Program_Main____:
			mov ebp,esp
			pushd ebp
			pushd ebp
			pushd ebp
			pushd ebp
			pushd ebp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_0 
			pushd 00h

		.L00000002:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L00000003:
			; IL: Ldstr Hello, World!
			mov eax,StringLiteral00000000
			pushd eax

		.L00000008:
			; IL: Stloc_1 
			pop eax
			mov [ebp - 010h],eax

		.L00000009:
			; IL: Ldc_I4_M1 
			pushd 0FFFFFFFFh

		.L0000000A:
			; IL: Call System.Boolean SimplePInvokeTest.Program::MessageBeep(System.UInt32)
			call System_Boolean___SimplePInvokeTest_Program_MessageBeep___System_UInt32___
			push eax

		.L0000000F:
			; IL: Stloc_2 
			pop eax
			mov [ebp - 014h],eax

		.L00000010:
			; IL: Call System.UInt32 SimplePInvokeTest.Program::GetLastError()
			call System_UInt32___SimplePInvokeTest_Program_GetLastError____
			push eax

		.L00000015:
			; IL: Stloc_3 
			pop eax
			mov [ebp - 018h],eax

		.L00000016:
			; IL: Ret 
			pop ebp
			pop ebp
			pop ebp
			pop ebp
			pop ebp
			ret 

	System_UInt32___SimplePInvokeTest_Program_GetLastError____:
			mov ebp,esp
			pushd ebp
			call [GetLastError]
			pop ebp
			ret 

	System_Boolean___SimplePInvokeTest_Program_MessageBeep___System_UInt32___:
			mov ebp,esp
			pushd ebp
			mov eax,[ebp + 04h]
			push eax
			call [MessageBeep]
			pop ebp
			ret 4

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel32_dll_name,rva kernel32_dll_table
	dd 0,0,0,rva user32_dll_name,rva user32_dll_table
	dd 0,0,0,0,0

	kernel32_dll_table:
		ExitProcess dd rva _ExitProcess
		GetLastError dd rva _GetLastError
		dd 0

	user32_dll_table:
		MessageBeep dd rva _MessageBeep
		dd 0

	kernel32_dll_name db 'kernel32.dll',0
	user32_dll_name db 'user32.dll',0

	_ExitProcess dw 0
	db 'ExitProcess',0
	_GetLastError dw 0
	db 'GetLastError',0
	_MessageBeep dw 0
	db 'MessageBeep',0
