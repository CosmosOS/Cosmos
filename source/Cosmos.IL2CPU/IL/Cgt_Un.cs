using System;
using CPUx86 = Cosmos.Assembler.x86;
using CPU = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using Cosmos.Assembler.x86.SSE;
using Cosmos.Assembler.x86.x87;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Cgt_Un )]
    public class Cgt_Un : ILOp
    {
        public Cgt_Un( Cosmos.Assembler.Assembler aAsmblr )
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
                //EmitNotImplementedException( Assembler, GetServiceProvider(), "Cgt_Un: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel );
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Cgt_Un.cs->Error: StackSizes > 8 not supported");
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
					// value 1
					new FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationDisplacement = 8, DestinationIsIndirect = true };
					// value 2
					new FloatLoad { DestinationReg = RegistersEnum.ESP, Size = 64, DestinationIsIndirect = true };
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

					XS.Compare(XSRegisters.OldToNewRegister(RegistersEnum.ECX), XSRegisters.OldToNewRegister(RegistersEnum.EDX));
					XS.Jump(ConditionalTestEnum.Above, LabelTrue);
					XS.Jump(ConditionalTestEnum.Below, LabelFalse);
					XS.Compare(XSRegisters.OldToNewRegister(RegistersEnum.EBX), XSRegisters.OldToNewRegister(RegistersEnum.EAX));
					XS.Label(LabelTrue);
					new ConditionalMove { Condition = ConditionalTestEnum.Above, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
					XS.Label(LabelFalse);
                }
				XS.Push(XSRegisters.OldToNewRegister(RegistersEnum.EDI));
				/*
                XS.Jump(ConditionalTestEnum.Above, LabelTrue);
				XS.Label(LabelFalse);
                new CPUx86.Push { DestinationValue = 0 };
                new CPUx86.Jump { DestinationLabel = GetLabel(aMethod, aOpCode.NextPosition) };
                XS.Label(LabelTrue );
                new CPUx86.Push { DestinationValue = 1 };
				*/
            }
            else
            {
                if (xStackItemIsFloat)
                {

                    XS.SSE.MoveSS(XSRegisters.XMM0, XSRegisters.ESP, sourceIsIndirect: true);
                    XS.Add(XSRegisters.OldToNewRegister(RegistersEnum.ESP), 4);
                    XS.SSE.MoveSS(XSRegisters.XMM1, XSRegisters.ESP, sourceIsIndirect: true);
                    new CompareSS { DestinationReg = RegistersEnum.XMM1, SourceReg = RegistersEnum.XMM0, pseudoOpcode = (byte)ComparePseudoOpcodes.NotLessThanOrEqualTo };
                    XS.SSE2.MoveD(XSRegisters.XMM1, XSRegisters.EBX);
                    XS.And(XSRegisters.OldToNewRegister(RegistersEnum.EBX), 1);
                    new Mov { SourceReg = RegistersEnum.EBX, DestinationReg = RegistersEnum.ESP, DestinationIsIndirect = true };
                }
                else
                {
                    XS.Pop(XSRegisters.OldToNewRegister(RegistersEnum.EAX));
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


        // using System;
        //
        // using CPUx86 = Cosmos.Assembler.x86;
        // using CPU = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.X86;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Cgt_Un)]
        // 	public class Cgt_Un: Op {
        // 		private readonly string NextInstructionLabel;
        //         private readonly string CurInstructionLabel;
        //         private uint mCurrentOffset;
        //         private MethodInformation mMethodInfo;
        //         public Cgt_Un(ILReader aReader, MethodInformation aMethodInfo)
        //             : base(aReader, aMethodInfo)
        //         {
        //             NextInstructionLabel = GetInstructionLabel(aReader.NextPosition);
        //             CurInstructionLabel = GetInstructionLabel(aReader);
        //             mMethodInfo = aMethodInfo;
        //             mCurrentOffset = aReader.Position;
        //         }
        //
        // 	    public override void DoAssemble()
        // 		{
        // 			var xStackItem = Assembler.Stack.Pop();
        //             if (xStackItem.IsFloat)
        //             {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Cgt_Un: Floats not yet supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
        //                 return;
        //             }
        //             if (xStackItem.Size > 8)
        //             {
        //                 EmitNotImplementedException(Assembler, GetServiceProvider(), "Cgt_Un: StackSizes>8 not supported", CurInstructionLabel, mMethodInfo, mCurrentOffset, NextInstructionLabel);
        //                 return;
        //             }
        // 			Assembler.Stack.Push(new StackContent(4, typeof(bool)));
        // 			string BaseLabel = CurInstructionLabel + ".";
        // 			string LabelTrue = BaseLabel + "True";
        // 			string LabelFalse = BaseLabel + "False";
        // 			if (xStackItem.Size > 4)
        // 			{
        //                 XS.Xor(XSRegisters.ESI, XSRegisters.CPUx86.Registers.ESI);
        //                 XS.Add(XSRegisters.ESI, 1);
        //                 XS.Xor(XSRegisters.EDI, XSRegisters.CPUx86.Registers.EDI);
        // 				//esi = 1
        //                 XS.Pop(XSRegisters.EAX);
        //                 XS.Pop(XSRegisters.EDX);
        //                 //value2: EDX:EAX
        //                 XS.Pop(XSRegisters.EBX);
        //                 XS.Pop(XSRegisters.ECX);
        //                 //value1: ECX:EBX
        //                 XS.Sub(XSRegisters.EBX, XSRegisters.CPUx86.Registers.EAX);
        //                 XS.SubWithCarry(XSRegisters.ECX, XSRegisters.CPUx86.Registers.EDX);
        // 				//result = value1 - value2
        // 				//new CPUx86.ConditionalMove(Condition.Above, "edi", "esi");
        //                 //XS.Push(XSRegisters.EDI);
        //
        //                 XS.Jump(ConditionalTestEnum.Above, LabelTrue);
        //                 new CPUx86.Push { DestinationValue = 0 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        //
        // 				XS.Label(LabelTrue);
        // 				new CPUx86.Push{DestinationValue=1};
        //
        // 			} else
        // 			{
        //                 XS.Pop(XSRegisters.EAX);
        //                 XS.Compare(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
        //                 XS.Jump(ConditionalTestEnum.Below, LabelTrue);
        //                 new CPUx86.Jump { DestinationLabel = LabelFalse };
        //                 XS.Label(LabelTrue);
        //                 XS.Add(XSRegisters.ESP, 4);
        //                 new CPUx86.Push { DestinationValue = 1 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        //                 XS.Label(LabelFalse);
        //                 XS.Add(XSRegisters.ESP, 4);
        //                 new CPUx86.Push { DestinationValue = 0 };
        //                 new CPUx86.Jump { DestinationLabel = NextInstructionLabel };
        // 			}
        // 		}
        // 	}
        // }

    }
}
