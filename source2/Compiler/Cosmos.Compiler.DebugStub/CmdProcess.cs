using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class CmdProcess : Cosmos.Assembler.Code {

		public CmdProcess(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new LiteralAssemblerCode("DebugStub_AckCommand:");
			new LiteralAssemblerCode("Mov AL, DebugStub_Const_Ds2Vs_CmdCompleted");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");
			new LiteralAssemblerCode("Mov EAX, [DebugStub_CommandID]");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");
			new LiteralAssemblerCode("DebugStub_AckCommand_Exit:");
			new LiteralAssemblerCode("Ret");
			new LiteralAssemblerCode("DebugStub_ProcessCommandBatch:");
			new LiteralAssemblerCode("DebugStub_ProcessCommandBatch_Begin:");
			new LiteralAssemblerCode("Call DebugStub_ProcessCommand");
			new LiteralAssemblerCode("Cmp AL, 8");
			new LiteralAssemblerCode("JNE DebugStub_ProcessCommandBatch_Begin");
			new LiteralAssemblerCode("Call DebugStub_AckCommand");
			new LiteralAssemblerCode("DebugStub_ProcessCommandBatch_Exit:");
			new LiteralAssemblerCode("Ret");
		}
	}
}
