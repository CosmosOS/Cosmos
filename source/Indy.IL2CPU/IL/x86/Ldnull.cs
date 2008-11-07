using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldnull)]
	public class Ldnull: Op {
		public Ldnull(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}
		public override void DoAssemble() {
            new CPU.Push { DestinationValue = 0 };
			Assembler.StackContents.Push(new StackContent(4, typeof(uint)));
		}
	}
}