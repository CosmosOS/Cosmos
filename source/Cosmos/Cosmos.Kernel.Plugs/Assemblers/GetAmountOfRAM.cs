using System;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;

using Assembler = Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using HW = Cosmos.Hardware;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class GetAmountOfRAM: AssemblerMethod {
		public override void Assemble(Assembler aAssembler) {
            new CPUx86.Move { DestinationReg = Registers.EAX, SourceRef = new ElementReference("MultiBootInfo_Memory_High"), SourceIsIndirect = true };
            new CPUx86.Xor { DestinationReg = CPUx86.Registers.EDX, SourceReg = CPUx86.Registers.EDX };
            new CPUx86.Move { DestinationReg = Registers.ECX, SourceValue = 1024 };
            new CPUx86.Divide { DestinationReg = Registers.ECX };
            new CPUx86.Add { DestinationReg = Registers.EAX, SourceValue = 1 };
            new CPUx86.Push { DestinationReg = CPUx86.Registers.EAX };
		}
	}
}
