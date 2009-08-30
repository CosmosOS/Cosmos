using System;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I1 )]
    public class Ldind_I1 : ILOp
    {
        public Ldind_I1( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Move
            {
                DestinationReg = CPUx86.Registers.EAX,
                SourceValue = 0
            };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            Assembler.Stack.Pop();
            Assembler.Stack.Push( 1, typeof( sbyte ) );
        }


        // using System;
        // 
        // using CPUx86 = Indy.IL2CPU.Assembler.X86;
        // using Indy.IL2CPU.Assembler;
        // 
        // namespace Indy.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldind_I1)]
        // 	public class Ldind_I1: Op {
        // 		public Ldind_I1(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
        // 		    new CPUx86.Move {
        // 		                            DestinationReg = CPUx86.Registers.EAX,
        // 		                            SourceValue = 0
        // 		                    };
        // 			new CPUx86.Move{DestinationReg=CPUx86.Registers.AL, SourceReg=CPUx86.Registers.ECX, SourceIsIndirect=true};
        // 			new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX};
        // 			Assembler.Stack.Pop();
        // 			Assembler.Stack.Push(new StackContent(1, typeof(sbyte)));
        // 		}
        // 	}
        // }

    }
}
