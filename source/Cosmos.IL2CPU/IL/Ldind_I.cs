using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_I )]
    public class Ldind_I : ILOp
    {
        public Ldind_I( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Push(XSRegisters.EAX, isIndirect: true);
        }


        // using System;
        //
        // using CPUx86 = Cosmos.Assembler.x86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldind_I)]
        // 	public class Ldind_I: Op {
        // 		public Ldind_I(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        //             XS.Pop(XSRegisters.EAX);
        //             XS.Push(XSRegisters.EAX, isIndirect: true);
        // 		}
        // 	}
        // }

    }
}
