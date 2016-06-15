using System;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Compares two values. If the first value is less than the second, the integer value 1 (int32) is pushed onto the evaluation stack;
    /// otherwise 0 (int32) is pushed onto the evaluation stack.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Clt )]
    public class Clt : ILOp
    {
        public Clt( Cosmos.Assembler.Assembler aAsmblr )
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
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Clt: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel );
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Clt.cs->Error: StackSizes > 8 not supported");
                //return;
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
					new ConditionalMove { Condition = ConditionalTestEnum.LessThan, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
                }
                XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EDI));
            }
            else
            {
                if (xStackItemIsFloat)
                {
                    new MoveSS { DestinationReg = RegistersEnum.XMM0, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true };
                    XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                    new MoveSS { DestinationReg = RegistersEnum.XMM1, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true };
                    new CompareSS { DestinationReg = RegistersEnum.XMM1, SourceReg = RegistersEnum.XMM0, pseudoOpcode = (byte)ComparePseudoOpcodes.LessThan };
                    XS.SSE2.MoveD(XSRegisters.XMM1, XSRegisters.EBX);
                    XS.And(XSRegisters.OldToNewRegister(RegistersEnum.EBX), 1);
                    new Mov { SourceReg = RegistersEnum.EBX, DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true };
                }
                else
                {
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
                    XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.ECX));
                    new Compare { DestinationReg = RegistersEnum.EAX, SourceReg = RegistersEnum.ESP, SourceIsIndirect = true };
                    new ConditionalJump { Condition = ConditionalTestEnum.LessThan, DestinationLabel = LabelTrue };
                    XS.Jump(LabelFalse);
                    XS.Label(LabelTrue );
                    XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                    new Push { DestinationValue = 1 };

                    new Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };

                    XS.Label(LabelFalse );
                    XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                    new CPUx86.Push { DestinationValue = 0 };
                }
            }
        }


        // using System;
        // using System.IO;
        //
        //
        // using CPUx86 = Cosmos.Assembler.x86;
        // using CPU = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Clt)]
        // 	public class Clt: Op {
        // 		private readonly string NextInstructionLabel;
        // 		private readonly string CurInstructionLabel;
        //         private uint mCurrentOffset;
        //         private MethodInformation mMethodInfo;
        //         public Clt(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
        // 			CurInstructionLabel = GetInstructionLabel(aReader);
        //             mMethodInfo = aMethodInfo;
        //             mCurrentOffset = aReader.Position;
        // 		}
        // 		public override void DoAssemble() {

        // 		}
        // 	}
        // }

    }
}
