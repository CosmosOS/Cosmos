using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Indy.IL2CPU.Plugs;

namespace Indy.IL2CPU.IL.X86.CustomImplementations.System {
	[Plug(Target = typeof(Array))]
	public class ArrayImpl {
		[PlugMethod(Signature = "System_Void__System_Array_Clear_System_Array__System_Int32__System_Int32_")]
		public static unsafe void Clear(uint* aArray, uint aIndex, uint aLength) {
			aArray += 3;
			uint xElementSize = *aArray;
			aArray += 1;
			byte* xBytes = (byte*)aArray;
			for (uint i = aIndex * xElementSize; i < ((aIndex + aLength) * xElementSize); i++) {
				xBytes[i] = 0;
			}
		}

        public static int GetUpperBound(Array aThis, int aDimension) {
            return GetLength(aThis, aDimension) - 1;
        }

        public static int GetLength(Array aThis, int aDimension)
        {
            if (aDimension != 0) { throw new NotSupportedException("Multidimensional array's are not yet supported!"); }
            return aThis.Length;
        }


		[PlugMethod(Signature = "System_Boolean__System_Array_TrySZBinarySearch_System_Array__System_Int32__System_Int32__System_Object__System_Int32__")]
		public static unsafe bool TrySZBinarySearch(uint* aArray, uint sourceIndex, uint count, uint value, out uint retVal) {
			return TrySZIndexOf(aArray, sourceIndex, count, value, out retVal);
		}

		[PlugMethod(Signature = "System_Boolean__System_Array_TrySZLastIndexOf_System_Array__System_Int32__System_Int32__System_Object__System_Int32__")]
		public static unsafe bool TrySZLastIndexOf(uint* aArray, uint sourceIndex, uint count, uint value, out uint retVal) {
			aArray += 4;
			for (uint i = (sourceIndex + count); i > sourceIndex; i--) {
				if (aArray[i - 1] == value) {
					retVal = i - 1;
					return true;
				}
			}
			retVal = 0;
			return false;
		}

		[PlugMethod(Signature = "System_Boolean__System_Array_TrySZIndexOf_System_Array__System_Int32__System_Int32__System_Object__System_Int32__")]
		public static unsafe bool TrySZIndexOf(uint* aArray, uint sourceIndex, uint count, uint value, out uint retVal) {
			aArray += 4;
			for (uint i = sourceIndex; i < (sourceIndex + count); i++) {
				if (aArray[i] == value) {
					retVal = i;
					return true;
				}
			}
			retVal = 0;
			return false;
		}

		[PlugMethod(Assembler = typeof(Assemblers.Array_InternalCopy))]
		public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length, bool reliable) {
		}

		public static unsafe int get_Length(int* aThis) {
			aThis += 2;
			return *aThis;
			//
		}

		public static int get_Rank(int aThis) {
			return 1;
		}

		public static unsafe int GetLowerBound(int* aThis, int aDimension) {
			if (aDimension != 0) {
				throw new NotSupportedException("Multidimensional arrays not supported yet!");
			}
            return 0;
		}

        [PlugMethod(Signature = "System_Object__System_Array_GetValue_System_Int32_")]
		public static unsafe uint GetValue(uint* aThis, int aIndex) {
			aThis += 3;
			uint xElementSize = *aThis;
			aThis += 1;
			aThis = ((uint*)(((byte*)aThis) + aIndex * xElementSize));
			switch (xElementSize) {
				case 1:
					return *((byte*)aThis);
				case 2:
					return *((ushort*)aThis);
				case 3:
					return (*((uint*)aThis)) & 0x0FFFFFFF;
				case 4:
					return *((uint*)aThis);
			}
			throw new NotSupportedException("GetValue not supported in this situation!");
		}

        public static unsafe object GetValue(Array aThis, params int[] aIndices) { throw new NotImplementedException("Multidimensional arrays not supported yet!"); }

		[PlugMethod(Signature = "System_Void__System_Array_SetValue_System_Object__System_Int32_")]
		public static unsafe void SetValue(uint* aThis, uint aValue, int aIndex) {
			aThis += 3;
			uint xElementSize = *aThis;
			aThis += 1;
			aThis = ((uint*)(((byte*)aThis) + aIndex * xElementSize));
			switch (xElementSize) {
				case 1:
					*((byte*)aThis) = (byte)aValue;
					return;
				case 2:
					*((ushort*)aThis) = (ushort)aValue;
					return;
				case 3:
					*((uint*)aThis) = (uint)aValue;
					return;
				case 4:
					*((uint*)aThis) = (uint)aValue;
					return;
			}
			throw new NotSupportedException("SetValue not supported in this situation!");
		}
	}
}