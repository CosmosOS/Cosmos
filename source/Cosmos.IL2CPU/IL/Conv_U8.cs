using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler.x86;
using XSharp.Compiler;
using static XSharp.Compiler.XSRegisters;

namespace Cosmos.IL2CPU.X86.IL
{
    /// <summary>
    /// Convert top Stack element to UInt64 and change its type to Int64.
    /// </summary>
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Conv_U8)]
    public class Conv_U8 : ILOp
    {
        public Conv_U8(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xSource = aOpCode.StackPopTypes[0];
            var xSourceSize = SizeOfType(xSource);
            switch (xSourceSize)
            {
                case 1:
                    XS.Pop(EAX);
                    XS.MoveZeroExtend(EAX, AL);
                    XS.Push(0);
                    XS.Push(EAX);
                    break;
                case 2:
                    XS.Pop(EAX);
                    XS.MoveZeroExtend(EAX, AX);
                    XS.Push(0);
                    XS.Push(EAX);
                    break;
                case 4:
                    {
                        if (TypeIsFloat(xSource))
                        {
                            XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Int32);
                            XS.Sub(ESP, 4);
                            XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                        }
                        else
                        {
                            XS.Pop(EAX);
                            XS.Push(0);
                            XS.Push(EAX);
                        }
                        break;
                    }
                case 8:
                    {
                        if (TypeIsFloat(xSource))
                        {
                            XS.FPU.FloatLoad(ESP, destinationIsIndirect: true, size: RegisterSize.Long64);
                            /* The sign of the value should not be changed a negative value is simply converted to its corresponding ulong value */
                            //XS.FPU.FloatAbs();
                            XS.FPU.IntStoreWithTruncate(ESP, isIndirect: true, size: RegisterSize.Long64);
                        }
                        //Else it's already an Int64, or UInt64
                        break;
                    }
                default:
                    throw new NotImplementedException("Cosmos.IL2CPU.x86->IL->Conv_U8.cs->Unknown size of variable on the top of the stack.");
            }
        }
    }
}
