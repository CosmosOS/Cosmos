using System;
using System.IO;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Stelem_Ref)]
	public class Stelem_Ref: Op {
		public Stelem_Ref(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		public static void Assemble(CPU.Assembler aAssembler, uint aElementSize) {
			// stack - 3 == the array
			// stack - 2 == the index
			// stack - 1 == the new value
			uint xStackSize = aElementSize;
			if (xStackSize % 4 != 0) {
				xStackSize += 4 - xStackSize % 4;
			}
            new CPUx86.Push { DestinationReg = CPUx86.Registers.ESP, DestinationIsIndirect = true, DestinationDisplacement = (int)(xStackSize + 4) };
			Engine.QueueMethod(GCImplementationRefs.DecRefCountRef);
            new CPUx86.Call { DestinationLabel = IL2CPU.Assembler.Label.GenerateLabelName(GCImplementationRefs.DecRefCountRef) };
			new CPUx86.Move{DestinationReg=CPUx86.Registers.EBX, SourceReg=CPUx86.Registers.ESP, SourceIsIndirect=true, SourceDisplacement= (int)xStackSize}; // the index
            new CPUx86.Move{DestinationReg=CPUx86.Registers.ECX, SourceReg=CPUx86.Registers.ESP, SourceIsIndirect=true, SourceDisplacement= (int)xStackSize+4}; // the index
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ECX, SourceValue = (uint)(ObjectImpl.FieldDataOffset + 4) };
            new CPUx86.Push { DestinationValue = aElementSize };
			aAssembler.StackContents.Push(new StackContent(4, true, false, true));
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EBX };
			aAssembler.StackContents.Push(new StackContent(4, true, false, true));
			Multiply(aAssembler);
			new CPUx86.Push{DestinationReg=CPUx86.Registers.ECX};
			aAssembler.StackContents.Push(new StackContent(4, true, false, true));
			Add(aAssembler);
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.ECX };
			aAssembler.StackContents.Pop();
			for (int i = (int)(aElementSize / 4) - 1; i >= 0; i -= 1) {
				new CPU.Comment("Start 1 dword");
                new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.EBX };
                new CPUx86.Add { DestinationReg = CPUx86.Registers.ECX, SourceValue = 4 };
			}
			switch (aElementSize % 4) {
				case 1: {
						new CPU.Comment("Start 1 byte");
                        new CPUx86.Pop { DestinationReg = CPUx86.Registers.EBX };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.BL };
						break;
					}
				case 2: {
						new CPU.Comment("Start 1 word");
						new CPUx86.Pop{DestinationReg = CPUx86.Registers.EBX};
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, DestinationIsIndirect = true, SourceReg = CPUx86.Registers.BX };
						break;
					}
				case 0: {
						break;
					}
				default:
					throw new Exception("Remainder size " + (aElementSize % 4) + " not supported!");

			}
            new CPUx86.Add { DestinationReg = CPUx86.Registers.ESP, SourceValue = 0x8 };
			aAssembler.StackContents.Pop();
			aAssembler.StackContents.Pop();
			aAssembler.StackContents.Pop();
		}

		public override void DoAssemble() {
			Assemble(Assembler, 4);
		}
	}
}