format PE console
entry ___ENTRYPOINT___


section '.data' data readable writeable

	static_field__Indy_IL2CPU_RuntimeEngine_HeapHandle dd 0,0,0,0

section '.code' code readable executable

	___ENTRYPOINT___:
			call System_Void___Indy_IL2CPU_RuntimeEngine_InitializeApplication____
			call System_Void___IL2CPU_Tests_Tests_TestEmptyMethodApp_Main____
			pushd 0
			call System_Void___Indy_IL2CPU_RuntimeEngine_FinalizeApplication___System_UInt32___

	System_Void___IL2CPU_Tests_Tests_TestEmptyMethodApp_Main____:
			push ebp
			mov ebp,esp
			pushd 0

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_5 
			pushd 05h

		.L00000002:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 0Ch],eax

		.L00000003:
			; IL: Ret 
			add esp,4
			pop ebp
			ret 

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

	System_Void___Indy_IL2CPU_PInvokes_Kernel32_ExitProcess___System_UInt32___:
			push ebp
			mov ebp,esp
			mov eax,[ebp + 08h]
			push eax
			call [ExitProcess]
			pushd eax
			pop ebp
			ret 4

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
