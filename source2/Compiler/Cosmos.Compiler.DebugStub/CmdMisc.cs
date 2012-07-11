using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class CmdMisc : Cosmos.Assembler.Code {

		public CmdMisc(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new LiteralAssemblerCode("DebugStub_Ping:");
			new LiteralAssemblerCode("Mov AL, 13");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");
			new LiteralAssemblerCode("DebugStub_Ping_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("DebugStub_TraceOn:");
			new LiteralAssemblerCode("Mov dword [DebugStub_TraceMode], 1");
			new LiteralAssemblerCode("DebugStub_TraceOn_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("DebugStub_TraceOff:");
			new LiteralAssemblerCode("Mov dword [DebugStub_TraceMode], 0");
			new LiteralAssemblerCode("DebugStub_TraceOff_Exit:");
			new LiteralAssemblerCode("Ret");
		}
	}
}
