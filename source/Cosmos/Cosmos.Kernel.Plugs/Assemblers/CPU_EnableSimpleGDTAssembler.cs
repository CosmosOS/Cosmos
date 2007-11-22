using System;
using Indy.IL2CPU.Assembler;
using Indy.IL2CPU.Assembler.X86;
using Indy.IL2CPU.Plugs;
using Assembler=Indy.IL2CPU.Assembler.Assembler;
using CPUx86 = Indy.IL2CPU.Assembler.X86;
using CPUNative = Indy.IL2CPU.Assembler.X86.Native;

namespace Cosmos.Kernel.Plugs.Assemblers {
	public class CPU_EnableSimpleGDTAssembler: BaseMethodAssembler {
		public override void Assemble(Assembler aAssembler) {
			string xFieldName = "_NATIVE_GDT_Contents";
			string xFieldData = "0,0,0,0,0,0,0,0,"; // system entry, all zeros
			// code entry
			xFieldData += "0xFF,0xFF,0,0,0,0x99,0xCF,0,";
			// data entry
			xFieldData += "0xFF,0xFF,0,0,0,0x93,0xCF,0";
			aAssembler.DataMembers.Add(new DataMember(xFieldName, "db", xFieldData));
			xFieldName = "_NATIVE_GDT_Pointer";
			xFieldData = "0x17,(_NATIVE_GDT_Contents and 0xFFFF),(_NATIVE_GDT_Contents shr 16)";
			aAssembler.DataMembers.Add(new DataMember(xFieldName, "dw", xFieldData));
			new CPUx86.Move(Registers.EAX, "_NATIVE_GDT_Pointer");
			new CPUNative.Lgdt(Registers.AtEAX);
			new CPUNative.Break();
			new CPUx86.Move(Registers.AX, "0x10");
			new CPUx86.Move("ds", "ax");
			new CPUx86.Move("es", "ax");
			new CPUx86.Move("fs", "ax");
			new CPUx86.Move("gs", "ax");
			new CPUx86.Move("ss", "ax");
			new CPUx86.JumpAlways("0x8:flush__GDT__table");
			new Label("flush__GDT__table");
		}
	}
}
