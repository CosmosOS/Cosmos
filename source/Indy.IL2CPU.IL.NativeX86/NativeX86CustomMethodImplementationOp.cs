using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;

namespace Indy.IL2CPU.IL.NativeX86 {
	public class NativeX86CustomMethodImplementationOp: X86.X86CustomMethodImplementationOp {
		public NativeX86CustomMethodImplementationOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		protected override void Assemble_System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___() {
			throw new NotImplementedException();
		}

		protected override void Assemble_System_Object___System_Array_GetValue___System_Int32___() {
			throw new NotImplementedException();
		}

		protected override void Assemble_System_Void___System_Array_SetValue___System_Object__System_Int32___() {
			throw new NotImplementedException();
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_CustomImplementation_System_StringImpl_GetStorage___System_UInt32___() {
			throw new NotImplementedException();
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___() {
			throw new NotImplementedException();
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____() {
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____() {
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___() {
		}

		protected override void Assemble_System_Int32_System_Array_get_Length____() {
			throw new NotImplementedException();
		}
	}
}
