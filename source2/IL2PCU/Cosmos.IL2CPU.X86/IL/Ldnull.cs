using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldnull )]
    public class Ldnull : ILOp
    {
        public Ldnull( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Push { DestinationValue = 0 };
            Assembler.Stack.Push(4, typeof( uint ));
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPU = Indy.IL2CPU.Assembler.X86;
        // using Indy.IL2CPU.Assembler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldnull)]
        // 	public class Ldnull: Op {
        // 		public Ldnull(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        //             new CPU.Push { DestinationValue = 0 };
        // 			Assembler.Stack.Push(new StackContent(4, typeof(uint)));
        // 		}
        // 	}
        // }

    }
}
