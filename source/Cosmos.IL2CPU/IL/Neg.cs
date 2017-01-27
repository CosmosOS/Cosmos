using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [OpCode(ILOpCode.Code.Neg)]
    public class Neg : ILOp
    {
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
					          XS.Pop(EBX); // low
					          XS.Pop(EAX); // high
					          XS.Negate(EBX); // set carry if EBX != 0
					          XS.AddWithCarry(EAX, 0);
					          XS.Negate(EAX);
					          XS.Push(EAX);
					          XS.Push(EBX);
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
					          XS.Negate(ESP, isIndirect: true, size: RegisterSize.Int32);
				        }
			      }
		    }
	  }
}
