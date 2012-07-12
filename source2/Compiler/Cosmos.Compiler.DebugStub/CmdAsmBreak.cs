using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class CmdAsmBreak : Cosmos.Assembler.Code {

		public CmdAsmBreak(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			mAssembler.DataMembers.Add(new DataMember() { RawAsm = "DebugStub_AsmBreakEIP dd 0" });
			mAssembler.DataMembers.Add(new DataMember() { RawAsm = "DebugStub_AsmOrigByte dd 0" });
			new LiteralAssemblerCode("");
			new LiteralAssemblerCode("");
			new LiteralAssemblerCode("");
			new LiteralAssemblerCode("DebugStub_SetAsmBreak:");
			new LiteralAssemblerCode("Call DebugStub_ComReadEAX");
			new LiteralAssemblerCode("Mov EDI, EAX");
			new LiteralAssemblerCode("Mov EAX, [EDI + 0]");
			new LiteralAssemblerCode("Mov [DebugStub_AsmOrigByte], EAX");
			new LiteralAssemblerCode("Mov dword [EDI + 0], 0xCC");
			new LiteralAssemblerCode("Mov [DebugStub_AsmBreakEIP], EDI");
			new LiteralAssemblerCode("DebugStub_SetAsmBreak_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("");
			new LiteralAssemblerCode("DebugStub_ClearAsmBreak:");
			new LiteralAssemblerCode("Mov EDI, [DebugStub_AsmBreakEIP]");
			new LiteralAssemblerCode("Cmp EDI, 0");
			new LiteralAssemblerCode("JE DebugStub_ClearAsmBreak_Exit");
			new LiteralAssemblerCode("");
			new LiteralAssemblerCode("Mov EAX, [DebugStub_AsmOrigByte]");
			new LiteralAssemblerCode("Mov [EDI + 0], EAX");
			new LiteralAssemblerCode("Mov dword [DebugStub_AsmOrigByte], 0");
			new LiteralAssemblerCode("DebugStub_ClearAsmBreak_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("");
		}
	}
}
