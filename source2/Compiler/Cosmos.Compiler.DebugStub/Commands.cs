using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Commands : Cosmos.Assembler.Code {

		public Commands(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("X#: procedure Ping {");
			new Label("DebugStub_Ping");

			new Comment("DsVsip.Pong");

			new Comment("X#: AL = 13");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 13 };

			new Comment("X#: Call .ComWriteAL");
			new Call { DestinationLabel = "DebugStub_ComWriteAL" };

			new Comment("X#: }");
			new Label("DebugStub_Ping_Exit");
			new Return();

		}
	}
}
