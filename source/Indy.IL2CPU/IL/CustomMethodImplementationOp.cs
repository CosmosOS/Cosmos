using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	public enum CustomMethodEnum {
		System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___,
		System_Object___System_Array_GetValue___System_Int32___,
		System_Void___System_Array_SetValue___System_Object__System_Int32___,
		System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___,
		System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____,
		System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____,
		System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___,
		System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Free___System_UInt32___,
		System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___,
		System_Int32_System_Array_get_Length____
	}
	public abstract class CustomMethodImplementationOp: Op {
		public MethodInformation MethodInfo;
		public CustomMethodImplementationOp(ILReader aILReader , MethodInformation aMethodInfo)
			: base(aILReader, aMethodInfo) {
			MethodInfo = aMethodInfo;
		}
		public CustomMethodEnum Method {
			get;
			set;
		}

		protected abstract void Assemble_System_Object___System_Array_GetValue___System_Int32___();
		protected abstract void Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___();
		protected abstract void Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___();

		public sealed override void DoAssemble() {
			switch (Method) {
				case CustomMethodEnum.System_Object___System_Array_GetValue___System_Int32___:
					Assemble_System_Object___System_Array_GetValue___System_Int32___();
					break;
				case CustomMethodEnum.System_Void___System_Array_SetValue___System_Object__System_Int32___:
					Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___();
					break;
				case CustomMethodEnum.System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___:
					Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___();
					break;
				case CustomMethodEnum.System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____:
					Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____();
					break;
				case CustomMethodEnum.System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____:
					Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____();
					break;
				case CustomMethodEnum.System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___:
					Assemble_System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___();
					break;
				case CustomMethodEnum.System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___:
					Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___();
					break;
				case CustomMethodEnum.System_Int32_System_Array_get_Length____:
					Assemble_System_Int32_System_Array_get_Length____();
					break;
				case CustomMethodEnum.System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Free___System_UInt32___: {
					Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Free___System_UInt32___();
					break;
				}
				default:
					throw new Exception("Method not handled: '" + Method + "'!");
			}
		}

		protected abstract void Assemble_System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___();
		protected abstract void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____();
		protected abstract void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____();
		protected abstract void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___();
		protected abstract void Assemble_System_Int32_System_Array_get_Length____();
		protected abstract void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Free___System_UInt32___();
	}
}
