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
		; IL: Nop 
		nop
		; IL: Ldc_I4_0 
		pushd 000000000h
		; IL: Stloc_0 
		pop eax
		mov [esp - 12],eax
		; IL: Ldstr Hello, World!
		mov eax,StringLiteral00000000
		pushd eax
		; IL: Stloc_1 
		pop eax
		mov [esp - 16],eax
		; IL: Ldc_I4_M1 
		pushd 0FFFFFFFFh
		; IL: Call System.Boolean SimplePInvokeTest.Program::MessageBeep(System.UInt32)
		call System_Boolean___SimplePInvokeTest_Program_MessageBeep___System_UInt32___
		push eax
		; IL: Stloc_2 
		pop eax
		mov [esp - 20],eax
		; IL: Call System.UInt32 SimplePInvokeTest.Program::GetLastError()
		call System_UInt32___SimplePInvokeTest_Program_GetLastError____
		push eax
		; IL: Stloc_3 
		pop eax
		mov [esp - 24],eax
		; IL: Ret 
		pop ebp
		pop ebp
		pop ebp
		pop ebp
		ret 

	System_UInt32___SimplePInvokeTest_Program_GetLastError____:
		mov ebp,esp
		call [GetLastError]
		ret 

	System_Boolean___SimplePInvokeTest_Program_MessageBeep___System_UInt32___:
		mov ebp,esp
		mov eax,[ebp + 4]
		push eax
		call [MessageBeep]
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
