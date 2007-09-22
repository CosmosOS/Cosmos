using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.CustomImplementation.System {
	public static class ArrayImpl {
		[MethodAlias(Name = "System.Array.Copy(System.Array,System.Int32,System.Array,System.Int32,System.Int32,System.Boolean)")]
		public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable) {
			for(int i = sourceIndex; i < length; i++) {
				destinationArray.SetValue(sourceArray.GetValue(i), (i - sourceIndex) + destinationIndex);
			}
		}
	}
}