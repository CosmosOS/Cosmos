using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Indy.IL2CPU.IL.X86.Native {
	public class NativeCustomMethodImplementationOp: X86.X86CustomMethodImplementationOp {
		public NativeCustomMethodImplementationOp(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		protected override void Assemble_System_UInt32___Indy_IL2CPU_RuntimeEngine_Heap_AllocNewObject___System_UInt32___() {
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Shutdown____() {
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Initialize____() {
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_ExitProcess___System_Int32___() {
		}

		protected override void Assemble_System_Int32_System_Array_get_Length____() {
		}

		protected override void Assemble_System_Void___Indy_IL2CPU_RuntimeEngine_Heap_Free___System_UInt32___() {
		}
	}
}
