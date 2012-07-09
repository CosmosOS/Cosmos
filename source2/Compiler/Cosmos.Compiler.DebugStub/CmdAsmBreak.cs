using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class CmdAsmBreak : Cosmos.Assembler.Code {

		public CmdAsmBreak(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("Location where INT3 has been injected.");

			new Comment("0 if no INT3 is active.");

			new Comment("X#: var AsmBreakEIP");
			mAssembler.DataMembers.Add(new DataMember("DebugStub_AsmBreakEIP", 0));

			new Comment("Old byte before INT3 was injected.");

			new Comment("Only 1 byte is used.");

			new Comment("X#: var AsmOrigByte");
			mAssembler.DataMembers.Add(new DataMember("DebugStub_AsmOrigByte", 0));

			new Comment("X#: procedure SetAsmBreak {");
			new Label("DebugStub_SetAsmBreak");

			new Comment("X#: ComReadEAX()");
			new Call { DestinationLabel = "DebugStub_ComReadEAX" };

			new Comment("X#: EDI = EAX");
			new Mov{ DestinationReg = RegistersEnum.EDI, SourceReg = RegistersEnum.EAX };

			new Comment("Save the old byte");

			new Comment("X#: EAX = EDI[0]");
			new Mov{ DestinationReg = RegistersEnum.EAX, SourceReg = RegistersEnum.EDI, SourceIsIndirect = true, SourceDisplacement = 0 };

			new Comment("X#: .AsmOrigByte = EAX");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_AsmOrigByte"), DestinationIsIndirect = true , SourceReg = RegistersEnum.EAX };

			new Comment("Inject INT3");

			new Comment("X#: EDI[0] = $CC");
			new Mov{ DestinationReg = RegistersEnum.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0, SourceValue = 0xCC };

			new Comment("Save EIP of the break");

			new Comment("X#: .AsmBreakEIP = EDI");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_AsmBreakEIP"), DestinationIsIndirect = true , SourceReg = RegistersEnum.EDI };

			new Comment("X#: }");
			new Label("DebugStub_SetAsmBreak_Exit");
			new Return();

			new Comment("X#: procedure ClearAsmBreak {");
			new Label("DebugStub_ClearAsmBreak");

			new Comment("X#: EDI = .AsmBreakEIP");
			new Mov { DestinationReg = RegistersEnum.EDI , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_AsmBreakEIP"), SourceIsIndirect = true };

			new Comment("If 0, we don't need to clear an older one.");

			new Comment("X#: if EDI = 0 exit");
			new Compare { DestinationReg = RegistersEnum.EDI, SourceValue = 0 };
			new ConditionalJump { Condition = ConditionalTestEnum.Zero, DestinationLabel = "DebugStub_ClearAsmBreak_Exit" };

			new Comment("Clear old break point and set back to original opcode / partial opcode");

			new Comment("X#: EAX = .AsmOrigByte");
			new Mov { DestinationReg = RegistersEnum.EAX , SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_AsmOrigByte"), SourceIsIndirect = true };

			new Comment("X#: EDI[0] = EAX");
			new Mov{ DestinationReg = RegistersEnum.EDI, DestinationIsIndirect = true, DestinationDisplacement = 0, SourceReg = RegistersEnum.EAX };

			new Comment("X#: .AsmOrigByte = 0");
			new Mov { DestinationRef = Cosmos.Assembler.ElementReference.New("DebugStub_AsmOrigByte"), DestinationIsIndirect = true , SourceValue = 0 };

			new Comment("X#: }");
			new Label("DebugStub_ClearAsmBreak_Exit");
			new Return();

		}
	}
}
