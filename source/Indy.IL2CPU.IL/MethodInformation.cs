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

		public MethodInformation(string aLabelName, Variable[] aLocals) {
			Locals = aLocals;
			LabelName = aLabelName;
		}

		public readonly string LabelName;
		public readonly Variable[] Locals;
	}
}