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
			new Label("DebugStub_Ping");

			new Comment("DsVsip.Pong");

			new Comment("X#: AL = 13");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 13 };

			new Comment("X#: ComWriteAL()");
			new Call { DestinationLabel = "DebugStub_ComWriteAL" };

			new Comment("X#: }");
			new Label("DebugStub_Ping_Exit");
			new Return();

			new Comment("X#: procedure TraceOn {");
			new Label("DebugStub_TraceOn");

			new Comment("Tracing.On");

			new Comment("X#: .TraceMode = 1");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_TraceMode"), DestinationIsIndirect = true , SourceValue = 1 };

			new Comment("X#: }");
			new Label("DebugStub_TraceOn_Exit");
			new Return();

			new Comment("X#: procedure TraceOff {");
			new Label("DebugStub_TraceOff");

			new Comment("Tracing.Off");

			new Comment("X#: .TraceMode = 0");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_TraceMode"), DestinationIsIndirect = true , SourceValue = 0 };

			new Comment("X#: }");
			new Label("DebugStub_TraceOff_Exit");
			new Return();

		}
	}
}
