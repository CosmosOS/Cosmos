using System;
using CPUx86 = Cosmos.Compiler.Assembler.X86;
using Cosmos.Compiler.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shl )]
    public class Shl : ILOp
    {
        public Shl( Cosmos.Compiler.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			string BaseLabel = GetLabel(aMethod, aOpCode) + "__";
			string ResultIsZero = BaseLabel + "ResultIsZero";
			string End_Shl = BaseLabel + "End_Shl";

            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX }; // shift amount
            var xStackItem_ShiftAmount = Assembler.Stack.Pop();
			var xStackItem_Value = Assembler.Stack.Peek();
#if DOTNETCOMPATIBLE
			if (xStackItem_Value.Size == 4)
#else
			if (xStackItem_Value.Size <= 4)
#endif
				new CPUx86.Compare { DestinationReg = CPUx86.Registers.ECX, SourceValue = 32 };
			else if (xStackItem_Value.Size == 8)
				new CPUx86.Compare { DestinationReg = CPUx86.Registers.ECX, SourceValue = 64 };
			else
				throw new NotSupportedException(string.Format("A value of size {0:D} is not useable, yet!", xStackItem_Value.Size));
			new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.AboveOrEqual, DestinationLabel = ResultIsZero };

			if (xStackItem_Value.Size == 4)
			{
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.ESP, Size = 32, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.CL };
				new CPUx86.Jump { DestinationLabel = End_Shl };
			}
			else if (xStackItem_Value.Size == 8)
			{
				new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX }; // value low part
				// shift higher part
				new CPUx86.ShiftLeftDouble { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX, ArgumentReg = CPUx86.Registers.CL };
				// shift lower part
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.EAX, Size = 32, SourceReg = CPUx86.Registers.CL };
				new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
				new CPUx86.Jump { DestinationLabel = End_Shl };
			}
            
			new Label(ResultIsZero);
			//replace unknown number with a zero, if more or equal 32(64 to shift
			if (xStackItem_ShiftAmount.Size == 8)
			{
				new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 64, SourceValue = 0 };
			}
			else
			{
				new CPUx86.Move { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32, SourceValue = 0 };
			}
			new Label(End_Shl);
        }
    }
}