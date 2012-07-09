using System;
using System.Linq;
using Cosmos.Assembler;
using Cosmos.Assembler.x86;

namespace Cosmos.Debug.DebugStub {
	public class Init : Cosmos.Assembler.Code {

		public Init(Assembler.Assembler aAssembler) : base(aAssembler) {}

		public override void Assemble() {
			new Comment("X#: Group DebugStub");

			new LiteralAssemblerCode(";  Todo: Change these to a group level var");

			new Comment("X#: var .DebugBPs int[256]");
			mAssembler.DataMembers.Add(new DataMember("DebugBPs", new int[256]));

			new Comment("X#: var .DebugWaitMsg = 'Waiting for debugger connection...'");
			mAssembler.DataMembers.Add(new DataMember("DebugWaitMsg", "Waiting for debugger connection..."));

			new LiteralAssemblerCode(";  Called before Kernel runs. Inits debug stub, etc");

			new Comment("X#: procedure Init {");
			new LiteralAssemblerCode("DebugStub_Init:");

			new Comment("X#: Cls()");
			new LiteralAssemblerCode("Call DebugStub_Cls");

			new LiteralAssemblerCode(";  Display message before even trying to init serial");

			new Comment("X#: DisplayWaitMsg()");
			new LiteralAssemblerCode("Call DebugStub_DisplayWaitMsg");

			new Comment("X#: InitSerial()");
			new LiteralAssemblerCode("Call DebugStub_InitSerial");

			new Comment("X#: WaitForDbgHandshake()");
			new LiteralAssemblerCode("Call DebugStub_WaitForDbgHandshake");

			new Comment("X#: Cls()");
			new LiteralAssemblerCode("Call DebugStub_Cls");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_Init_Exit:");
			new LiteralAssemblerCode("Ret");

			new Comment("X#: procedure WaitForSignature {");
			new LiteralAssemblerCode("DebugStub_WaitForSignature:");

			new Comment("X#: EBX = 0");
			new LiteralAssemblerCode("Mov EBX, 0");

			new Comment("X#: Read:");
			new LiteralAssemblerCode("DebugStub_WaitForSignature_Read:");

			new Comment("X#: ComReadAL()");
			new LiteralAssemblerCode("Call DebugStub_ComReadAL");

			new Comment("X#: BL = AL");
			new LiteralAssemblerCode("Mov BL, AL");

			new Comment("X#: EBX ~> 8");
			new LiteralAssemblerCode("ROR EBX, 8");

			new Comment("X#: if EBX != $19740807 goto Read");
			new Compare { DestinationReg = RegistersEnum.EBX, SourceValue = 0x19740807 };
			new ConditionalJump { Condition = ConditionalTestEnum.NotZero, DestinationLabel = "DebugStub_WaitForSignature_Read" };

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_WaitForSignature_Exit:");
			new LiteralAssemblerCode("Ret");

			new LiteralAssemblerCode(";  QEMU (and possibly others) send some garbage across the serial line first.");

			new LiteralAssemblerCode(";  Actually they send the garbage inbound, but garbage could be inbound as well so we");

			new LiteralAssemblerCode(";  keep this.");

			new LiteralAssemblerCode(";  To work around this we send a signature. DC then discards everything before the signature.");

			new LiteralAssemblerCode(";  QEMU has other serial issues too, and we dont support it anymore, but this signature is a good");

			new LiteralAssemblerCode(";  feature so we kept it.");

			new Comment("X#: procedure WaitForDbgHandshake {");
			new LiteralAssemblerCode("DebugStub_WaitForDbgHandshake:");

			new LiteralAssemblerCode(";  \"Clear\" the UART out");

			new Comment("X#: AL = 0");
			new LiteralAssemblerCode("Mov AL, 0");

			new Comment("X#: ComWriteAL()");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");

			new LiteralAssemblerCode(";  Cosmos.Debug.Consts.Consts.SerialSignature");

			new Comment("X#: +$19740807");
			new LiteralAssemblerCode("Push dword 0x19740807");

			new Comment("X#: ESI = ESP");
			new LiteralAssemblerCode("Mov ESI, ESP");

			new LiteralAssemblerCode(";  TODO pass a count register");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new Comment("X#: ComWrite8()");
			new LiteralAssemblerCode("Call DebugStub_ComWrite8");

			new LiteralAssemblerCode(";  Restore ESP, we actually dont care about EAX or the value on the stack anymore.");

			new Comment("X#: -EAX");
			new LiteralAssemblerCode("Pop EAX");

			new LiteralAssemblerCode(";  We could use the signature as the start signal, but I prefer");

			new LiteralAssemblerCode(";  to keep the logic separate, especially in DC.");

			new LiteralAssemblerCode(";  Send the actual started signal");

			new LiteralAssemblerCode(";  Ds2Vs.Started = 6");

			new Comment("X#: AL = 6");
			new LiteralAssemblerCode("Mov AL, 6");

			new Comment("X#: ComWriteAL()");
			new LiteralAssemblerCode("Call DebugStub_ComWriteAL");

			new Comment("X#: WaitForSignature()");
			new LiteralAssemblerCode("Call DebugStub_WaitForSignature");

			new Comment("X#: ProcessCommandBatch()");
			new LiteralAssemblerCode("Call DebugStub_ProcessCommandBatch");

			new Comment("X#: }");
			new LiteralAssemblerCode("DebugStub_WaitForDbgHandshake_Exit:");
			new LiteralAssemblerCode("Ret");

		}
	}
}
