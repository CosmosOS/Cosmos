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
					XS.Pop(OldToNewRegister(CPUx86.RegistersEnum.EBX)); // low
					XS.Pop(OldToNewRegister(CPUx86.RegistersEnum.EAX)); // high
					XS.Negate(OldToNewRegister(CPUx86.RegistersEnum.EBX)); // set carry if EBX != 0
					XS.AddWithCarry(OldToNewRegister(CPUx86.RegistersEnum.EAX), 0);
					XS.Negate(OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Push(OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Push(OldToNewRegister(CPUx86.RegistersEnum.EBX));
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
					XS.Pop(OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Negate(OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Push(OldToNewRegister(CPUx86.RegistersEnum.EAX));
				}
			}
		}
	}
}
