using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_U1 )]
    public class Ldind_U1 : ILOp
    {
        public Ldind_U1( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            Assembler.Stack.Pop();
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceValue = 0 };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
            Assembler.Stack.Push(1, typeof( byte ) ) ;
        }


        // using System;
        // 
        // using CPUx86 = Cosmos.Compiler.Assembler.X86;
        // using Cosmos.IL2CPU.X86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[OpCode(OpCodeEnum.Ldind_U1)]
        // 	public class Ldind_U1: Op {
        // 		public Ldind_U1(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			Assembler.Stack.Pop();
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
        // 			new CPUx86.Move{DestinationReg = CPUx86.Registers.EAX, SourceValue=0};
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.AL, SourceReg = CPUx86.Registers.ECX, SourceIsIndirect = true };
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
        // 			Assembler.Stack.Push(new StackContent(1, typeof(byte)));
        // 		}
        // 	}
        // }

    }
}
