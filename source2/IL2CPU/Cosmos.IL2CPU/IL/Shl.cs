using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shl )]
    public class Shl : ILOp
    {
        public Shl( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX }; // shift amount
            var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
			var xStackItem_Value = aOpCode.StackPopTypes[1];
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);
#if DOTNETCOMPATIBLE
			if (xStackItem_Value.Size == 4)
#else
			if (xStackItem_Value_Size <= 4)
#endif
			{
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.ESP, Size = 32, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.CL };
			}
#if DOTNETCOMPATIBLE
			else if (xStackItem_Value.Size == 8)
#else
			else if (xStackItem_Value_Size <= 8)
#endif
			{
				string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
				string LowPartIsZero = BaseLabel + "LowPartIsZero";
				string End_Shl = BaseLabel + "End_Shl";

				// [ESP] is low part
				// [ESP + 4] is high part

				// move low part to eax
				new CPUx86.Mov { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true };

				new CPUx86.Compare { DestinationReg = CPUx86.Registers.CL, SourceValue = 32, Size = 8 };
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.AboveOrEqual, DestinationLabel = LowPartIsZero };

				// shift higher part
				new CPUx86.ShiftLeftDouble { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EAX, ArgumentReg = CPUx86.Registers.CL };
				// shift lower part
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32, SourceReg = CPUx86.Registers.CL };
				new CPUx86.Jump { DestinationLabel = End_Shl };

				new Label(LowPartIsZero);
				// remove bits >= 32, so that CL max value could be only 31
				new CPUx86.And { DestinationReg = CPUx86.Registers.CL, SourceValue = 0x1f, Size = 8 };
				// shift low part in EAX and move it in high part
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.CL, Size = 32};
				new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.Registers.EAX };
				// replace unknown low part with a zero, if <= 32
				new CPUx86.Mov { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceValue = 0 };

				new Label(End_Shl);
			}
			else
				throw new NotSupportedException("A size bigger 8 not supported at Shl!");
        }
    }
}