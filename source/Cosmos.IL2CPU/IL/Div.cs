using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using CPUx86 = Cosmos.Assembler.x86;
using Label = Cosmos.Assembler.Label;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Div )]
    public class Div : ILOp
    {
        public Div( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
			var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItemSize = SizeOfType(xStackItem);
            var xStackItem2 = aOpCode.StackPopTypes[0];
            var xStackItem2Size = SizeOfType(xStackItem2);
			if (xStackItemSize == 8)
            {
				// there seem to be an error in MS documentation, there is pushed an int32, but IL shows else
                if (xStackItem2Size != 8)
                {
                    throw new Exception("Cosmos.IL2CPU.x86->IL->Div.cs->Error: Expected a size of 8 for Div!");
                }
                if (TypeIsFloat(xStackItem))
                {// TODO add 0/0 infinity/infinity X/infinity
					// value 1
					new CPUx86.x87.FloatLoad { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// value 2
                    new CPUx86.x87.FloatDivide { DestinationReg = CPUx86.RegistersEnum.ESP, DestinationIsIndirect = true, Size = 64 };
					// override value 1
					new CPUx86.x87.FloatStoreAndPop { DestinationReg = CPUx86.RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true, DestinationDisplacement = 8 };
					// pop value 2
					XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);
                }
                else
                {
					string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
					string LabelShiftRight = BaseLabel + "ShiftRightLoop";
					string LabelNoLoop = BaseLabel + "NoLoop";
					string LabelEnd = BaseLabel + "End";

					// divisor
					//low
					XS.Set(XSRegisters.ESI, XSRegisters.ESP, sourceIsIndirect: true);
					//high
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDI), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 4);

					// pop both 8 byte values
					XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);

					//dividend
					// low
					XS.Set(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
					//high
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), sourceDisplacement: 4);

                    XS.Add(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESP), 8);

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

					// sign extend
					XS.SignExtendAX(XSRegisters.RegisterSize.Int32);

					// save result to stack
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

					//TODO: implement proper derivation correction and overflow detection

					new CPUx86.Jump { DestinationLabel = LabelEnd };

					XS.Label(LabelNoLoop);
					//save high dividend
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EDX));
					// extend that sign is in edx
					XS.SignExtendAX(XSRegisters.RegisterSize.Int32);
					// divide high part
					XS.IntegerDivide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
					// save high result
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					XS.Set(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
					// divide low part
					XS.Divide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ESI));
					// save low result
					XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));

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
                    XS.SSE.DivSS(XSRegisters.XMM0, XSRegisters.XMM1);
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                }
                else
                {
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                    XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
					          XS.SignExtendAX(XSRegisters.RegisterSize.Int32);
                    XS.IntegerDivide(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.ECX));
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
                }
            }
        }
    }
}
