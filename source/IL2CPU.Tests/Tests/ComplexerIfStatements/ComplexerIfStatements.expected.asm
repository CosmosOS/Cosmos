format PE console
entry ___ENTRYPOINT___


section '.data' data readable writeable

	static_field__Indy_IL2CPU_RuntimeEngine_HeapHandle dd 0,0,0,0

section '.code' code readable executable

	___ENTRYPOINT___:
			call System_Void___Indy_IL2CPU_RuntimeEngine_InitializeApplication____
			call System_Void___Program_Main____
			pushd 0
			call System_Void___Indy_IL2CPU_RuntimeEngine_FinalizeApplication___System_UInt32___
			;Method: System.Void Indy.IL2CPU.RuntimeEngine::FinalizeApplication(System.UInt32)
			;  Locals:
			;  Args:
			;    [1] aExitCode

	System_Void___Indy_IL2CPU_RuntimeEngine_FinalizeApplication___System_UInt32___:
			push ebp
			mov ebp,esp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Call System.Void Indy.IL2CPU.RuntimeEngine::ShutdownHeap()
			call System_Void___Indy_IL2CPU_RuntimeEngine_ShutdownHeap____

		.L00000006:
			; IL: Nop 
			nop

		.L00000007:
			; IL: Ldarg_0 
			mov eax,[ebp + 08h]
			push eax

		.L00000008:
			; IL: Call System.Void Indy.IL2CPU.PInvokes::Kernel32_ExitProcess(System.UInt32)
			call System_Void___Indy_IL2CPU_PInvokes_Kernel32_ExitProcess___System_UInt32___

		.L0000000D:
			; IL: Nop 
			nop

		.L0000000E:
			; IL: Ret 
			pop ebp
			ret 4
			;Method: System.Void Indy.IL2CPU.PInvokes::Kernel32_ExitProcess(System.UInt32)
			;  (No locals)
			;  Args:
			;    [1] uExitCode

	System_Void___Indy_IL2CPU_PInvokes_Kernel32_ExitProcess___System_UInt32___:
			push ebp
			mov ebp,esp
			mov eax,[ebp + 08h]
			push eax
			call [ExitProcess]
			pushd eax
			pop ebp
			ret 4
			;Method: System.Void Indy.IL2CPU.RuntimeEngine::InitializeApplication()
			;  Locals:
			;  Args:

	System_Void___Indy_IL2CPU_RuntimeEngine_InitializeApplication____:
			push ebp
			mov ebp,esp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Call System.Void Indy.IL2CPU.RuntimeEngine::StartupHeap()
			call System_Void___Indy_IL2CPU_RuntimeEngine_StartupHeap____

		.L00000006:
			; IL: Nop 
			nop

		.L00000007:
			; IL: Ret 
			pop ebp
			ret 
			;Method: System.Void Indy.IL2CPU.RuntimeEngine::ShutdownHeap()
			;  Locals:
			;  Args:

	System_Void___Indy_IL2CPU_RuntimeEngine_ShutdownHeap____:
			push ebp
			mov ebp,esp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldsfld System.IntPtr Indy.IL2CPU.RuntimeEngine::HeapHandle
			pushd [static_field__Indy_IL2CPU_RuntimeEngine_HeapHandle]

		.L00000006:
			; IL: Call System.Boolean Indy.IL2CPU.PInvokes::Kernel32_HeapDestroy(System.IntPtr)
			call System_Boolean___Indy_IL2CPU_PInvokes_Kernel32_HeapDestroy___System_IntPtr___
			push eax

		.L0000000B:
			; IL: Pop 
			pop eax

		.L0000000C:
			; IL: Ldsfld System.IntPtr System.IntPtr::Zero
			pushd 0

		.L00000011:
			; IL: Stsfld System.IntPtr Indy.IL2CPU.RuntimeEngine::HeapHandle
			pop eax
			mov [static_field__Indy_IL2CPU_RuntimeEngine_HeapHandle],eax

		.L00000016:
			; IL: Ret 
			pop ebp
			ret 
			;Method: System.Boolean Indy.IL2CPU.PInvokes::Kernel32_HeapDestroy(System.IntPtr)
			;  (No locals)
			;  Args:
			;    [1] aHeap

	System_Boolean___Indy_IL2CPU_PInvokes_Kernel32_HeapDestroy___System_IntPtr___:
			push ebp
			mov ebp,esp
			mov eax,[ebp + 08h]
			push eax
			call [HeapDestroy]
			pushd eax
			pop eax
			pop ebp
			ret 4
			;Method: System.Void Indy.IL2CPU.RuntimeEngine::StartupHeap()
			;  Locals:
			;  Args:

	System_Void___Indy_IL2CPU_RuntimeEngine_StartupHeap____:
			push ebp
			mov ebp,esp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_0 
			pushd 00h

		.L00000002:
			; IL: Ldc_I4 1024
			pushd 0400h

		.L00000007:
			; IL: Ldc_I4 10240
			pushd 02800h

		.L0000000C:
			; IL: Call System.IntPtr Indy.IL2CPU.PInvokes::Kernel32_HeapCreate(System.UInt32,System.UInt32,System.UInt32)
			call System_IntPtr___Indy_IL2CPU_PInvokes_Kernel32_HeapCreate___System_UInt32__System_UInt32__System_UInt32___
			push eax

		.L00000011:
			; IL: Stsfld System.IntPtr Indy.IL2CPU.RuntimeEngine::HeapHandle
			pop eax
			mov [static_field__Indy_IL2CPU_RuntimeEngine_HeapHandle],eax

		.L00000016:
			; IL: Ret 
			pop ebp
			ret 
			;Method: System.IntPtr Indy.IL2CPU.PInvokes::Kernel32_HeapCreate(System.UInt32,System.UInt32,System.UInt32)
			;  (No locals)
			;  Args:
			;    [1] flOptions
			;    [2] dwInitialSize
			;    [3] dwMaximumSize

	System_IntPtr___Indy_IL2CPU_PInvokes_Kernel32_HeapCreate___System_UInt32__System_UInt32__System_UInt32___:
			push ebp
			mov ebp,esp
			mov eax,[ebp + 08h]
			push eax
			mov eax,[ebp + 0Ch]
			push eax
			mov eax,[ebp + 010h]
			push eax
			call [HeapCreate]
			pushd eax
			pop eax
			pop ebp
			ret 12
			;Method: System.Void Program::Main()
			;  Locals:
			;    [0] V_0
			;    [1] V_1
			;  Args:

	System_Void___Program_Main____:
			push ebp
			mov ebp,esp
			pushd 0
			pushd 0

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_1 
			pushd 01h

		.L00000002:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 04h],eax

		.L00000003:
			; IL: Ldloc_0 
			mov eax,[ebp - 04h]
			push eax

		.L00000004:
			; IL: Call System.Int32 Program::ConditionalFunction(System.Boolean)
			call System_Int32___Program_ConditionalFunction___System_Boolean___
			push eax

		.L00000009:
			; IL: Stloc_1 
			pop eax
			mov [ebp - 08h],eax

		.L0000000A:
			; IL: Ret 
			add esp,4
			add esp,4
			pop ebp
			ret 
			;Method: System.Int32 Program::ConditionalFunction(System.Boolean)
			;  Locals:
			;    [0] V_0
			;    [1] V_1
			;  Args:
			;    [1] aValue

	System_Int32___Program_ConditionalFunction___System_Boolean___:
			push ebp
			mov ebp,esp
			pushd 0
			pushd 0

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldarg_0 
			mov eax,[ebp + 08h]
			push eax

		.L00000002:
			; IL: Ldc_I4_0 
			pushd 00h

		.L00000003:
			; IL: Ceq 
			pop eax
			cmp eax,[esp]
			je .L00000003__True
			jmp .L00000003__False

		.L00000003__True:
			add esp,4
			push 01h
			jmp .L00000005

		.L00000003__False:
			add esp,4
			push 00h
			jmp .L00000005

		.L00000005:
			; IL: Stloc_1 
			pop eax
			mov [ebp - 08h],eax

		.L00000006:
			; IL: Ldloc_1 
			mov eax,[ebp - 08h]
			push eax

		.L00000007:
			; IL: Brtrue_S Mono.Cecil.Cil.Instruction
			popd eax
			cmp eax,01h
			je .L0000001C

		.L00000009:
			; IL: Nop 
			nop

		.L0000000A:
			; IL: Ldarg_0 
			mov eax,[ebp + 08h]
			push eax

		.L0000000B:
			; IL: Ldc_I4_0 
			pushd 00h

		.L0000000C:
			; IL: Ceq 
			pop eax
			cmp eax,[esp]
			je .L0000000C__True
			jmp .L0000000C__False

		.L0000000C__True:
			add esp,4
			push 01h
			jmp .L0000000E

		.L0000000C__False:
			add esp,4
			push 00h
			jmp .L0000000E

		.L0000000E:
			; IL: Stloc_1 
			pop eax
			mov [ebp - 08h],eax

		.L0000000F:
			; IL: Ldloc_1 
			mov eax,[ebp - 08h]
			push eax

		.L00000010:
			; IL: Brtrue_S Mono.Cecil.Cil.Instruction
			popd eax
			cmp eax,01h
			je .L00000017

		.L00000012:
			; IL: Nop 
			nop

		.L00000013:
			; IL: Ldc_I4_4 
			pushd 04h

		.L00000014:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 04h],eax

		.L00000015:
			; IL: Br_S Mono.Cecil.Cil.Instruction
			jmp .L00000021

		.L00000017:
			; IL: Nop 
			nop

		.L00000018:
			; IL: Ldc_I4_6 
			pushd 06h

		.L00000019:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 04h],eax

		.L0000001A:
			; IL: Br_S Mono.Cecil.Cil.Instruction
			jmp .L00000021

		.L0000001C:
			; IL: Nop 
			nop

		.L0000001D:
			; IL: Ldc_I4_2 
			pushd 02h

		.L0000001E:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 04h],eax

		.L0000001F:
			; IL: Br_S Mono.Cecil.Cil.Instruction
			jmp .L00000021

		.L00000021:
			; IL: Ldloc_0 
			mov eax,[ebp - 04h]
			push eax

		.L00000022:
			; IL: Ret 
			pop eax
			add esp,4
			add esp,4
			pop ebp
			ret 4

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel32_dll_name,rva kernel32_dll_table
	dd 0,0,0,0,0

	kernel32_dll_table:
		ExitProcess dd rva _ExitProcess
		HeapDestroy dd rva _HeapDestroy
		HeapCreate dd rva _HeapCreate
		dd 0

	kernel32_dll_name db 'kernel32.dll',0

	_ExitProcess dw 0
	db 'ExitProcess',0
	_HeapDestroy dw 0
	db 'HeapDestroy',0
	_HeapCreate dw 0
	db 'HeapCreate',0
