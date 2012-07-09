using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class CmdMisc : Cosmos.Assembler.Code {

		public CmdMisc(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("X#: procedure Ping {");
			new LiteralAssemblerCode("DebugStub_Ping:");

			new LiteralAssemblerCode("; Ds2Vs.Pong");

			new Comment("X#: AL = 13");
			new LiteralAssemblerCode("Mov AL, 13");

			new Comment("X#: ComWriteAL()");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_Ping_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure TraceOn {");
			new LiteralAssemblerCode("DebugStub_TraceOn:");

			new LiteralAssemblerCode("; Tracing.On");

			new Comment("X#: .TraceMode = 1");
			new LiteralAssemblerCode("Mov dword [DebugStub_TraceMode], 1");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_TraceOn_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure TraceOff {");
			new LiteralAssemblerCode("DebugStub_TraceOff:");

			new LiteralAssemblerCode("; Tracing.Off");

			new Comment("X#: .TraceMode = 0");
			new LiteralAssemblerCode("Mov dword [DebugStub_TraceMode], 0");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_TraceOff_Exit:");
			new LiteralAssemblerCode("Ret");

		}
	}
}
