using System;

using XSharp.Common;
using static XSharp.Common.XSRegisters;
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

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            XS.Pop(XSRegisters.EAX);
            XS.Push(EAX, isIndirect: true, size: RegisterSize.Int32);
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
        //             XS.Pop(XSRegisters.EAX);
        //             XS.Push(XSRegisters.EAX, destinationIsIndirect: true, size: RegisterSize.Int32);
        //             Assembler.Stack.Push(new StackContent(4, typeof(Single)));
        // 		}
        // 	}
        // }

    }
}
