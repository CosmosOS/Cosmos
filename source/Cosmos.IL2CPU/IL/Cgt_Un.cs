using System;

using Cosmos.Assembler.x86;
using XSharp.Compiler;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;
using static XSharp.Compiler.XSRegisters;

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
            var xStackItem2 = aOpCode.StackPopTypes[1];
            var xIsFloat = TypeIsFloat(xStackItem) || TypeIsFloat(xStackItem2);
            var xSize = Math.Max(SizeOfType(xStackItem), SizeOfType(xStackItem2));

            if (xSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Cgt_Un.cs->Error: StackSizes > 8 not supported");
            }
            else if (xSize <= 4)
            {
                if (xIsFloat) //float
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.CompareSS(XMM1, XMM0, comparision: NotLessThanOrEqualTo);
                    XS.MoveD(EBX, XMM1);
                    XS.And(EBX, 1);
                    XS.Set(ESP, EBX, destinationIsIndirect: true);
                }
                else //uint
                {
                    XS.Pop(EAX);
                    XS.Pop(EBX);

                    // push 0
                    XS.Push(0);

                    // compare them
                    XS.Compare(EBX, EAX);

                    // if ebx > eax, set [esp] to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Above, ESP, destinationIsIndirect: true);

                    //XS.Pop(EAX);
                    //XS.Compare(EAX, ESP, sourceIsIndirect: true);

                    //XS.Jump(ConditionalTestEnum.Below, LabelTrue);
                    //XS.Jump(LabelFalse);
                    //XS.Label(LabelTrue );
                    //XS.Add(ESP, 4);
                    //XS.Push(1);
                    //XS.Jump(GetLabel(aMethod, aOpCode.NextPosition));
                    //XS.Label(LabelFalse );
                    //XS.Add(ESP, 4);
                    //XS.Push(0);
                }
            }
            else if (xSize <= 8)
            {
                if (xIsFloat) //double
                {
                    // Please note that SSE supports double operations only from version 2
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    // Increment ESP to get the value of the next double
                    XS.Add(ESP, 8);
                    XS.SSE2.MoveSD(XMM1, ESP, sourceIsIndirect: true);
                    // We need to move the stack pointer of 4 Byte to "eat" the second double that is yet in the stack or we get a corrupted stack!
                    XS.Add(ESP, 4);
                    XS.SSE2.CompareSD(XMM1, XMM0, comparision: NotLessThanOrEqualTo);
                    XS.MoveD(EBX, XMM1);
                    // greater than or unordered
                    XS.And(EBX, 1);
                    XS.Set(ESP, EBX, destinationIsIndirect: true);
                }
                else //ulong
                {
                    XS.Pop(EAX);
                    XS.Pop(EBX);

                    XS.Pop(ECX);
                    XS.Pop(EDX);

                    // push 0
                    XS.Push(0);

                    // compare low parts
                    XS.Compare(ECX, EAX);

                    // if above, set al to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Above, AL);

                    // compare high parts
                    XS.Compare(EDX, EBX);

                    // if above, set [esp] to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Above, ESP, destinationIsIndirect: true);
                    // if equal, set bl to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Equal, BL);

                    // if high parts are equal, low parts define the result
                    XS.And(BL, AL);

                    XS.Or(ESP, BL, destinationIsIndirect: true);

                    //XS.Set(ESI, 1);
                    //// esi = 1
                    //XS.Xor(EDI, EDI);
                    //// edi = 0
                    //XS.Pop(EAX);
                    //XS.Pop(EDX);
                    ////value2: EDX:EAX
                    //XS.Pop(EBX);
                    //XS.Pop(ECX);
                    ////value1: ECX:EBX

                    //XS.Compare(ECX, EDX);
                    //XS.Jump(ConditionalTestEnum.Above, LabelTrue);
                    //XS.Jump(ConditionalTestEnum.Below, LabelFalse);
                    //XS.Compare(EBX, EAX);
                    //XS.Label(LabelTrue);
                    //new ConditionalMove { Condition = ConditionalTestEnum.Above, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
                    //XS.Label(LabelFalse);
                    //XS.Push(EDI);
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
        //                 XS.Push(0);
        //                 XS.Jump(NextInstructionLabel);
        //
        // 				XS.Label(LabelTrue);
        // 				XS.Push(1);
        //
        // 			} else
        // 			{
        //                 XS.Pop(XSRegisters.EAX);
        //                 XS.Compare(XSRegisters.EAX, XSRegisters.ESP, sourceIsIndirect: true);
        //                 XS.Jump(ConditionalTestEnum.Below, LabelTrue);
        //                 XS.Jump(LabelFalse);
        //                 XS.Label(LabelTrue);
        //                 XS.Add(XSRegisters.ESP, 4);
        //                 XS.Push(1);
        //                 XS.Jump(NextInstructionLabel);
        //                 XS.Label(LabelFalse);
        //                 XS.Add(XSRegisters.ESP, 4);
        //                 XS.Push(0);
        //                 XS.Jump(NextInstructionLabel);
        // 			}
        // 		}
        // 	}
        // }

    }
}
