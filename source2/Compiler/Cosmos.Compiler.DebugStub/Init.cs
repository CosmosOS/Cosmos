using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Init : Cosmos.Assembler.Code {
		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("Called before Kernel runs. Inits debug stub, etc");

			new Comment("X#: procedure Init {");
			new Label("DebugStub_Init");

			new Comment("X#: Call .Cls");
			new Call { DestinationLabel = "DebugStub_Cls" };

			new Comment("X#: Call .DisplayWaitMsg");
			new Call { DestinationLabel = "DebugStub_DisplayWaitMsg" };

			new Comment("X#: Call .InitSerial");
			new Call { DestinationLabel = "DebugStub_InitSerial" };

			new Comment("X#: Call .WaitForDbgHandshake");
			new Call { DestinationLabel = "DebugStub_WaitForDbgHandshake" };

			new Comment("X#: Call .Cls");
			new Call { DestinationLabel = "DebugStub_Cls" };

			new Comment("X#: }");
			new Label("DebugStub_Init_Exit");
			new Return();

		}
	}
}
