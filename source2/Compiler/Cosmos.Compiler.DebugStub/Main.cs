using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Main : Cosmos.Assembler.Code {

		public Main(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new LiteralAssemblerCode("DebugStub_HackCompareAsmBreakEIP:");
			new LiteralAssemblerCode("Cmp EAX, DebugStub_AsmBreakEIP");
			new LiteralAssemblerCode("DebugStub_HackCompareAsmBreakEIP_Exit:");
			new LiteralAssemblerCode("Ret");
		}
	}
}
