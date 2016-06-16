using System;
using CPU = Cosmos.Assembler.x86;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Shr )]
    public class Shr : ILOp
    {
        public Shr( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			XS.Pop(XSRegisters.OldToNewRegister(CPU.RegistersEnum.ECX)); // shift amount
			var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
			var xStackItem_Value = aOpCode.StackPopTypes[1];
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);
#if DOTNETCOMPATIBLE
			if (xStackItem_Value.Size == 4)
#else
			if (xStackItem_Value_Size <= 4)
#endif
			{
				new CPUx86.ShiftRight { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 32, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.CL };
			}
#if DOTNETCOMPATIBLE
			else if (xStackItem_Value_Size == 8)
#else
			else if (xStackItem_Value_Size <= 8)
#endif
			{
				string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
				string HighPartIsZero = BaseLabel + "HighPartIsZero";
				string End_Shr = BaseLabel + "End_Shr";

				// [ESP] is low part
				// [ESP + 4] is high part

				// move high part in EAX
				XS.Set(XSRegisters.OldToNewRegister(CPU.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPU.RegistersEnum.ESP), sourceDisplacement: 4);

				new CPUx86.Compare { DestinationReg = CPUx86.RegistersEnum.CL, SourceValue = 32, Size = 8 };
				new CPUx86.ConditionalJump { Condition = CPUx86.ConditionalTestEnum.AboveOrEqual, DestinationLabel = HighPartIsZero };

				// shift lower part
				new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EAX, ArgumentReg = CPUx86.RegistersEnum.CL };
				// shift higher part
				new CPUx86.ShiftRight { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, Size = 32, SourceReg = CPUx86.RegistersEnum.CL };
				new CPUx86.Jump { DestinationLabel = End_Shr };

				XS.Label(HighPartIsZero);
				// remove bits >= 32, so that CL max value could be only 31
				new CPUx86.And { DestinationReg = CPUx86.RegistersEnum.CL, SourceValue = 0x1f, Size = 8 };

				// shift high part and move it in low part
				new CPUx86.ShiftRight{ DestinationReg = CPUx86.RegistersEnum.EAX, Size = 32, SourceReg = CPUx86.RegistersEnum.CL };
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EAX };
				// replace unknown high part with a zero, if <= 32
				new CPUx86.Mov { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, DestinationDisplacement = 4, SourceValue = 0};

				XS.Label(End_Shr);
			}
			else
				throw new NotSupportedException("A size bigger 8 not supported at Shr!");
            /*string xLabelName = AppAssembler.TmpPosLabel(aMethod, aOpCode);
            var xStackItem_ShiftAmount = Assembler.Stack.Pop();
            var xStackItem_Value = Assembler.Stack.Peek();
            if( xStackItem_Value.Size <= 4 )
            {
                XS.Pop(XSRegisters.ECX); // shift amount
                new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.CL };
            }
            else if( xStackItem_Value.Size <= 8 )
            {
				XS.Pop(XSRegisters.ECX); // shift amount
				// [ESP] is high part
				// [ESP + 4] is low part
				new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.ESP, SourceIsIndirect = true, SourceDisplacement = 4 };
				// shift low part
				new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EAX, ArgumentReg = CPUx86.Registers.CL };
				// shift high part
				new CPUx86.ShiftRight { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, Size = 32, SourceReg = CPUx86.Registers.CL };
            }*/
        }
    }
}
