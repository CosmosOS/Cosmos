using System;
using XSharp.Compiler;
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
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            XS.Set(XSRegisters.EAX, XSRegisters.EAX, sourceIsIndirect: true);
            XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), 8);
            new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true };
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
        //             XS.Pop(XSRegisters.EAX);
        // 			XS.Add(XSRegisters.EAX, 8);
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true };
        // 			Assembler.Stack.Push(new StackContent(4, typeof(uint)));
        // 		}
        // 	}
        // }

    }
}
