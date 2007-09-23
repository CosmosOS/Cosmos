using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Indy.IL2CPU.CustomImplementation.CompilerServices {
	public class RuntimeHelpers {
		//[MethodAlias(Name = "System_Void___System_Runtime_CompilerServices_RuntimeHelpers_InitializeArray___System_Array__System_RuntimeFieldHandle___")]
		//[DllImport("test.dll")]
		//public static extern void InitializeArray(Array aArray, RuntimeFieldHandle aFieldHandle);
		public static void InitializeArrayImpl(int[] aArray, int[] aFieldHandle) {
			for (int i = 0; i < aArray.Length; i++) {
				aArray[i] = aFieldHandle[i];
			}
		}
	}
}