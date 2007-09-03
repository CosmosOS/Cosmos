using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL {
	public class MethodInformation {
		public struct Variable {
			public Variable(int aOffset, int aSize) {
				Offset = aOffset;
				Size = aSize;
			}
			public readonly int Offset;
			public readonly int Size;
		}

		public struct Argument {
			public Argument(int aSize, int aOffset) {
				Size = aSize;
				Offset = aOffset;
			}

			public readonly int Size;
			public readonly int Offset;
		}

		public MethodInformation(string aLabelName, Variable[] aLocals, Argument[] aArguments) {
			Locals = aLocals;
			LabelName = aLabelName;
			Arguments = aArguments;
		}

		public readonly string LabelName;
		public readonly Variable[] Locals;
		public readonly Argument[] Arguments;
	}
}