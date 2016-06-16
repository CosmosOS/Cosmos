using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;

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
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX)); // shift amount
            var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
			var xStackItem_Value = aOpCode.StackPopTypes[1];
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);
#if DOTNETCOMPATIBLE
			if (xStackItem_Value.Size == 4)
#else
			if (xStackItem_Value_Size <= 4)
#endif
			{
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 32, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.CL };
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
				XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);

				new CPUx86.Compare { DestinationReg = CPUx86.RegistersEnum.CL, SourceValue = 32, Size = 8 };
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.AboveOrEqual, DestinationLabel = LowPartIsZero };

				// shift higher part
				new CPUx86.ShiftLeftDouble { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.RegistersEnum.EAX, ArgumentReg = CPUx86.RegistersEnum.CL };
				// shift lower part
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, Size = 32, SourceReg = CPUx86.RegistersEnum.CL };
				new CPUx86.Jump { DestinationLabel = End_Shl };

				XS.Label(LowPartIsZero);
				// remove bits >= 32, so that CL max value could be only 31
				new CPUx86.And { DestinationReg = CPUx86.RegistersEnum.CL, SourceValue = 0x1f, Size = 8 };
				// shift low part in EAX and move it in high part
				new CPUx86.ShiftLeft { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.CL, Size = 32};
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceReg = CPUx86.RegistersEnum.EAX };
				// replace unknown low part with a zero, if <= 32
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceValue = 0 };

				XS.Label(End_Shl);
			}
			else
				throw new NotSupportedException("A size bigger 8 not supported at Shl!");
        }
    }
}
