using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86;
using Mono.Cecil;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public class Win32CustomMethodImplementationOp: X86CustomMethodImplementationOp {
		public Win32CustomMethodImplementationOp(Instruction aInstruction, MethodInformation aMethodInfo)
			: base(aInstruction, aMethodInfo) {
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___() {
			PassCall(RuntimeEngineImplRefs.Heap_AllocNewObjectRef);
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____() {
			PassCall(RuntimeEngineImplRefs.Heap_ShutdownRef);
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____() {
			PassCall(RuntimeEngineImplRefs.Heap_InitializeRef);
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___() {
			PassCall(RuntimeEngineImplRefs.ExitProcessRef);
		}
	}
}