using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Cosmos.IL2CPU.Plugs;

namespace Cosmos.Core.Plugs.System.IO
{
    [Plug(TargetName = "System.IO.PathHelper")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class PathHelperImpl
    {
        public static unsafe void Ctor(ref object aThis, char* aCharArrayPtr, int aLength,
            [FieldAccess(Name = "System.Boolean System.IO.PathHelper.doNotTryExpandShortFileName")] ref bool mDoNotTryExpandShortFileName,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_capacity")] ref int mCapacity,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_maxPath")] ref int mMaxPath,
            [FieldAccess(Name = "System.Boolean System.IO.PathHelper.useStackAlloc")] ref bool mUseStackAlloc
            )
        {
            mLength = 0;
            mCapacity = aLength;
            mArrayPtr = aCharArrayPtr;
            mUseStackAlloc = true;
        }

        public static unsafe void Ctor(ref object aThis, int aCapacity, int aMaxPath,
            [FieldAccess(Name = "System.Boolean System.IO.PathHelper.doNotTryExpandShortFileName")] ref bool mDoNotTryExpandShortFileName,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_capacity")] ref int mCapacity,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_maxPath")] ref int mMaxPath,
            [FieldAccess(Name = "System.Boolean System.IO.PathHelper.useStackAlloc")] ref bool mUseStackAlloc)
        {
            mLength = 0;
            mCapacity = aCapacity;
            mUseStackAlloc = true;

        }

        public static int get_Capacity(ref object aThis,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_capacity")] ref int mCapacity)
        {
            return mCapacity;
        }

        public static unsafe char get_Item(ref object aThis, int aIndex,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr)
        {
            return mArrayPtr[aIndex];
        }

        public static unsafe void set_Item(ref object aThis, int aIndex, char aValue,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr)
        {
            mArrayPtr[aIndex] = aValue;
        }

        public static int get_Length(ref object aThis,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength)
        {
            return mLength;
        }

        public static void set_Length(ref object aThis, int aValue,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength)
        {
            mLength = aValue;
        }

        public static unsafe void Append(ref object aThis, char aValue,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_capacity")] ref int mCapacity,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr)
        {
            if (mLength + 1 > mCapacity)
            {
                throw new PathTooLongException();
            }

            mArrayPtr[mLength] = aValue;
            mLength++;
        }

        public static unsafe int GetFullPathName(ref object aThis,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr)
        {
            int xLength = 0;
            while (*mArrayPtr != '\0')
            {
                xLength++;
                mArrayPtr++;
            }
            mLength = xLength;
            return xLength;
        }

        public static unsafe string ToString(ref object aThis,
                        [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr)
        {
            return new string(mArrayPtr, 0, mLength);
        }

        public static unsafe bool TryExpandShortFileName(ref object aThis,
            [FieldAccess(Name = "System.Int32 System.IO.PathHelper.m_length")] ref int mLength,
            [FieldAccess(Name = "System.Char* System.IO.PathHelper.m_arrayPtr")] ref char* mArrayPtr)
        {
            int xLength = 0;
            while (*mArrayPtr != '\0')
            {
                xLength++;
                mArrayPtr++;
            }
            mLength = xLength;
            return true;
        }
    }
}
