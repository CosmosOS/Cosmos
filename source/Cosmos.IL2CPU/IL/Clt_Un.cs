using System;

using Cosmos.Assembler.x86;
using XSharp.Compiler;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;
using static XSharp.Compiler.XSRegisters;

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
            var xStackItem2 = aOpCode.StackPopTypes[1];
            var xIsFloat = TypeIsFloat(xStackItem) || TypeIsFloat(xStackItem2);
            var xSize = Math.Max(SizeOfType(xStackItem), SizeOfType(xStackItem2));

            if (xSize > 8)
            {
                throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Clt_Un.cs->Error: StackSizes > 8 not supported");
            }
            else if (xSize <= 4)
            {
                if (xIsFloat) //float
                {
                    XS.Comment("TEST TODO");
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.CompareSS(XMM1, XMM0, comparision: LessThan);
                    XS.MoveD(EBX, XMM1);
                    XS.And(ESP, 1, destinationIsIndirect: true);
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

                    // if ebx < eax, set [esp] to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Below, ESP, destinationIsIndirect: true);

                    //XS.Pop(ECX);
                    //XS.Pop(EAX);
                    //XS.Push(ECX);
                    //XS.Compare(EAX, ESP, sourceIsIndirect: true);
                    //XS.Jump(ConditionalTestEnum.Below, LabelTrue);
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
                    XS.SSE2.MoveSD(XMM2, XMM1);
                    // LessThan means it can't be unordered, so allow it to be unordered
                    XS.SSE2.CompareSD(XMM2, XMM0, comparision: Unordered);
                    XS.MoveD(EAX, XMM2);
                    XS.SSE2.CompareSD(XMM1, XMM0, comparision: LessThan);
                    XS.MoveD(EBX, XMM1);
                    // lower than or unordered
                    XS.Or(EBX, EAX);
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

                    // if below, set al to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Below, AL);

                    // compare high parts
                    XS.Compare(EDX, EBX);

                    // if below, set [esp] to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Below, ESP, destinationIsIndirect: true);
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
                    //XS.Sub(EBX, EAX);
                    //XS.SubWithCarry(ECX, EDX);
                    ////result = value1 - value2
                    //new ConditionalMove { Condition = ConditionalTestEnum.Below, DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.ESI };
                    //XS.Push(EDI);
                }
            }
        }
    }
}
