using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
	[Cosmos.IL2CPU.OpCode(ILOpCode.Code.Rem)]
	public class Rem: ILOp
	{
		public Rem(Cosmos.Assembler.Assembler aAsmblr):base(aAsmblr)
		{
		}

    public override void Execute(MethodInfo aMethod, ILOpCode aOpCode) {

        var xStackItem = aOpCode.StackPopTypes[0];
        var xStackItemSize = SizeOfType(xStackItem);
        var xSize = Math.Max(xStackItemSize, SizeOfType(aOpCode.StackPopTypes[1]));
        if( xSize > 4 )
        {
            if (TypeIsFloat( xStackItem))
            {
                XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);
                XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                XS.SSE.XorPS(XSRegisters.XMM2, XSRegisters.XMM2);
                XS.SSE.DivPS(XSRegisters.XMM1, XSRegisters.XMM0);
                new MoveSS { SourceReg = CPUx86.RegistersEnum.XMM2, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
            }
            else
            {
				string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
				string LabelShiftRight = BaseLabel + "ShiftRightLoop";
				string LabelNoLoop = BaseLabel + "NoLoop";
				string LabelEnd = BaseLabel + "End";

				// divisor
				//low
				XS.Test(XSRegisters.ESI, XSRegisters.ESP, sourceIsIndirect: true);
				//high
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 4);

				// pop both 8 byte values
				XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 16);

				//dividend
				// low
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 8);
				//high
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 12);

				// set flags
				XS.Or(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI));
				// if high dword of divisor is already zero, we dont need the loop
				XS.Jump(CPUx86.ConditionalTestEnum.Zero, LabelNoLoop);

				// set ecx to zero for counting the shift operations
				XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

				XS.Label(LabelShiftRight);

				// shift divisor 1 bit right
				new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.ESI, SourceReg = CPUx86.RegistersEnum.EDI, ArgumentValue = 1 };
				XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), 1);

				// increment shift counter
				XS.Increment(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));

				// set flags
				XS.Or(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI));
				// loop while high dword of divisor till it is zero
				XS.Jump(CPUx86.ConditionalTestEnum.NotZero, LabelShiftRight);

				// shift the divident now in one step
				// shift divident CL bits right
				new CPUx86.ShiftRightDouble { DestinationReg = CPUx86.RegistersEnum.EAX, SourceReg = CPUx86.RegistersEnum.EDX, ArgumentReg = CPUx86.RegistersEnum.CL };
				XS.ShiftRight(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.CL);

				// so we shifted both, so we have near the same relation as original values
				// divide this
				XS.IntegerDivide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));

				// save result to stack
				new CPUx86.Push { DestinationValue = 0 };
				XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));

				//TODO: implement proper derivation correction and overflow detection

				new CPUx86.Jump { DestinationLabel = LabelEnd };

				XS.Label(LabelNoLoop);
				//save high dividend
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
				// extend that sign is in edx
				new CPUx86.SignExtendAX { Size = 32 };
				// divide high part
				XS.IntegerDivide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
				XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
				// divide low part
				XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
				// save low result
				new CPUx86.Push { DestinationValue = 0};
				XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));

				XS.Label(LabelEnd);
            }
        }
        else
        {
            if (TypeIsFloat(xStackItem))
            {
                XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                XS.SSE.XorPS(XSRegisters.XMM2, XSRegisters.XMM2);
                XS.SSE.DivSS(XSRegisters.XMM1, XSRegisters.XMM0);
                XS.Sub(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 4);
                new MoveSS { SourceReg = CPUx86.RegistersEnum.XMM2, DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true };
            }
            else
            {
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX)); // gets devised by ecx
                XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));

                XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX)); // => EAX / ECX
                XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
            }
        }
     }
	}
}
