using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Clt_Un )]
    public class Clt_Un : ILOp
    {
        public Clt_Un( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItemSize = SizeOfType(xStackItem);
            var xStackItemIsFloat = TypeIsFloat(xStackItem);
            if( xStackItemSize > 8 )
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Clt_Un.cs->Error: StackSizes > 8 not supported");
            }
            string BaseLabel = GetLabel( aMethod, aOpCode ) + ".";
            string LabelTrue = BaseLabel + "True";
            string LabelFalse = BaseLabel + "False";
            if( xStackItemSize > 4 )
            {
				XS.Set(XSRegisters.OldToNewRegister(RegistersEnum.ESI), 1);
				// esi = 1
				XS.Xor(XSRegisters.OldToNewRegister(RegistersEnum.EDI), XSRegisters.OldToNewRegister(RegistersEnum.EDI));
				// edi = 0
                if (xStackItemIsFloat)
                {
					// value 2
					new FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
					// value 1
					new FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationDisplacement = 8, DestinationIsIndirect = true };
					XS.FPU.FloatCompareAndSet(XSRegisters.ST1);
					// if carry is set, ST(0) < ST(i)
					new ConditionalMove { Condition = ConditionalTestEnum.Below, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
					// pops fpu stack
					XS.FPU.FloatStoreAndPop(XSRegisters.ST0);
					XS.FPU.FloatStoreAndPop(XSRegisters.ST0);
					XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 16);
				}
                else
                {
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EDX));
                    //value2: EDX:EAX
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EBX));
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                    //value1: ECX:EBX
                    XS.Sub(XSRegisters.OldToNewRegister(RegistersEnum.EBX), XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                    XS.SubWithCarry(XSRegisters.OldToNewRegister(RegistersEnum.ECX), XSRegisters.OldToNewRegister(RegistersEnum.EDX));
                    //result = value1 - value2
					new ConditionalMove { Condition = ConditionalTestEnum.Below, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
                }
                XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EDI));
            }
            else
            {
                if (xStackItemIsFloat)
                {
                	#warning THIS NEEDS TO BE TESTED!!!
					XS.Comment("TEST TODO");
                    XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                    new CompareSS { DestinationReg = RegistersEnum.XMM1, SourceReg = RegistersEnum.XMM0, pseudoOpcode = (byte)ComparePseudoOpcodes.LessThan };
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                    new CPUx86.And { DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true, SourceValue = 1 };
                }
                else
                {
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                    XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                    XS.Compare(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.Jump(ConditionalTestEnum.Below, LabelTrue);
                    XS.Jump(LabelFalse);
                    XS.Label(LabelTrue );
                    XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                    XS.Push(1);
                    new Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
                    XS.Label(LabelFalse );
                    XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                    new CPUx86.Push { DestinationValue = 0 };
            }
            }
        }

    }
}
