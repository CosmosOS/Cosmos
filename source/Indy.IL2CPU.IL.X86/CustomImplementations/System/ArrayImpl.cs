using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target=typeof(Array))]
	public class ArrayImpl {
		//[PlugMethod(Signature="System_Void___System_Array_Clear___System_Array__System_Int32__System_Int32___")]
		//public static unsafe void Clear(uint* aArray, int aIndex, int aLength) {
		//}

		[PlugMethod(MethodAssembler=typeof(Assemblers.Array_InternalCopy)/*, Signature="System_Void___System_Array_Copy___System_Array__System_Int32__System_Array__System_Int32__System_Int32__System_Boolean___"*/)]
		public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable) {
			
		}
	}
}