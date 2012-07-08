using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Main : Cosmos.Assembler.Code {

		public Main(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new Comment("X#: procedure HackCompareAsmBreakEIP {");
			new Label("DebugStub_HackCompareAsmBreakEIP");

			new Comment("X#: EAX ?= .AsmBreakEIP");
			new Compare { DestinationReg = RegistersEnum.EAX, SourceIsIndirect = true, SourceRef = Cosmos.Assembler.ElementReference.New("DebugStub_AsmBreakEIP") };

			new Comment("X#: }");
			new Label("DebugStub_HackCompareAsmBreakEIP_Exit");
			new Return();

		}
	}
}
