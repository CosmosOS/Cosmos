using System;


namespace Cosmos.IL2CPU.X86.IL
{
    //[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Brtrue)]
    public class Brtrue : ILOp
    {
        public Brtrue( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            throw new NotImplementedException();
        }


        // using System;
        // using System.IO;
        // 
        // 
        // using CPUx86 = Cosmos.Assembler.x86;
        // 
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Brtrue)]
        // 	public class Brtrue: Op {
        // 		public readonly string TargetLabel;
        // 		public Brtrue(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			TargetLabel = GetInstructionLabel(aReader.OperandValueBranchPosition);
        // 		}
        // 
        // 		public override void DoAssemble() {
        // 			int xSize = Assembler.Stack.Pop().Size;
        // 			if (xSize > 8) {
        // 				throw new Exception("StackSize>8 not supported");
        // 			}
        // 			if (xSize > 4)
        // 			{
        //                 XS.Pop(XSRegisters.EAX);
        //                 XS.Pop(XSRegisters.EBX);
        //                 XS.Xor(XSRegisters.EAX, XSRegisters.CPUx86.Registers.EAX);
        //                 XS.Jump(ConditionalTestEnum.NotZero, TargetLabel);
        //                 XS.Xor(XSRegisters.EBX, XSRegisters.CPUx86.Registers.EBX);
        //                 XS.Jump(ConditionalTestEnum.NotZero, TargetLabel);
        // 			} else
        // 			{
        //                 XS.Pop(XSRegisters.EAX);
        //                 XS.Compare(XSRegisters.EAX, 0);
        //                 XS.Jump(ConditionalTestEnum.NotEqual, TargetLabel);
        // 			}
        // 		}
        // 	}
        // }

    }
}
