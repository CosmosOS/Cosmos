using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.IL.X86;

using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace Indy.IL2CPU.IL.X86.Win32 {
	public class Win32CustomMethodImplementationOp: X86CustomMethodImplementationOp {
		public Win32CustomMethodImplementationOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___() {
			PassCall(RuntimeEngineImplRefs.Heap_AllocNewObjectRef);
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____() {
			if (Assembler.InMetalMode)
				throw new NotImplementedException();
			PassCall(RuntimeEngineImplRefs.Heap_ShutdownRef);
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____() {
			PassCall(RuntimeEngineImplRefs.Heap_InitializeRef);
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Free___System_UInt32___() {
			PassCall(RuntimeEngineImplRefs.Heap_FreeRef);
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___() {
			PassCall(RuntimeEngineImplRefs.ExitProcessRef);
		}
	}
}