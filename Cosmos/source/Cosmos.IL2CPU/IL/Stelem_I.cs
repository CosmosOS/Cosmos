using System;

using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Stelem_I )]
    public class Stelem_I : ILOp
    {
        public Stelem_I( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            Stelem_Ref.Assemble(Assembler, 4, aMethod, aOpCode, DebugEnabled);
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Cosmos.Assembler.x86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Stelem_I)]
        // 	public class Stelem_I: Op {
        // 		private string mNextLabel;
        // 	    private string mCurLabel;
        // 	    private uint mCurOffset;
        // 	    private MethodInformation mMethodInformation;
        //         
        //         public Stelem_I(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        //              mMethodInformation = aMethodInfo;
        // 		    mCurOffset = aReader.Position;
        // 		    mCurLabel = IL.Op.GetInstructionLabel(aReader);
        //             mNextLabel = IL.Op.GetInstructionLabel(aReader.NextPosition);
        // 		}
        // 		public override void DoAssemble() {
        //             Stelem_Ref.Assemble(Assembler, 4, GetServiceProvider(), mCurLabel, mMethodInformation, mCurOffset, mNextLabel);
        // 		}
        // 	}
        // }

    }
}
