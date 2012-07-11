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
			new Comment("X#: if EBX != #Signature goto Read");
			new LiteralAssemblerCode("Cmp EBX, DebugStub_Const_Signature");
			new LiteralAssemblerCode("JNE DebugStub_WaitForSignature_Read");
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
			new Comment("X#: +#Signature");

Unknown tokens found.
