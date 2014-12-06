using System;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldlen )]
    public class Ldlen : ILOp
    {
        public Ldlen( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 8 };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldlen)]
        // 	public class Ldlen: Op {
        // 		public Ldlen(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			Assembler.Stack.Pop();
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        // 			new CPUx86.Add{DestinationReg=CPUx86.Registers.EAX, SourceValue=8};
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
        // 			Assembler.Stack.Push(new StackContent(4, typeof(uint)));
        // 		}
        // 	}
        // }

    }
}
