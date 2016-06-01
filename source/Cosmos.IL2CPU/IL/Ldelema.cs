using System;
using System.Linq;
using CPUx86 = Cosmos.Assembler.x86;
using Cosmos.Assembler;
using Cosmos.IL2CPU.ILOpCodes;
using Cosmos.IL2CPU.Plugs.System;

namespace Cosmos.IL2CPU.X86.IL
{
    [Cosmos.IL2CPU.OpCode(ILOpCode.Code.Ldelema)]
    public class Ldelema : ILOp
    {
        public Ldelema(Cosmos.Assembler.Assembler aAsmblr)
            : base(aAsmblr)
        {
        }

        public static void Assemble(Cosmos.Assembler.Assembler aAssembler, OpType aOpType, uint aElementSize, bool debugEnabled)
        {
            new Comment("Arraytype: " + aOpType.StackPopTypes.Last().FullName);
            new Comment("Size: " + aElementSize);

            DoNullReferenceCheck(aAssembler, debugEnabled, 4);
            // calculate element offset into array memory (including header)
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceValue = aElementSize };
            new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)(ObjectImpl.FieldDataOffset + 4) };

            // pop the array now
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            // translate it to actual memory
            new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.RegistersEnum.EDX, SourceIsIndirect = true };

            if (aOpType.StackPopTypes.Last().GetElementType().IsClass)
            {
                new CPUx86.Mov { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.RegistersEnum.EDX, SourceIsIndirect = true };
            }

            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
        }

        public override void Execute(MethodInfo aMethod, ILOpCode aOpCode)
        {
            var xOpType = (OpType)aOpCode;
            var xSize = SizeOfType(xOpType.Value);
            Assemble(Assembler, xOpType, xSize, DebugEnabled);
        }


        // using System;
        // using System.IO;
        //
        //
        // using CPU = Cosmos.Assembler.x86;
        // using CPUx86 = Cosmos.Assembler.x86;
        // using Cosmos.IL2CPU.X86;
        // using Cosmos.IL2CPU.Compiler;
        //
        // namespace Cosmos.IL2CPU.IL.X86 {
        // 	[Cosmos.Assembler.OpCode(OpCodeEnum.Ldelema)]
        // 	public class Ldelema: Op {
        //         private Type mType;
        // 		public Ldelema(ILReader aReader, MethodInformation aMethodInfo)
        // 			: base(aReader, aMethodInfo) {
        // 			mType= aReader.OperandValueType;
        // 		}
        //
        // 		public static void Assemble(CPU.Assembler aAssembler, uint aElementSize) {
        // 			aAssembler.Stack.Pop();
        // 			aAssembler.Stack.Pop();
        // 			aAssembler.Stack.Push(new StackContent(4, typeof(uint)));
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EAX };
        //             new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceValue = aElementSize };
        // 			new CPUx86.Multiply{DestinationReg=CPUx86.Registers.EDX};
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (uint)(ObjectImpl.FieldDataOffset + 4) };
        //             new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
        //             new CPUx86.Add { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
        //             new CPUx86.Push { DestinationReg = CPUx86.Registers.EDX };
        // 		}
        //
        // 		public override void DoAssemble() {
        //             var xElementSize = GetService<IMetaDataInfoService>().SizeOfType(mType);
        // 			Assemble(Assembler, xElementSize);
        // 		}
        // 	}
        // }

    }
}
