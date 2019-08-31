using System;

using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.Core_Plugs.System
{
    [Plug(Target = typeof(Array))]
    public static class ArrayImpl
    {
        [PlugMethod(Signature = "System_Void__System_Array_Clear_System_Array__System_Int32__System_Int32_")]
        public static unsafe void Clear([ObjectPointerAccess] uint* aArray, uint aIndex, uint aLength)
        {
            aArray = (uint*) aArray[0];
            aArray += 3;
            uint xElementSize = *aArray;
            aArray += 1;
            byte* xBytes = (byte*) aArray;
            for (uint i = aIndex * xElementSize; i < ((aIndex + aLength) * xElementSize); i++)
            {
                xBytes[i] = 0;
            }
        }

        public static int GetUpperBound(Array aThis, int aDimension)
        {
            return GetLength(aThis, aDimension) - 1;
        }

        public static int GetLength(Array aThis, int aDimension)
        {
            if (aDimension != 0)
            {
                throw new NotSupportedException("Multidimensional array's are not yet supported!");
            }
            return aThis.Length;
        }

        [PlugMethod(Signature = "System_Boolean__System_Array_TrySZBinarySearch_System_Array__System_Int32__System_Int32__System_Object___System_Int32_")]
        public static unsafe bool TrySZBinarySearch([ObjectPointerAccess] uint* aArray, uint sourceIndex, uint count, uint value, out uint retVal)
        {
            aArray = (uint*) aArray[0];
            return TrySZIndexOf(aArray, sourceIndex, count, value, out retVal);
        }

        [PlugMethod(Signature = "System_Boolean__System_Array_TrySZLastIndexOf_System_Array__System_Int32__System_Int32__System_Object___System_Int32_")]
        public static unsafe bool TrySZLastIndexOf([ObjectPointerAccess] uint* aArray, uint sourceIndex, uint count, uint value, out uint retVal)
        {
            aArray = (uint*) aArray[0];
            aArray += 4;
            for (uint i = (sourceIndex + count); i > sourceIndex; i--)
            {
                if (aArray[i - 1] == value)
                {
                    retVal = i - 1;
                    return true;
                }
            }
            retVal = 0;
            return false;
        }

        //[PlugMethod(Signature = "System_Boolean__System_Array_TrySZIndexOf_System_Array__System_Int32__System_Int32__System_Object__System_Int32__")]
        private static unsafe bool TrySZIndexOf([ObjectPointerAccess] uint* aArray, uint sourceIndex, uint count, uint value, out uint retVal)
        {
            aArray = (uint*) aArray[0];
            aArray += 4;
            for (uint i = sourceIndex; i < (sourceIndex + count); i++)
            {
                if (aArray[i] == value)
                {
                    retVal = i;
                    return true;
                }
            }
            retVal = 0;
            return false;
        }

        public static unsafe int get_Rank([ObjectPointerAccess]int* aThis)
        {
            return 1;
        }

        public static unsafe int GetLowerBound([ObjectPointerAccess]int* aThis, int aDimension)
        {
            aThis = (int*) aThis[0];
            if (aDimension != 0)
            {
                //throw new NotSupportedException("Multidimensional arrays not supported yet!");
            }
            return 0;
        }

        [PlugMethod(Signature = "System_Object__System_Array_GetValue_System_Int32_")]
        public static unsafe uint GetValue([ObjectPointerAccess]uint* aThis, int aIndex)
        {
            aThis = (uint*) aThis[0];
            aThis += 3;
            uint xElementSize = *aThis;
            aThis += 1;
            aThis = ((uint*) (((byte*) aThis) + aIndex * xElementSize));
            switch (xElementSize)
            {
                case 1:
                    return *((byte*) aThis);
                case 2:
                    return *((ushort*) aThis);
                case 3:
                    return (*aThis) & 0x0FFFFFFF;
                case 4:
                    return *aThis;
            }
            throw new NotSupportedException("GetValue not supported in this situation!");
        }

        public static unsafe object GetValue(Array aThis, params int[] aIndices)
        {
            throw new NotImplementedException("Multidimensional arrays not supported yet!");
        }

        [PlugMethod(Signature = "System_Void__System_Array_SetValue_System_Object__System_Int32_")]
        public static unsafe void SetValue([ObjectPointerAccess] uint* aThis, uint aValue, int aIndex)
        {
            aThis = (uint*) aThis[0];
            aThis += 3;
            uint xElementSize = *aThis;
            aThis += 1;
            aThis = ((uint*) (((byte*) aThis) + aIndex * xElementSize));
            switch (xElementSize)
            {
                case 1:
                    *((byte*) aThis) = (byte) aValue;
                    return;
                case 2:
                    *((ushort*) aThis) = (ushort) aValue;
                    return;
                case 3:
                    *((uint*) aThis) = (uint) aValue;
                    return;
                case 4:
                    *((uint*) aThis) = (uint) aValue;
                    return;
            }
            throw new NotSupportedException("SetValue not supported in this situation!");
        }
    }
}
