using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_Ref )]
    public class Ldind_Ref : ILOp
    {
        public Ldind_Ref( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
        }


        // using System;
        // using System.IO;
        //
        //
        // using CPU = Cosmos.Assembler.x86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldind_Ref)]
        // 	public class Ldind_Ref: Op {
        // 		public Ldind_Ref(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        //             new CPU.Pop { DestinationReg = CPU.Registers.EAX };
        //             new CPU.Push { DestinationReg = CPU.Registers.EAX, DestinationIsIndirect = true };
        // 		}
        // 	}
        // }

    }
}
