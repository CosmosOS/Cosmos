using System;
using System.IO;
using CPU = Indy.IL2CPU.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Assembler;

namespace Indy.IL2CPU.IL.X86 {
	[OpCode(OpCodeEnum.Ldelem_Ref)]
	public class Ldelem_Ref: Op {
		public Ldelem_Ref(ILReader aReader, MethodInformation aMethodInfo)
			: base(aReader, aMethodInfo) {
		}

		public static void Assemble(CPU.Assembler aAssembler, uint aElementSize) {
			new CPUx86.Pop{DestinationReg=CPUx86.Registers.EAX};
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EDX, SourceValue = aElementSize };
            new CPUx86.Multiply { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = (ObjectImpl.FieldDataOffset + 4) };
            new CPUx86.Pop { DestinationReg = CPUx86.Registers.EDX };
            new CPUx86.Add { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EAX };
            new CPUx86.Move { DestinationReg = CPUx86.Registers.EAX, SourceReg = CPUx86.Registers.EDX };
			uint xSizeLeft = aElementSize;
			while (xSizeLeft > 0) {
				if (xSizeLeft >= 4) {
                    new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX, DestinationIsIndirect=true};
                    new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 4 };
					xSizeLeft -= 4;
				} else {
					if (xSizeLeft >= 2) {
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
                        new CPUx86.Move { DestinationReg = CPUx86.Registers.CX, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
                        new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                        new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 2 };
						xSizeLeft -= 2;
					} else {
						if (xSizeLeft >= 1) {
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.ECX, SourceValue = 0 };
                            new CPUx86.Move { DestinationReg = CPUx86.Registers.CL, SourceReg = CPUx86.Registers.EAX, SourceIsIndirect = true };
                            new CPUx86.Push { DestinationReg = CPUx86.Registers.ECX };
                            new CPUx86.Add { DestinationReg = CPUx86.Registers.EAX, SourceValue = 1 };
							xSizeLeft -= 1;
						} else {
							throw new Exception("Size left: " + xSizeLeft);
						}
					}
				}
			}
			aAssembler.StackContents.Pop();
			aAssembler.StackContents.Pop();
			aAssembler.StackContents.Push(new StackContent((int)aElementSize, true, false, false));
		}

		public override void DoAssemble() {
			Assemble(Assembler, 4);
		}
	}
}