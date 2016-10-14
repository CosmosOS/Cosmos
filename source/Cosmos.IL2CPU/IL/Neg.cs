using System;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
  [OpCode(ILOpCode.Code.Neg)]
  public class Neg : ILOp
  {
    static bool varDone = false;

    public Neg(Assembler.Assembler aAsmblr)
  : base(aAsmblr)
    {
    }

		public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
		{
			var xStackContent = aOpCode.StackPopTypes[0];
		    var xStackContentSize = SizeOfType(xStackContent);
		    var xStackContentIsFloat = TypeIsFloat(xStackContent);
			if (xStackContentSize > 4)
			{
				if (xStackContentIsFloat)
				{
                    // There is no direct double negate instruction in SSE simply we do a XOR with 0x8000000000 to flip the sign bit
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    XS.SSE2.XorPD(XMM0, "__doublesignbit", sourceIsIndirect: true);
                    XS.SSE2.MoveSD(ESP, XMM0, destinationIsIndirect: true);
                }
				else
				{
					XS.Pop(XSRegisters.EBX); // low
					XS.Pop(XSRegisters.EAX); // high
					XS.Negate(XSRegisters.EBX); // set carry if EBX != 0
					XS.AddWithCarry(XSRegisters.EAX, 0);
					XS.Negate(XSRegisters.EAX);
					XS.Push(XSRegisters.EAX);
					XS.Push(XSRegisters.EBX);
				}
			}
			else
			{
				if (xStackContentIsFloat)
				{
                    // There is no direct float negate instruction in SSE simply we do a XOR with 0x80000000 to flip the sign bit
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.SSE.MoveSS(XMM1, "__floatsignbit", sourceIsIndirect: true);
                    XS.SSE.XorPS(XMM0, XMM1);
                    XS.SSE.MoveSS(ESP, XMM0, destinationIsIndirect: true);
                }
				else
				{
					XS.Pop(XSRegisters.EAX);
					XS.Negate(XSRegisters.EAX);
					XS.Push(XSRegisters.EAX);
				}
			}
		}
	}
}
