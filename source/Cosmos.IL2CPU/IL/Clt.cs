using System;

using Cosmos.Assembler.x86;
using XSharp.Compiler;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Compares two values. If the first value is less than the second, the integer value 1 (int32) is pushed onto the evaluation stack;
    /// otherwise 0 (int32) is pushed onto the evaluation stack.
    /// </summary>
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Clt )]
    public class Clt : ILOp
    {
        public Clt(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
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
              throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Cgt.cs->Error: StackSizes > 8 not supported");
            }
            else if (xSize <= 4)
            {
                if (xIsFloat) //float
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.CompareSS(XMM1, XMM0, comparision: LessThan);
                    XS.MoveD(EBX, XMM1);
                    XS.And(EBX, 1);
                    XS.Set(ESP, EBX, destinationIsIndirect: true);
                }
                else //int
                {
                    XS.Pop(EAX);
                    XS.Pop(EBX);

                    // push 0
                    XS.Push(0);

                    // compare them
                    XS.Compare(EBX, EAX);

                    // if ebx < eax, set [esp] to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.LessThan, ESP, destinationIsIndirect: true);

                    //XS.Pop(ECX);
                    //XS.Pop(EAX);
                    //XS.Push(ECX);
                    //XS.Compare(EAX, ESP, sourceIsIndirect: true);
                    //XS.Jump(ConditionalTestEnum.LessThan, LabelTrue);
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
                    XS.SSE2.CompareSD(XMM1, XMM0, comparision: LessThan);
                    XS.MoveD(EBX, XMM1);
                    XS.And(EBX, 1);
                    XS.Set(ESP, EBX, destinationIsIndirect: true);
                }
                else //long
                {
                    XS.Pop(EAX);
                    XS.Pop(EBX);

                    XS.Pop(ECX);
                    XS.Pop(EDX);

                    // push 0
                    XS.Push(0);

                    // compare low parts
                    XS.Compare(ECX, EAX);

                    // if less than, set al to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.LessThan, AL);

                    // compare high parts
                    XS.Compare(EDX, EBX);

                    // if less than, set [esp] to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.LessThan, ESP, destinationIsIndirect: true);
                    // if equal, set bl to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Equal, BL);

                    // if high parts are equal, low parts define the result
                    XS.And(BL, AL);

                    XS.Or(ESP, BL, destinationIsIndirect: true);

                    //XS.Pop(ECX);
                    //XS.Pop(EAX);
                    //XS.Push(ECX);
                    //XS.Compare(EAX, ESP, sourceIsIndirect: true);
                    //XS.Jump(ConditionalTestEnum.LessThan, LabelTrue);
                    //XS.Jump(LabelFalse);
                    //XS.Label(LabelTrue);
                    //XS.Add(ESP, 4);
                    //XS.Push(1);

                    //XS.Jump(GetLabel(aMethod, aOpCode.NextPosition));

                    //XS.Label(LabelFalse);
                    //XS.Add(ESP, 4);
                    //XS.Push(0);
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
