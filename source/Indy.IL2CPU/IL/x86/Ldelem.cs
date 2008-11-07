using System;
using System.Collections.Generic;
using System.Linq;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelem)]
	public class Ldelem: Op {
		private uint mElementSize;
        
        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
        {
            Type xType = aReader.OperandValueType;
            if (xType == null)
                throw new Exception("Unable to determine Type!");
            Engine.RegisterType(xType);
        }

        public Ldelem(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
			Type xType = aReader.OperandValueType;
			if (xType == null)
				throw new Exception("Unable to determine Type!");
			mElementSize = Engine.GetFieldStorageSize(xType);
		}

		public override void DoAssemble() {
			Ldelem_Ref.Assemble(Assembler, mElementSize);
		}
	}
}