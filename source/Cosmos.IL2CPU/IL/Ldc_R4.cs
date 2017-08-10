using System;
using CPU = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;


namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldc_R4 )]
    public class Ldc_R4 : ILOp
    {
        public Ldc_R4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpSingle xOp = ( OpSingle )aOpCode; 
            new CPU.Push { DestinationValue = BitConverter.ToUInt32( BitConverter.GetBytes( xOp.Value ), 0 ) };
        }


        // using System;
        // using System.Linq;
        // 
        // using CPU = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldc_R4)]
        // 	public class Ldc_R4: Op {
        // 		private Single mValue;
        // 		public Ldc_R4(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mValue = aReader.OperandValueSingle;
        // 		}
        // 		public override void DoAssemble() {
        // 			new CPU.Push{DestinationValue=BitConverter.ToUInt32(BitConverter.GetBytes(mValue), 0)};
        // 			Assembler.Stack.Push(new StackContent(4, typeof(Single)));
        // 		}
        // 	}
        // }

    }
}
