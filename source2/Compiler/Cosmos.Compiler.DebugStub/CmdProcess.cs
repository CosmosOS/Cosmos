using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class CmdProcess : Cosmos.Assembler.Code {

		public CmdProcess(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("X#: procedure AckCommand {");
			new LiteralAssemblerCode("DebugStub_AckCommand:");

			new LiteralAssemblerCode(";  We acknowledge receipt of the command AND the processing of it.");

			new LiteralAssemblerCode(";    -In the past the ACK only acknowledged receipt.");

			new LiteralAssemblerCode(";  We have to do this because sometimes callers do more processing.");

			new LiteralAssemblerCode(";  We ACK even ones we dont process here, but do not ACK Noop.");

			new LiteralAssemblerCode(";  The buffers should be ok because more wont be sent till after our NACK");

			new LiteralAssemblerCode(";  is received.");

			new LiteralAssemblerCode(";  Right now our max cmd size is 2 (Cmd + Cmd ID) + 5 (Data) = 7.");

			new LiteralAssemblerCode(";  UART buffer is 16.");

			new LiteralAssemblerCode(";  We may need to revisit this in the future to ack not commands, but data chunks");

			new LiteralAssemblerCode(";  and move them to a buffer.");

			new LiteralAssemblerCode(";  The buffer problem exists only to inbound data, not outbound data (relative to DebugStub).");

			new Comment("X#: AL = #Ds2Vs_CmdCompleted");
			new LiteralAssemblerCode("Mov AL, DebugStub_Const_Ds2Vs_CmdCompleted");

			new Comment("X#: ComWriteAL()");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");

			new Comment("X#: EAX = .CommandID");
			new LiteralAssemblerCode("Mov EAX, [DebugStub_CommandID]");

			new Comment("X#: ComWriteAL()");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_AckCommand_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure ProcessCommandBatch {");
			new LiteralAssemblerCode("DebugStub_ProcessCommandBatch:");

			new Comment("X#: Begin:");
			new LiteralAssemblerCode("DebugStub_ProcessCommandBatch_Begin:");

			new Comment("X#: ProcessCommand()");
			new LiteralAssemblerCode("Call DebugStub_ProcessCommand");

			new LiteralAssemblerCode(";  See if batch is complete");

			new LiteralAssemblerCode(";  Loop and wait");

			new LiteralAssemblerCode(";  Vs2Ds.BatchEnd");

			new Comment("X#: if AL != 8 goto Begin");
			new LiteralAssemblerCode("Cmp AL, 8");
			new LiteralAssemblerCode("JNE DebugStub_ProcessCommandBatch_Begin");

			new Comment("X#: AckCommand()");
			new LiteralAssemblerCode("Call DebugStub_AckCommand");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_ProcessCommandBatch_Exit:");
			new LiteralAssemblerCode("Ret");

		}
	}
}
