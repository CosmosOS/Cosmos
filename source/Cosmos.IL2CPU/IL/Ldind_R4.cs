using System;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_R4 )]
    public class Ldind_R4 : ILOp
    {
        public Ldind_R4( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, Size = 32 };
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldind_R4)]
        // 	public class Ldind_R4: Op {
        // 		public Ldind_R4(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        //             Assembler.Stack.Pop();
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect=true, Size=32 };
        //             Assembler.Stack.Push(new StackContent(4, typeof(Single)));
        // 		}
        // 	}
        // }

    }
}
