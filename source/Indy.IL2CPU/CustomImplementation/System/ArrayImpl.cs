using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.CustomImplementation.System {
    [Plug(Target=typeof(Array))]
	public static class ArrayImpl {
        public static object Clone(Array aThis) { return aThis; }
		//[MethodAlias(Name = "System.Void System.Array.Copy(System.Array,System.Int32,System.Array,System.Int32,System.Int32,System.Boolean)")]
		//public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable) {
		//    for(int i = sourceIndex; i < length; i++) {
		//        destinationArray.SetValue(sourceArray.GetValue(i), (i - sourceIndex) + destinationIndex);
		//    }
		//}

//		public static void InitArrayWithReferenceTypes(, uint aSize) {
//
//			for(int i = 0; i < aArray.Length; i++) {
//				uint xNewObj = RuntimeEngine.Heap_AllocNewObject(aSize);
//				aArray.SetValue(xNewObj, i);
//			}
//		}
	}
}