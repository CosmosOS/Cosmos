using System;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.Assembler;
using XSharp.Compiler;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode( ILOpCode.Code.Ldobj )]
    public class Ldobj : ILOp
    {
        public Ldobj( Cosmos.Assembler.Assembler aAsmblr )
            : base( aAsmblr )
        {
        }

        public override void Execute( MethodInfo aMethod, ILOpCode aOpCode )
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
            XS.Pop(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EAX));
            var xObjSize = GetStorageSize(type);

            switch (xObjSize % 4)
            {
                case 1:
                {
                    XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
                    new CPUx86.Mov {DestinationReg = CPUx86.RegistersEnum.BL, SourceIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EAX, SourceDisplacement = (int)(xObjSize - 1)};
                    //XS.ShiftLeft(XSRegisters.EBX, 24);
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
                    break;
                }
                case 2:
                {
                    XS.Xor(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX), XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
                    new CPUx86.Mov {DestinationReg = CPUx86.RegistersEnum.BX, SourceIsIndirect = true, SourceReg = CPUx86.RegistersEnum.EAX, SourceDisplacement = (int)(xObjSize - 2)};
                    //XS.ShiftLeft(XSRegisters.EBX, 16);
                    XS.Push(XSRegisters.OldToNewRegister(CPUx86.RegistersEnum.EBX));
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
