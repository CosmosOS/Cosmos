using System;
using CPUx86 = Cosmos.IL2CPU.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_U4 )]
    public class Ldind_U4 : ILOp
    {
        public Ldind_U4( Cosmos.IL2CPU.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldind_U4)]
        // 	public class Ldind_U4: Op {
        // 		public Ldind_U4(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
        // 			new CPUx86.Push{DestinationReg=CPUx86.Registers.EAX, DestinationIsIndirect=true};
        // 		}
        // 	}
        // }

    }
}
