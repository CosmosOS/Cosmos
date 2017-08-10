using System;

using XSharp.Common;
using CPUx86 = Cosmos.Assembler.x86;
namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldind_R8 )]
    public class Ldind_R8 : ILOp
    {
        public Ldind_R8( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(XSRegisters.EAX);
            new CPUx86.Push { DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4 };
            XS.Push(XSRegisters.EAX, isIndirect: true);
        }


        // using System;
        //
        // using CPUx86 = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldind_R8)]
        // 	public class Ldind_R8: Op {
        // 		public Ldind_R8(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 		}
        // 		public override void DoAssemble() {
        // 			XS.Pop(XSRegisters.EAX);
        // 			Assembler.Stack.Pop();
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = 4 };
        //             XS.Push(XSRegisters.EAX, isIndirect: true);
        // 			Assembler.Stack.Push(new StackContent(8, typeof(Double)));
        // 		}
        // 	}
        // }

    }
}
