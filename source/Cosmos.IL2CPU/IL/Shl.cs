using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

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
            XS.Pop(XSRegisters.ECX); // shift amount
            var xStackItem_ShiftAmount = aOpCode.StackPopTypes[0];
			var xStackItem_Value = aOpCode.StackPopTypes[1];
            var xStackItem_Value_Size = SizeOfType(xStackItem_Value);
#if DOTNETCOMPATIBLE
			if (xStackItem_Value.Size == 4)
#else
			if (xStackItem_Value_Size <= 4)
#endif
			{
				XS.ShiftLeft(XSRegisters.ESP, XSRegisters.CL, destinationIsIndirect: true, size: RegisterSize.Int32);
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
				XS.Set(EAX, ESP, sourceIsIndirect: true);

				XS.Compare(XSRegisters.CL, 32, size: RegisterSize.Byte8);
				XS.Jump(CPUx86.ConditionalTestEnum.AboveOrEqual, LowPartIsZero);

				// shift higher part
				XS.ShiftLeftDouble(ESP, EAX, CL, destinationDisplacement: 4);
                // shift lower part
                // To retain the sign bit we must use ShiftLeftArithmetic and not ShiftLeft!
                XS.ShiftLeftArithmetic(XSRegisters.ESP, XSRegisters.CL, destinationIsIndirect: true, size: RegisterSize.Int32);
				XS.Jump(End_Shl);

				XS.Label(LowPartIsZero);
				// remove bits >= 32, so that CL max value could be only 31
				XS.And(XSRegisters.CL, 0x1f, size: RegisterSize.Byte8);
                // shift low part in EAX and move it in high part
                // To retain the sign bit we must use ShiftLeftArithmetic and not ShiftLeft!
                XS.ShiftLeftArithmetic(EAX, CL);
				XS.Set(ESP, EAX, destinationDisplacement: 4);
				// replace unknown low part with a zero, if <= 32
				XS.Set(XSRegisters.ESP, 0, destinationIsIndirect: true);

				XS.Label(End_Shl);
			}
			else
				throw new NotSupportedException("A size bigger 8 not supported at Shl!");
        }
    }
}
