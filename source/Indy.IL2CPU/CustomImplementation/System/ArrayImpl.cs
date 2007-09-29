using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.CustomImplementation.System {
	public static class ArrayImpl {
		[MethodAlias(Name = "System.Void System.Array.Copy(System.Array,System.Int32,System.Array,System.Int32,System.Int32,System.Boolean)")]
		public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable) {
			for(int i = sourceIndex; i < length; i++) {
				destinationArray.SetValue(sourceArray.GetValue(i), (i - sourceIndex) + destinationIndex);
			}
		}

		[MethodAlias(Name = "System.Int32 System.Array.get_Length()")]
		public static unsafe int Length_Get(int aThis) {
			int* xArray = &aThis;
			xArray += 8;
			return *xArray;
		}

//		public static void InitArrayWithReferenceTypes(, uint aSize) {
//
//			for(int i = 0; i < aArray.Length; i++) {
//				uint xNewObj = RuntimeEngine.Heap_AllocNewObject(aSize);
//				aArray.SetValue(xNewObj, i);
//			}
//		}
	}
}