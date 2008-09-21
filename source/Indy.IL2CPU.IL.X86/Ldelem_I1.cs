using System;
using System.Collections.Generic;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelem_I1)]
	public class Ldelem_I1: Op {
        public static void ScanOp(ILReader aReader, MethodInformation aMethodInfo, SortedList<string, object> aMethodData)
        {
            Engine.RegisterType(typeof(sbyte));
        }

        public Ldelem_I1(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
			Ldelem_Ref.Assemble(Assembler, 1);
		}
	}
}