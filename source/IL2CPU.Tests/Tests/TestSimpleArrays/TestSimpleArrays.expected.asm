format PE console
entry ___ENTRYPOINT___


section '.data' data readable writeable

	static_field___PrivateImplementationDetails__FBC97C64_9997_4E9D_A969_8DB06BAF3B33____method0x6000001_1 db 16,0,0,0,1,0,0,0,2,0,0,0,3,0,0,0,4,0,0,0
	static_field__Indy_IL2CPU_RuntimeEngine_HeapHandle dd 0,0,0,0

section '.code' code readable executable

	___ENTRYPOINT___:
			call System_Void___Indy_IL2CPU_RuntimeEngine_InitializeApplication____
			call System_Void___ConsoleDrv_Main____
			pushd 0
			call System_Void___Indy_IL2CPU_RuntimeEngine_FinalizeApplication___System_UInt32___
			;Method: System.Void ConsoleDrv::Main()
			;  Locals:
			;    [0] V_0
			;    [1] V_1
			;  Args:

	System_Void___ConsoleDrv_Main____:
			push ebp
			mov ebp,esp
			pushd 0
			pushd 0

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldc_I4_4 
			pushd 04h

		.L00000002:
			; IL: Newarr System.Int32
			pushd 04h
			pop eax
			mul dword [esp]
			add esp,4
			pushd eax
			pushd 0Ch
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax
			call System_IntPtr___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___
			pushd eax
			mov dword [eax + 4],000000002h
			pushd eax
			call System_Void___System_Array__ctor____
			pop eax
			pushd eax

		.L00000007:
			; IL: Dup 
			pop eax
			pushd eax
			pushd eax

		.L00000008:
			; IL: Ldtoken <PrivateImplementationDetails>{FBC97C64-9997-4E9D-A969-8DB06BAF3B33}/__StaticArrayInitTypeSize=16 <PrivateImplementationDetails>{FBC97C64-9997-4E9D-A969-8DB06BAF3B33}::$$method0x6000001-1
			pushd static_field___PrivateImplementationDetails__FBC97C64_9997_4E9D_A969_8DB06BAF3B33____method0x6000001_1

		.L0000000D:
			; IL: Call System.Void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array,System.RuntimeFieldHandle)
			call System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___

		.L00000012:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 04h],eax

		.L00000013:
			; IL: Ldloc_0 
			mov eax,[ebp - 04h]
			push eax

		.L00000014:
			; IL: Ldc_I4_0 
			pushd 00h

		.L00000015:
			; IL: Ldelem_I4 
			pop eax
			mov edx,4
			mul edx
			pushd eax
			pushd 0Ch
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax
			pop eax
			pop edx
			add edx,eax
			mov eax,[edx]
			pushd eax

		.L00000016:
			; IL: Ldloc_0 
			mov eax,[ebp - 04h]
			push eax

		.L00000017:
			; IL: Ldc_I4_1 
			pushd 01h

		.L00000018:
			; IL: Ldelem_I4 
			pop eax
			mov edx,4
			mul edx
			pushd eax
			pushd 0Ch
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax
			pop eax
			pop edx
			add edx,eax
			mov eax,[edx]
			pushd eax

		.L00000019:
			; IL: Add 
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax

		.L0000001A:
			; IL: Ldloc_0 
			mov eax,[ebp - 04h]
			push eax

		.L0000001B:
			; IL: Ldc_I4_2 
			pushd 02h

		.L0000001C:
			; IL: Ldelem_I4 
			pop eax
			mov edx,4
			mul edx
			pushd eax
			pushd 0Ch
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax
			pop eax
			pop edx
			add edx,eax
			mov eax,[edx]
			pushd eax

		.L0000001D:
			; IL: Add 
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax

		.L0000001E:
			; IL: Ldloc_0 
			mov eax,[ebp - 04h]
			push eax

		.L0000001F:
			; IL: Ldc_I4_3 
			pushd 03h

		.L00000020:
			; IL: Ldelem_I4 
			pop eax
			mov edx,4
			mul edx
			pushd eax
			pushd 0Ch
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax
			pop eax
			pop edx
			add edx,eax
			mov eax,[edx]
			pushd eax

		.L00000021:
			; IL: Add 
			pop eax
			add eax,[esp]
			add esp,4
			pushd eax

		.L00000022:
			; IL: Stloc_1 
			pop eax
			mov [ebp - 08h],eax

		.L00000023:
			; IL: Ret 
			add esp,4
			add esp,4
			pop ebp
			ret 
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
			;Method: System.IntPtr Indy.IL2CPU.RuntimeEngine::Heap_AllocNewObject(System.UInt32)
			;  Locals:
			;    [0] V_0
			;  Args:
			;    [1] aSize

	System_IntPtr___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___:
			push ebp
			mov ebp,esp
			pushd 0

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ldsfld System.IntPtr Indy.IL2CPU.RuntimeEngine::HeapHandle
			pushd [static_field__Indy_IL2CPU_RuntimeEngine_HeapHandle]

		.L00000006:
			; IL: Ldc_I4_8 
			pushd 08h

		.L00000007:
			; IL: Ldarg_0 
			mov eax,[ebp + 08h]
			push eax

		.L00000008:
			; IL: Call System.IntPtr Indy.IL2CPU.PInvokes::Kernel32_HeapAlloc(System.IntPtr,System.UInt32,System.UInt32)
			call System_IntPtr___Indy_IL2CPU_PInvokes_Kernel32_HeapAlloc___System_IntPtr__System_UInt32__System_UInt32___
			push eax

		.L0000000D:
			; IL: Stloc_0 
			pop eax
			mov [ebp - 04h],eax

		.L0000000E:
			; IL: Br_S Mono.Cecil.Cil.Instruction
			jmp .L00000010

		.L00000010:
			; IL: Ldloc_0 
			mov eax,[ebp - 04h]
			push eax

		.L00000011:
			; IL: Ret 
			pop eax
			add esp,4
			pop ebp
			ret 4
			;Method: System.IntPtr Indy.IL2CPU.PInvokes::Kernel32_HeapAlloc(System.IntPtr,System.UInt32,System.UInt32)
			;  (No locals)
			;  Args:
			;    [1] hHeap
			;    [2] dwFlags
			;    [3] dwBytes

	System_IntPtr___Indy_IL2CPU_PInvokes_Kernel32_HeapAlloc___System_IntPtr__System_UInt32__System_UInt32___:
			push ebp
			mov ebp,esp
			mov eax,[ebp + 08h]
			push eax
			mov eax,[ebp + 0Ch]
			push eax
			mov eax,[ebp + 010h]
			push eax
			call [HeapAlloc]
			pushd eax
			pop eax
			pop ebp
			ret 12
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
			;Method: System.Void System.Array::.ctor()
			;  Locals:
			;  Args:

	System_Void___System_Array__ctor____:
			push ebp
			mov ebp,esp

		.L00000000:
			; IL: Ldarg_0 
			mov eax,[ebp + 08h]
			push eax

		.L00000001:
			; IL: Call System.Void System.Object::.ctor()
			call System_Void___System_Object__ctor____

		.L00000006:
			; IL: Ret 
			pop ebp
			ret 4
			;Method: System.Void Indy.IL2CPU.ObjectImpl::Ctor(System.IntPtr)
			;  Locals:
			;  Args:
			;    [1] aThis

	System_Void___System_Object__ctor____:
			push ebp
			mov ebp,esp

		.L00000000:
			; IL: Nop 
			nop

		.L00000001:
			; IL: Ret 
			pop ebp
			ret 4
			;Method: System.Void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(System.Array,System.RuntimeFieldHandle)
			;  (No locals)
			;  Args:
			;    [1] array
			;    [2] fldHandle

	System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___:
			push ebp
			mov ebp,esp
			mov eax,0
			mov edi,[ebp + 0Ch]
			mov esi,[ebp + 08h]
			mov ecx,[esi]
			add dword esi,4
			add dword edi,12

		.StartLoop:
			mov edx,[esi]
			mov [edi],edx
			add eax,4
			add dword esi,4
			add dword edi,4
			cmp eax,ecx
			je .EndLoop
			jmp .StartLoop

		.EndLoop:
			pop ebp
			ret 8

section '.idata' import data readable writeable

	dd 0,0,0,rva kernel32_dll_name,rva kernel32_dll_table
	dd 0,0,0,0,0

	kernel32_dll_table:
		ExitProcess dd rva _ExitProcess
		HeapAlloc dd rva _HeapAlloc
		HeapDestroy dd rva _HeapDestroy
		HeapCreate dd rva _HeapCreate
		dd 0

	kernel32_dll_name db 'kernel32.dll',0

	_ExitProcess dw 0
	db 'ExitProcess',0
	_HeapAlloc dw 0
	db 'HeapAlloc',0
	_HeapDestroy dw 0
	db 'HeapDestroy',0
	_HeapCreate dw 0
	db 'HeapCreate',0
