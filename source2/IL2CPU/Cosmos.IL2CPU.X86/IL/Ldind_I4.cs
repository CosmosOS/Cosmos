using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I4 )]
    public class Ldind_I4 : ILOp
    {
        public Ldind_I4( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Push
            {
                DestinationReg = CPUx86.Registers.EAX,
                DestinationIsIndirect = true
            };
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldind_I4)]
        // 	public class Ldind_I4: Op {
        // 		public Ldind_I4(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        // 		    new CPUx86.Push {
        // 		                            DestinationReg = CPUx86.Registers.EAX,
        // 		                            DestinationIsIndirect = true
        // 		                    };
        // 		}
        // 	}
        // }

    }
}
