using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL {
	public enum CustomMethodEnum {
		System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___,
		System_Object___System_Array_GetValue___System_Int32___,
		System_Void___System_Array_SetValue___System_Object__System_Int32___,
		System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___
	}
	public abstract class CustomMethodImplementationOp: Op {
		public MethodInformation MethodInfo;
		public CustomMethodImplementationOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
			MethodInfo = aMethodInfo;
		}
		public CustomMethodEnum Method {
			get;
			set;
		}

		protected abstract void Assemble_System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___();
		protected abstract void Assemble_System_Object___System_Array_GetValue___System_Int32___();
		protected abstract void Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___();
		protected abstract void Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___();

		public sealed override void DoAssemble() {
			switch (Method) {
				case CustomMethodEnum.System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___:
					Assemble_System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___();
					break;
				case CustomMethodEnum.System_Object___System_Array_GetValue___System_Int32___:
					Assemble_System_Object___System_Array_GetValue___System_Int32___();
					break;
				case CustomMethodEnum.System_Void___System_Array_SetValue___System_Object__System_Int32___:
					Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___();
					break;
				case CustomMethodEnum.System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___:
					Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___();
					break;
				default:
					throw new Exception("Method not handled: '" + Method + "'!");
			}
		}
	}
}
