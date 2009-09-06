using System;
using CPU = Cosmos.IL2CPU.X86;
using Cosmos.IL2CPU.ILOpCodes;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldc_R4 )]
    public class Ldc_R4 : ILOp
    {
        public Ldc_R4( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            OpSingle xOp = ( OpSingle )aOpCode; 
            new CPU.Push { DestinationValue = BitConverter.ToUInt32( BitConverter.GetBytes( xOp.Value ), 0 ) };
            Assembler.Stack.Push( new StackContents.Item( 4, typeof( Single ) ) );
        }


        // using System;
        // using System.Linq;
        // 
        // using CPU = Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldc_R4)]
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
