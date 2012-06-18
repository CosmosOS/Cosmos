using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Screen : Cosmos.Assembler.Code {
		public override void Assemble() {
			new Comment("X#: Group DebugStub");
			new Comment("X#: procedure Cls {");
			new Label("DebugStub_Cls");
			new Comment("X#: # VidBase");
			new Comment("VidBase");
			new Comment("X#: ESI = $B8000");
			new Mov{ DestinationReg = RegistersEnum.ESI, SourceValue = 0xB8000 };
			new Comment("X#: BeginLoop:");
			new Label("DebugStub_Cls_BeginLoop");
			new Comment("X#: # Text");
			new Comment("Text");
			new Comment("X#: AL = $00");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x00 };
			new Comment("X#: ESI[0] = AL");
			new Mov { DestinationReg = RegistersEnum.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0, SourceReg = RegistersEnum.AL};
			new Comment("X#: ESI + 1");
			new INC { DestinationReg = RegistersEnum.ESI };
			new Comment("X#: # Colour");
			new Comment("Colour");
			new Comment("X#: AL = $0A");
			new Mov{ DestinationReg = RegistersEnum.AL, SourceValue = 0x0A };
			new Comment("X#: ESI[0] = AL");
			new Mov { DestinationReg = RegistersEnum.ESI, DestinationIsIndirect = true, DestinationDisplacement = 0, SourceReg = RegistersEnum.AL};
			new Comment("X#: ESI + 1");
			new INC { DestinationReg = RegistersEnum.ESI };
			new Comment("X#: # End of Video Area");
			new Comment("End of Video Area");
			new Comment("X#: # VidBase + 25 * 80 * 2 = B8FA0");
			new Comment("VidBase + 25 * 80 * 2 = B8FA0");
			new Comment("X#: #ESI ? $B8FA0");
			new Comment("ESI ? $B8FA0");
			new Comment("X#: #if < goto BeginLoop");
			new Comment("if < goto BeginLoop");
			new Comment("X#: If (ESI < $B8FA0) goto BeginLoop");
			new Compare { DestinationReg = RegistersEnum.ESI, SourceValue = 0xB8FA0 };
			new ConditionalJump { Condition = ConditionalTestEnum.LessThan, DestinationLabel = "DebugStub_Cls_BeginLoop" };
			new Comment("X#: }");
			new Label("DebugStub_Cls_Exit");
			new Return();
		}
	}
}
