using System;
using System.Collections.Generic;
using System.Linq;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stelem)]
	public class Stelem: Op {
		private uint mElementSize;
		public Stelem(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xType = aReader.OperandValueType;
			if (xType == null)
				throw new Exception("Unable to determine Type!");
			mElementSize = Engine.GetFieldStorageSize(xType);
		}

		public override void DoAssemble() {
			Stelem_Ref.Assemble(Assembler, mElementSize);
		}
	}
}