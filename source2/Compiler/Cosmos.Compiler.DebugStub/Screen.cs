using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Screen : Cosmos.Assembler.Code {

		public Screen(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new LiteralAssemblerCode("DebugStub_Cls:");
			new LiteralAssemblerCode("Mov ESI, 0xB8000");
			new LiteralAssemblerCode("DebugStub_Cls_BeginLoop:");
			new LiteralAssemblerCode("Mov AL, 0x00");
			new LiteralAssemblerCode("Mov [ESI + 0], AL");
			new LiteralAssemblerCode("Inc ESI");
			new LiteralAssemblerCode("Mov AL, 0x0A");
			new LiteralAssemblerCode("Mov [ESI + 0], AL");
			new LiteralAssemblerCode("Inc ESI");
			new LiteralAssemblerCode("Cmp ESI, 0xB8FA0");
			new LiteralAssemblerCode("JB DebugStub_Cls_BeginLoop");
			new LiteralAssemblerCode("DebugStub_Cls_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("DebugStub_DisplayWaitMsg:");
			new LiteralAssemblerCode("Mov ESI, DebugWaitMsg");
			new LiteralAssemblerCode("Mov EDI, 0xB8000");
			new LiteralAssemblerCode("Add EDI, 1640");
			new LiteralAssemblerCode("DebugStub_DisplayWaitMsg_ReadChar:");
			new LiteralAssemblerCode("Mov AL, [ESI + 0]");
			new LiteralAssemblerCode("Cmp AL, 0");
			new LiteralAssemblerCode("JE DebugStub_DisplayWaitMsg_Exit");
			new LiteralAssemblerCode("Inc ESI");
			new LiteralAssemblerCode("Mov [EDI + 0], AL");
			new LiteralAssemblerCode("Add EDI, 2");
			new LiteralAssemblerCode("Jmp DebugStub_DisplayWaitMsg_ReadChar");
			new LiteralAssemblerCode("DebugStub_DisplayWaitMsg_Exit:");
			new LiteralAssemblerCode("Ret");
		}
	}
}
