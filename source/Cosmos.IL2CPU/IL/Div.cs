using System;
using Cosmos.Assembler.x86.SSE;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;
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
                {
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 8);
                    XS.SSE2.MoveSD(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE2.DivSD(XMM1, XMM0);
                    XS.SSE2.MoveSD(ESP, XMM1, destinationIsIndirect: true);
                }
                else
                {
					string BaseLabel = GetLabel(aMethod, aOpCode) + ".";
					string LabelShiftRight = BaseLabel + "ShiftRightLoop";
					string LabelNoLoop = BaseLabel + "NoLoop";
					string LabelEnd = BaseLabel + "End";

					// divisor
					//low
					XS.Set(ESI, ESP, sourceIsIndirect: true);
					//high
					XS.Set(EDI, ESP, sourceDisplacement: 4);

					// pop both 8 byte values
					XS.Add(ESP, 8);

					//dividend
					// low
					XS.Set(EAX, ESP, sourceIsIndirect: true);
					//high
					XS.Set(EDX, ESP, sourceDisplacement: 4);

                    XS.Add(ESP, 8);

					// set flags
					XS.Or(EDI, EDI);
					// if high dword of divisor is already zero, we dont need the loop
					XS.Jump(CPUx86.ConditionalTestEnum.Zero, LabelNoLoop);

					// set ecx to zero for counting the shift operations
					XS.Xor(ECX, ECX);

					XS.Label(LabelShiftRight);

					// shift divisor 1 bit right
                    XS.ShiftRightDouble(ESI, EDI, 1);
					XS.ShiftRight(EDI, 1);

					// increment shift counter
					XS.Increment(ECX);

					// set flags
					XS.Or(EDI, EDI);
					// loop while high dword of divisor till it is zero
					XS.Jump(CPUx86.ConditionalTestEnum.NotZero, LabelShiftRight);

					// shift the divident now in one step
					// shift divident CL bits right
					XS.ShiftRightDouble(EAX, EDX, CL);
					XS.ShiftRight(EDX, CL);

					// so we shifted both, so we have near the same relation as original values
					// divide this
					XS.IntegerDivide(ESI);

					// sign extend
					XS.SignExtendAX(RegisterSize.Int32);

					// save result to stack
					XS.Push(EDX);
					XS.Push(EAX);

					//TODO: implement proper derivation correction and overflow detection

					XS.Jump(LabelEnd);

					XS.Label(LabelNoLoop);
					//save high dividend
					XS.Set(ECX, EAX);
					XS.Set(EAX, EDX);
					// extend that sign is in edx
					XS.SignExtendAX(RegisterSize.Int32);
					// divide high part
					XS.IntegerDivide(ESI);
					// save high result
					XS.Push(EAX);
					XS.Set(EAX, ECX);
					// divide low part
					XS.Divide(ESI);
					// save low result
					XS.Push(EAX);

					XS.Label(LabelEnd);
                }
            }
            else
            {
				if (TypeIsFloat(xStackItem))
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.DivSS(XMM1, XMM0);
                    XS.SSE.MoveSS(ESP, XMM1, destinationIsIndirect: true);
                }
                else
                {
                    XS.Pop(XSRegisters.ECX);
                    XS.Pop(XSRegisters.EAX);
					XS.SignExtendAX(RegisterSize.Int32);
                    XS.IntegerDivide(XSRegisters.ECX);
                    XS.Push(XSRegisters.EAX);
                }
            }
        }
    }
}
