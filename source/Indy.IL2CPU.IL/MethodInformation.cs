using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	// TODO: abstract this one out to a X86 specific one
	public class MethodInformation {
		public struct Variable {
			public Variable(int aOffset, int aSize) {
				Offset = aOffset;
				Size = aSize;
				VirtualAddress = "ebp - 0" + (Offset + Size + 8).ToString("X") + "h";
			}
			public readonly int Offset;
			public readonly int Size;
			public readonly string VirtualAddress;
		}

		public struct Argument {
			public Argument(int aSize, int aOffset) {
				Size = aSize;
				Offset = aOffset;
				VirtualAddress = "ebp + 0" + (Offset + Size + 0).ToString("X") + "h";
			}

			public readonly string VirtualAddress;
			public readonly int Size;
			public readonly int Offset;
		}

		public MethodInformation(string aLabelName, Variable[] aLocals, Argument[] aArguments, bool aHasReturnValue) {
			Locals = aLocals;
			LabelName = aLabelName;
			Arguments = aArguments;
			HasReturnValue = aHasReturnValue;
		}

		public readonly string LabelName;
		public readonly Variable[] Locals;
		public readonly Argument[] Arguments;
		public readonly bool HasReturnValue;
	}
}