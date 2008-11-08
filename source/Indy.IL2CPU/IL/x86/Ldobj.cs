using System;
using Indy.IL2CPU.Assembler.X86;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
    [OpCode(OpCodeEnum.Ldobj)]
    public class Ldobj : Op {
        private Type xType;

        public Ldobj(ILReader aReader,
                     MethodInformation aMethodInfo)
            : base(aReader,
                   aMethodInfo) {
            xType = aReader.OperandValueType;
            if (xType == null) {
                throw new Exception("Type specification not found!");
            }
            mSize = Engine.GetFieldStorageSize(xType);
        }

        private uint mSize;

        public override void DoAssemble() {
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            for (int i = 1; i <= (mSize / 4); i++) {
                new CPUx86.Push { DestinationReg = Registers.EAX, DestinationIsIndirect = true, DestinationDisplacement = (int)(mSize - (i * 4)) };
            }
            switch (mSize % 4) {
                case 1: {
                        new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.BL, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                    new CPUx86.Push { DestinationReg = Registers.EBX };
                    break;
                }
                case 2: {
                        new CPUx86.Xor { DestinationReg = CPUx86.Registers.EBX, SourceReg = CPUx86.Registers.EBX };
                    new CPUx86.Move { DestinationReg = CPUx86.Registers.BX, SourceIsIndirect = true, SourceReg = CPUx86.Registers.EAX };
                    new CPUx86.Push{DestinationReg=Registers.EBX};
                    break;
                }
                case 0: {
                    break;
                }
                default: {
                    throw new Exception("Remainder not supported!");
                }
            }
            Assembler.StackContents.Pop();
            Assembler.StackContents.Push(new StackContent((int)mSize,
                                                          xType));
        }
    }
}