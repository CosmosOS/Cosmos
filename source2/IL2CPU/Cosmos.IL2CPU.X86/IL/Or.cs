using System;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Or )]
    public class Or : ILOp
    {
        public Or( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			var xStackContent = Assembler.Stack.Pop();
			var xStackContentSecond = Assembler.Stack.Pop();

			var xSize = Math.Max(xStackContent.Size, xStackContentSecond.Size);
			if (ILOp.Align(xStackContent.Size, 4u) != ILOp.Align(xStackContentSecond.Size, 4u))
				throw new NotSupportedException("Operands have different size!");
			if (xSize > 8)
				throw new NotImplementedException("StackSize>8 not supported");

			if (xSize > 4)
			{
				// [ESP] is low part
				// [ESP + 4] is high part
				// [ESP + 8] is low part
				// [ESP + 12] is high part
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
				// [ESP] is low part
				// [ESP + 4] is high part
				new CPUx86.Or { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
				new CPUx86.Or { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EDX };
			}
			else
			{
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
				new CPUx86.Or { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
			}
			Assembler.Stack.Push(xStackContent);
        }
    }
}