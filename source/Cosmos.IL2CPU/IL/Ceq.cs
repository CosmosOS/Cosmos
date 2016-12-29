using System;

using Cosmos.Assembler;
using Cosmos.Assembler.x86;
using XSharp.Compiler;
using static Cosmos.Assembler.x86.SSE.ComparePseudoOpcodes;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ceq)]
    public class Ceq : ILOp
    {
        public Ceq(Cosmos.Assembler.Assembler aAsmblr)
          : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xStackItem = aOpCode.StackPopTypes[0];
            var xStackItem2 = aOpCode.StackPopTypes[1];
            var xIsFloat = TypeIsFloat(xStackItem) || TypeIsFloat(xStackItem2);
            var xSize = Math.Max(SizeOfType(xStackItem), SizeOfType(xStackItem2));

            var xNextLabel = GetLabel(aMethod, aOpCode.NextPosition);

            if (xSize > 8)
            {
                throw new Exception("Cosmos.IL2CPU.x86->IL->Ceq.cs->Error: StackSizes > 8 not supported");
            }
            else if (xSize <= 4)
            {
                if (xIsFloat) //float
                {
                    XS.SSE.MoveSS(XMM0, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 4);
                    XS.SSE.MoveSS(XMM1, ESP, sourceIsIndirect: true);
                    XS.SSE.CompareSS(XMM1, XMM0, comparision: Equal);
                    XS.MoveD(EBX, XMM1);
                    XS.And(EBX, 1);
                    XS.Set(ESP, EBX, destinationIsIndirect: true);
                }
                else //int and uint
                {
                    XS.Pop(EAX);
                    XS.Pop(EBX);

                    // push 0 to the stack
                    XS.Push(0);

                    // compare eax and ebx
                    XS.Compare(EBX, EAX);

                    // if they are equal, set [esp] to 1
                    XS.SetByteOnCondition(ConditionalTestEnum.Equal, ESP, destinationIsIndirect: true);

                    //XS.Pop(EAX);
                    //XS.Compare(EAX, ESP, sourceIsIndirect: true);
                    //XS.Jump(ConditionalTestEnum.Equal, Label.LastFullLabel + ".True");
                    //XS.Jump(Label.LastFullLabel + ".False");
                    //XS.Label(".True");
                    //XS.Add(ESP, 4);
                    //XS.Push(1);
                    //XS.Jump(xNextLabel);
                    //XS.Label(".False");
                    //XS.Add(ESP, 4);
                    //XS.Push(0);
                    //XS.Jump(xNextLabel);
                }
            }
            else if(xSize <= 8)
            {
                if (xIsFloat) //double
                {
                    // Please note that SSE supports double operations only from version 2
                    XS.SSE2.MoveSD(XMM0, ESP, sourceIsIndirect: true);
                    // Increment ESP to get the value of the next double
                    XS.Add(ESP, 8);
                    XS.SSE2.MoveSD(XMM1, ESP, sourceIsIndirect: true);
                    XS.Add(ESP, 8);
                    XS.SSE2.CompareSD(XMM1, XMM0, comparision: Equal);
                    XS.MoveD(EBX, XMM1);
                    XS.And(EBX, 1);
                    XS.Push(EBX);
                }
                else
                {
                    if (TypeIsReferenceType(xStackItem) && TypeIsReferenceType(xStackItem2))
                    {
                        XS.Comment(xStackItem.Name);
                        XS.Add(ESP, 4);
                        XS.Pop(EAX);

                        XS.Comment(xStackItem2.Name);
                        XS.Add(ESP, 4);
                        XS.Pop(EBX);

                        XS.Push(0);

                        XS.Compare(EBX, EAX);

                        XS.SetByteOnCondition(ConditionalTestEnum.Equal, ESP, destinationIsIndirect: true);

                        //XS.Compare(EAX, EBX);
                        //XS.Jump(ConditionalTestEnum.NotEqual, Label.LastFullLabel + ".False");

                        //// equal
                        //XS.Push(1);
                        //XS.Jump(xNextLabel);
                        //XS.Label(Label.LastFullLabel + ".False");
                        ////not equal
                        //XS.Push(0);
                        //XS.Jump(xNextLabel);
                    }
                    else //long and ulong
                    {
                        XS.Pop(EAX);
                        XS.Pop(EBX);

                        XS.Pop(ECX);
                        XS.Pop(EDX);

                        // push 0
                        XS.Push(0);

                        // compare low parts
                        XS.Compare(ECX, EAX);

                        // if they are equal, set [esp] to 1
                        XS.SetByteOnCondition(ConditionalTestEnum.Equal, ESP, destinationIsIndirect: true);

                        // compare high parts
                        XS.Compare(EDX, EBX);

                        // if they are equal, set al to 1
                        XS.SetByteOnCondition(ConditionalTestEnum.Equal, AL);

                        // both high and low parts need to be equal
                        XS.And(ESP, AL, destinationIsIndirect: true);

                        //XS.Pop(EAX);
                        //XS.Compare(EAX, ESP, sourceDisplacement: 4);
                        //XS.Pop(EAX);
                        //XS.Jump(ConditionalTestEnum.NotEqual, Label.LastFullLabel + ".False");
                        //XS.Xor(EAX, ESP, sourceDisplacement: 4);
                        //XS.Jump(ConditionalTestEnum.NotZero, Label.LastFullLabel + ".False");

                        ////they are equal
                        //XS.Add(ESP, 8);
                        //XS.Push(1);
                        //XS.Jump(xNextLabel);
                        //XS.Label(Label.LastFullLabel + ".False");
                        ////not equal
                        //XS.Add(ESP, 8);
                        //XS.Push(0);
                        //XS.Jump(xNextLabel);
                    }
                }
            }
        }
    }
}
