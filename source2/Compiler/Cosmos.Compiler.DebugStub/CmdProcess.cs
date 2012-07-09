using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class CmdProcess : Cosmos.Assembler.Code {

		public CmdProcess(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("test when emitted after usage too");

			new Comment("X#: ! DebugStub_Const_DsVsip_CmdCompleted equ 9");
			new LiteralAssemblerCode("DebugStub_Const_DsVsip_CmdCompleted equ 9");

			new Comment("X#: procedure AckCommand {");
			new Label("DebugStub_AckCommand");

			new Comment("We acknowledge receipt of the command AND the processing of it.");

			new Comment("-In the past the ACK only acknowledged receipt.");

			new Comment("We have to do this because sometimes callers do more processing.");

			new Comment("We ACK even ones we dont process here, but do not ACK Noop.");

			new Comment("The buffers should be ok because more wont be sent till after our NACK");

			new Comment("is received.");

			new Comment("Right now our max cmd size is 2 (Cmd + Cmd ID) + 5 (Data) = 7.");

			new Comment("UART buffer is 16.");

			new Comment("We may need to revisit this in the future to ack not commands, but data chunks");

			new Comment("and move them to a buffer.");

			new Comment("The buffer problem exists only to inbound data, not outbound data (relative to DebugStub).");

			new Comment("DsVsip.CmdCompleted");

			new Comment("X#: AL = #DsVsip_CmdCompleted");
			new Mov {DestinationReg = RegistersEnum.AL , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_Const_DsVsip_CmdCompleted") };

			new Comment("AL = 9");

			new Comment("X#: ComWriteAL()");
			new Call { DestinationLabel = "DebugStub_ComWriteAL" };

			new Comment("X#: EAX = .CommandID");
			new Mov { DestinationReg = RegistersEnum.EAX , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_CommandID"), SourceIsIndirect = true };

			new Comment("X#: ComWriteAL()");
			new Call { DestinationLabel = "DebugStub_ComWriteAL" };

			new Comment("X#: }");
			new Label("DebugStub_AckCommand_Exit");
			new Return();

			new Comment("X#: procedure ProcessCommandBatch {");
			new Label("DebugStub_ProcessCommandBatch");

			new Comment("X#: Begin:");
			new Label("DebugStub_ProcessCommandBatch_Begin");

			new Comment("X#: ProcessCommand()");
			new Call { DestinationLabel = "DebugStub_ProcessCommand" };

			new Comment("See if batch is complete");

			new Comment("Loop and wait");

			new Comment("VsipDs.BatchEnd");

			new Comment("X#: if AL != 8 goto Begin");
			new Compare { DestinationReg = RegistersEnum.AL, SourceValue = 8 };
			new ConditionalJump { Condition = ConditionalTestEnum.NotZero, DestinationLabel = "DebugStub_ProcessCommandBatch_Begin" };

			new Comment("X#: AckCommand()");
			new Call { DestinationLabel = "DebugStub_AckCommand" };

			new Comment("X#: }");
			new Label("DebugStub_ProcessCommandBatch_Exit");
			new Return();

		}
	}
}
