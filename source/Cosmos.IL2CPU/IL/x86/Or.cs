using System;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.IL.x86
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
            var xStackContent = aOpCode.StackPopTypes[0];
            var xStackContentSecond = aOpCode.StackPopTypes[1];
            var xStackContentSize = SizeOfType(xStackContent);
            var xStackContentSecondSize = SizeOfType(xStackContentSecond);
			var xSize = Math.Max(xStackContentSize, xStackContentSecondSize);
            if (ILOp.Align(xStackContentSize, 4u) != ILOp.Align(xStackContentSecondSize, 4u))
            {
                throw new NotSupportedException("Operands have different size!");
            }
            if (xSize > 8)
            {
                throw new NotImplementedException("StackSize>8 not supported");
            }

            if (xSize > 4)
			{
				// [ESP] is low part
				// [ESP + 4] is high part
				// [ESP + 8] is low part
				// [ESP + 12] is high part
				XS.Pop(XSRegisters.EAX);
				XS.Pop(XSRegisters.EDX);
				// [ESP] is low part
				// [ESP + 4] is high part
				XS.Or(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
				XS.Or(XSRegisters.ESP, XSRegisters.EDX, destinationDisplacement: 4);
			}
			else
			{
				XS.Pop(XSRegisters.EAX);
				XS.Or(XSRegisters.ESP, XSRegisters.EAX, destinationIsIndirect: true);
			}
        }
    }
}
