using System;
using System.IO;


using CPU = Indy.IL2CPU.Assembler.X86;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Arglist)]
	public class Arglist: Op {
        private string mNextLabel;
	    private string mCurLabel;
	    private uint mCurOffset;
	    private MethodInformation mMethodInformation;

		public Arglist(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
             mMethodInformation = aMethodInfo;
		    mCurOffset = aReader.Position;
		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
            mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
		}
		public override void DoAssemble() {
            EmitNotImplementedException(Assembler, GetServiceProvider(), "ArgList opcode has not been implemented yet", mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
		}
	}
}