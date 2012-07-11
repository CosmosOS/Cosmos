using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class TracerEntry : Cosmos.Assembler.Code {

		public TracerEntry(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new LiteralAssemblerCode("DebugStub_TracerEntry:");
			new LiteralAssemblerCode("Mov [DebugStub_CallerEBP], EBP");
			new LiteralAssemblerCode("Add ESP, 12");
			new LiteralAssemblerCode("Mov [DebugStub_CallerESP], ESP");
			new LiteralAssemblerCode("Sub ESP, 12");
			new LiteralAssemblerCode("Pushad");
			new LiteralAssemblerCode("Mov [DebugStub_PushAllPtr], ESP");
			new LiteralAssemblerCode("Mov EBP, ESP");
			new LiteralAssemblerCode("Add EBP, 32");
			new LiteralAssemblerCode("Mov EAX, [EBP + 0]");
			new LiteralAssemblerCode("Dec EAX");
			new LiteralAssemblerCode("Mov [DebugStub_CallerEIP], EAX");
			new LiteralAssemblerCode("Call DebugStub_Executing");
			new LiteralAssemblerCode("Popad");
			new LiteralAssemblerCode("DebugStub_TracerEntry_Exit:");
			new LiteralAssemblerCode("IRet");
		}
	}
}
