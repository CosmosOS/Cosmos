using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Screen : Cosmos.Assembler.Code {

		public Screen(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new LiteralAssemblerCode("DebugStub_Const_VidBase equ 0xB8000");
			new LiteralAssemblerCode("DebugStub_Cls:");
			new LiteralAssemblerCode("Mov ESI, DebugStub_Const_VidBase");
			new LiteralAssemblerCode("DebugStub_Cls_Block1Begin:");
			new LiteralAssemblerCode("Cmp ESI, 0xB8FA0");
			new LiteralAssemblerCode("JAE DebugStub_Cls_Block1End");
			new LiteralAssemblerCode("Mov dword [ESI + 0], 0x00");
			new LiteralAssemblerCode("Inc ESI");
			new LiteralAssemblerCode("Mov dword [ESI + 0], 0x0A");
			new LiteralAssemblerCode("Inc ESI");
			new LiteralAssemblerCode("jmp DebugStub_Cls_Block1Begin");
			new LiteralAssemblerCode("DebugStub_Cls_Block1End:");
			new LiteralAssemblerCode("DebugStub_Cls_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("DebugStub_DisplayWaitMsg:");
			new LiteralAssemblerCode("Mov ESI, DebugWaitMsg");
			new LiteralAssemblerCode("Mov EDI, DebugStub_Const_VidBase");
			new LiteralAssemblerCode("Add EDI, 1640");
			new LiteralAssemblerCode("Mov AL, 1");
			new LiteralAssemblerCode("DebugStub_DisplayWaitMsg_Block2Begin:");
			new LiteralAssemblerCode("Cmp AL, 0");
			new LiteralAssemblerCode("JE DebugStub_DisplayWaitMsg_Block2End");
			new LiteralAssemblerCode("Mov AL, [ESI + 0]");
			new LiteralAssemblerCode("Mov [EDI + 0], AL");
			new LiteralAssemblerCode("Inc ESI");
			new LiteralAssemblerCode("Add EDI, 2");
			new LiteralAssemblerCode("jmp DebugStub_DisplayWaitMsg_Block2Begin");
			new LiteralAssemblerCode("DebugStub_DisplayWaitMsg_Block2End:");
			new LiteralAssemblerCode("DebugStub_DisplayWaitMsg_Exit:");
			new LiteralAssemblerCode("Ret");
		}
	}
}
