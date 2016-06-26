using System;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
using CPUx86 = Cosmos.Assembler.x86;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode( ILOpCode.Code.Neg )]
	public class Neg : ILOp
	{
		public Neg( Cosmos.Assembler.Assembler aAsmblr )
			: base( aAsmblr )
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
					XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Long64);
					XS.FPU.FloatNegate();
					XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Long64);
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
					XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Int32);
				  XS.FPU.FloatNegate();
					XS.FPU.FloatStoreAndPop(ESP, isIndirect: true, size: RegisterSize.Int32);
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
