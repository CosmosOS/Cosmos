using System;
using System.Collections.Generic;
using System.Linq;

namespace Indy.IL2CPU.Assembler {
	public sealed class StackContent {
		public StackContent(int aSize) {
			Size = aSize;
		}

		public StackContent(int aSize, Type aType)
			: this(aSize) {
			IsNumber = (aType == typeof(byte)
				|| aType == typeof(sbyte)
				|| aType == typeof(Boolean)
				|| aType == typeof(short)
				|| aType == typeof(ushort)
				|| aType == typeof(int)
				|| aType == typeof(uint)
				|| aType == typeof(long)
				|| aType == typeof(ulong)
				|| aType == typeof(Single)
				|| aType == typeof(Double));
			IsFloat = (aType == typeof(Single) || aType == typeof(Double));
			IsSigned = (aType == typeof(sbyte)
				|| aType == typeof(short)
				|| aType == typeof(int)
				|| aType == typeof(long)
				|| aType == typeof(Single)
				|| aType == typeof(Double));
			ContentType = aType;
		}

		public StackContent(int aSize, bool aIsNumber, bool aIsFloat, bool aIsSigned)
			: this(aSize) {
			IsNumber = aIsNumber;
			IsFloat = aIsFloat;
			IsSigned = aIsSigned;
		}
		public readonly int Size;
		public readonly bool IsNumber = false;
		public readonly bool IsFloat = false;
		public readonly bool IsSigned = false;
		public readonly Type ContentType = null;
		public readonly bool IsBox = false;
	}
}