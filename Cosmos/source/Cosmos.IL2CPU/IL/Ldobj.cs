using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;

using XSharp.Common;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldobj )]
    public class Ldobj : ILOp
    {
        public Ldobj( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute(_MethodInfo aMethod, ILOpCode aOpCode )
        {
            DoNullReferenceCheck(Assembler, DebugEnabled, 0);
            OpType xType = (OpType)aOpCode;
            DoAssemble(xType.Value);
        }

        public static void DoAssemble(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            XS.Pop(XSRegisters.EAX);
            var xObjSize = GetStorageSize(type);

            switch (xObjSize % 4)
            {
                case 1:
                {
                    XS.Xor(XSRegisters.EBX, XSRegisters.EBX);
                    XS.Set(XSRegisters.BL, XSRegisters.EAX, sourceDisplacement: (int)(xObjSize - 1));
                    //XS.ShiftLeft(XSRegisters.EBX, 24);
                    XS.Push(XSRegisters.EBX);
                    break;
                }
                case 2:
                {
                    XS.Xor(XSRegisters.EBX, XSRegisters.EBX);
                    XS.Set(XSRegisters.BX, XSRegisters.EAX, sourceDisplacement: (int)(xObjSize - 2));
                    //XS.ShiftLeft(XSRegisters.EBX, 16);
                    XS.Push(XSRegisters.EBX);
                    break;
                }
                case 0:
                {
                    break;
                }
                default:
                    throw new Exception("Remainder not supported!");
            }

            xObjSize -= (xObjSize % 4);

            for (int i = 1; i <= (xObjSize / 4); i++)
            {
                new CPUx86.Push {DestinationReg = CPUx86.RegistersEnum.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)(xObjSize - (i * 4))};
            }
        }
    }
}
